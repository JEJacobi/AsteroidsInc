using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace AsteroidsInc.Components
{
    public sealed class AsteroidManager
    {
        #region Declarations

        public List<GameObject> Asteroids; //main list of GameObjects
        Vector2 lastCollisionIndex = new Vector2(-1, -1); //for keeping track of stuck objects
        public bool RegenerateAsteroids { get; set; } //regen asteroids?

        public readonly int InitialAsteroids; //initally spawning asteroids
        public readonly float MinVelocity;
        public readonly float MaxVelocity;
        public readonly float MinRotationalVelocity; //in degrees
        public readonly float MaxRotationalVelocity; //in degrees
        public readonly List<Texture2D> Textures;
        public readonly List<Texture2D> ExplosionParticleTextures;

        List<ParticleEmitter> emitters; //list of particle emitters, used for effects
        Random rnd; //general random number generator

        #endregion

        #region Constants
        //Draw depths
        const float EFFECT_DRAW_DEPTH = 0.8f;
        const float ASTEROID_DRAW_DEPTH = 0.5f;

        //Scrape particle configuration
        const int SCRAPE_PARTICLES = 4;
        const int SCRAPE_FRAMES_TO_LIVE = 120;
        const float SCRAPE_EJECTION_SPEED = 20f;
        const float SCRAPE_SPRAY = 20f;

        //Explosion particle configuration
        const int EXPLOSION_PARTICLES = 30;
        const int EXPLOSION_FRAMES_TO_LIVE = 250;
        const float EXPLOSION_EJECTION_SPEED = 20f;

        //Misc particle emission stuff
        const int MAX_TRIES = 50;
        const int PARTICLE_TIME_TO_EMIT = 1;
        const float PARTICLERANDOMIZATION = 2f;

        //Texture key constants
        public const string SMALL_ASTEROID = "smallAsteroid";
        public const string LARGE_ASTEROID = "largeAsteroid";
        public const string ORE_ASTEROID = "oreAsteroid";

        //Effect colors
        readonly Color[] SCRAPE_COLORS = { Color.Gray, Color.DimGray, Color.LightSlateGray, Color.SandyBrown, Color.RosyBrown };
        readonly Color[] EXPLOSION_COLORS = { Color.LawnGreen, Color.White, Color.LightSlateGray, Color.LightGray };

        public const int LARGE_ASTEROID_DAMAGE_THRESHOLD = 25;

        #endregion

        public AsteroidManager(
            int initialAsteroids,
            float minVel,
            float maxVel,
            float minRotVel,
            float maxRotVel,
            List<Texture2D> textures,
            List<Texture2D> explosionParticleTextures,
            bool regenAsteroids)
        {
            rnd = new Random(); //randomize the randomizer

            InitialAsteroids = initialAsteroids;
            MinVelocity = minVel;
            MaxVelocity = maxVel;
            MinRotationalVelocity = minRotVel;
            MaxRotationalVelocity = maxRotVel;
            Textures = textures;
            ExplosionParticleTextures = explosionParticleTextures;
            RegenerateAsteroids = regenAsteroids;

            //init the collections
            Asteroids = new List<GameObject>();
            emitters = new List<ParticleEmitter>();

            for (int i = 0; i < InitialAsteroids; i++)
                AddRandomAsteroid(); //initial placement of asteroids
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < Asteroids.Count; i++)
                Asteroids[i].Update(gameTime); //update the asteroids themselves

            for (int i = 0; i < emitters.Count; i++)
            {
                emitters[i].Update(gameTime); //update all the explosion/scrape particle emitters
                if (emitters[i].TimeToEmit == 0 && emitters[i].Particles.Count == 0)
                    emitters.RemoveAt(i);
            }

            if (Asteroids.Count < InitialAsteroids && RegenerateAsteroids)
                AddRandomAsteroid(true); //regenerate asteroid offscreen

            for (int x = 0; x < Asteroids.Count; x++) //Pretty much brute-forcing the collision detection
            {
                for (int y = x + 1; y < Asteroids.Count; y++) //eck n^2
                    if (Asteroids[x].IsCircleColliding(Asteroids[y]))
                    {
                        GameObjectPair temp = GameObject.Bounce(Asteroids[x], Asteroids[y]); //get pair of objects
                        Asteroids[x] = temp.Object1; //and assign
                        Asteroids[y] = temp.Object2;

                        //if the same asteroids are colliding twice in a row, and a collision has been recorded
                        if (lastCollisionIndex.X != -1 &&
                            lastCollisionIndex.Y != -1 &&
                            lastCollisionIndex.X == x &&
                            lastCollisionIndex.Y == y)
                        {
                            if (Camera.IsObjectVisible(temp.Object1.BoundingBox) == false)
                            {
                                DestroyAsteroid(temp.Object1, false); //silently blow up the first one
                            }
                            else if (Camera.IsObjectVisible(temp.Object2.BoundingBox) == false)
                            {
                                DestroyAsteroid(temp.Object2, false); //silently blow up the second one
                            }
                            else
                            {
                                DestroyAsteroid(temp.Object2, true); //if player is seeing both, no choice but to blow it up
                            }
                            AddRandomAsteroid(true);
                        }

                        addScrapeEffect(temp);
                        lastCollisionIndex = new Vector2(x, y);
                    }

                Projectile p;
                if (ProjectileManager.IsHit(Asteroids[x], out p))
                {
                    if (Asteroids[x].Texture == ContentHandler.Textures[LARGE_ASTEROID]) //if it's a big asteroid
                    {
                        if (p.Damage >= LARGE_ASTEROID_DAMAGE_THRESHOLD)
                        {
                            DestroyAsteroid(Asteroids[x], true);
                            ProjectileManager.PlayShotHitSound(p);
                            break;
                        }
                        else
                            break;
                    }
                    else
                    {
                        DestroyAsteroid(Asteroids[x], true);
                        ProjectileManager.PlayShotHitSound(p);
                        break;
                    }
                }

                if(Asteroids[x].IsCircleColliding(Player.Ship))
                {
                    Player.Health -= Player.ASTEROID_COLLISION_DAMAGE; //add damage
                    Player.Ship = GameObject.Bounce(Player.Ship, Asteroids[x]).Object1; //bounce the ship
                    Player.TriggerShield(); //activate the shield effect

                    Player.Ship.RotationVelocityDegrees = (float)rnd.NextDouble(
                        -Player.ROT_VEL_BOUNCE_CHANGE, Player.ROT_VEL_BOUNCE_CHANGE);

                    DestroyAsteroid(Asteroids[x], true); //and blow up the asteroid

                    //ContentHandler.PlaySFX(Player.COLLISION_SFX);
                    break;
                }
            }


        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < Asteroids.Count; i++)
            {
                Asteroids[i].Draw(spriteBatch);

                //TEMPORARY - FOR DEBUG
                spriteBatch.DrawString(ContentHandler.Fonts["lcd"],
                    i.ToString(),
                    Camera.GetLocalCoords(Asteroids[i].WorldLocation),
                    Color.White);
            }

            for (int i = 0; i < emitters.Count; i++)
                emitters[i].Draw(spriteBatch);
        }

        public void DestroyAsteroid(GameObject asteroid, bool effect = true)
        {
            int x = rnd.Next(50); //temp var

            //around 50% chance of splitting if asteroid is a large non-ore asteroid
            if (x > 25 && asteroid.Texture == ContentHandler.Textures["largeAsteroid"])
                splitAsteroid(asteroid);

            if(effect) //add a particle effect if flag is true
                addExplosion(asteroid.WorldCenter);

            Asteroids.Remove(asteroid); //remove the asteroid from the list
        }

        public void AddRandomAsteroid(bool offScreen = false) //add a random asteroid, param for offscreen generation
        {
            AsteroidType temp = AsteroidType.Small; //temporary variables
            int x = rnd.Next(100); //get a random number between 0 and 100

            if (x >= 0 && x < 33) //if 0-32, asteroid is small
                temp = AsteroidType.Small;
            if (x >= 33 && x < 66) //if 33-65, asteroid is big
                temp = AsteroidType.Large;
            if (x >= 66 && x <= 100) //if 66-100, asteroid is ore
                temp = AsteroidType.Ore;

            AddAsteroid(temp, offScreen); //call overload of self
        }

        public void AddAsteroid(AsteroidType type, bool offScreen = false) //add a specified type asteroid
        {
            GameObject tempAsteroid; //temporary asteroid
            bool isOverlap = false; //overlap flag
            int counter = 0; //generation counter
            Texture2D text; //temporary texture
            switch (type) //assign appropriate texture to asteroid type
            {
                case AsteroidType.Small: text = ContentHandler.Textures[SMALL_ASTEROID];
                    break;
                case AsteroidType.Large: text = ContentHandler.Textures[LARGE_ASTEROID];
                    break;
                case AsteroidType.Ore: text = ContentHandler.Textures[ORE_ASTEROID];
                    break;
                default:
                    throw new InvalidOperationException();
            }

            do //Do-While to ensure that the asteroid gets generated at least once
            {
                float rot = MathHelper.ToRadians((float)rnd.NextDouble(0f, 359f)); //random rotation

                //random rotational velocity; within constants
                float rotVel = MathHelper.ToRadians((float)rnd.NextDouble(MinRotationalVelocity, MaxRotationalVelocity));

                int colRadius = text.GetMeanRadius();

                Vector2 vel; //temporary velocity
                Vector2 worldPos; //temporary worldposition

                if (offScreen) //if asteroid needs to be generated offscreen
                {
                    SpawnSide side = Util.RandomEnumValue<SpawnSide>(); //pick a random world border to spawn it from
                    switch (side)
                    {
                        case SpawnSide.Up:
                            vel = getVelocity(MathHelper.ToRadians((float)rnd.Next(140, 230))); //get a velocity pointing between 140/230 degrees

                            worldPos = new Vector2( //and add a velocity which will knock it into the world borders next tick
                                rnd.Next(Camera.WorldRectangle.X, Camera.WorldRectangle.Width),
                                Camera.WorldRectangle.Y - text.Height);
                            break;
                        case SpawnSide.Left:
                            vel = getVelocity(MathHelper.ToRadians((float)rnd.Next(50, 130))); //between 50/130 degrees

                            worldPos = new Vector2(
                                Camera.WorldRectangle.X - text.Width,
                                rnd.Next(Camera.WorldRectangle.Y, Camera.WorldRectangle.Height));
                            break;
                        case SpawnSide.Right:
                            vel = getVelocity(MathHelper.ToRadians((float)rnd.Next(230, 310))); //between 230/310 degrees

                            worldPos = new Vector2(
                                Camera.WorldRectangle.Width + text.Width,
                                rnd.Next(Camera.WorldRectangle.Y, Camera.WorldRectangle.Height));
                            break;
                        case SpawnSide.Down:
                            vel = getVelocity(
                                MathHelper.ToRadians(rnd.Next(320, 410) % 360)); //between 320/(360 + 50) degrees

                            worldPos = new Vector2(
                                rnd.Next(Camera.WorldRectangle.X, Camera.WorldRectangle.Width),
                                Camera.WorldRectangle.Height + text.Height);
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
                else //if the asteroid does not need to be generated offscreen...
                {
                    vel = getVelocity(rot); //get a random velocity according to the rotation

                    worldPos = new Vector2( //and simply get a random position in the world to place it...
                        rnd.Next(Camera.WorldRectangle.X, Camera.WorldRectangle.Width),
                        rnd.Next(Camera.WorldRectangle.Y, Camera.WorldRectangle.Height));
                }

                tempAsteroid = new GameObject( //init a temporary asteroid to check for overlaps
                    text, worldPos, vel, Color.White, false, rot, rotVel, 1f, ASTEROID_DRAW_DEPTH, text.GetMeanRadius());

                foreach (GameObject asteroid in Asteroids)
                {
                    if (tempAsteroid.GetRect.Intersects(asteroid.GetRect)) //TEMP - GetRect
                    {
                        isOverlap = true; //flag if overlapping
                        break; //and break; no need to check all other asteroids
                    }
                }
                counter++; //increase counter

            } while (isOverlap && counter < MAX_TRIES); //if overlapping, loop, if maxtries exceeded, quit

            if (counter >= MAX_TRIES)
            {
                return;
            }
            else
            {
                Asteroids.Add(tempAsteroid); //add the temp asteroid if not at maxtries
            }
        }

        #region Local Methods

        void splitAsteroid(GameObject asteroidToSplit) //splits a destroyed large asteroid into a small one
        {
            //split asteroid into small, inherting most of the characteristics
            GameObject split = new GameObject(
                ContentHandler.Textures[SMALL_ASTEROID],
                asteroidToSplit.WorldCenter,
                getVelocity(asteroidToSplit.RotationDegrees + rnd.Next(90, 270)), //get a random new velocity
                asteroidToSplit.TintColor,
                false, //nothing to animate
                asteroidToSplit.Rotation,
                asteroidToSplit.RotationalVelocity,
                asteroidToSplit.Scale,
                asteroidToSplit.Depth,
                ContentHandler.Textures[SMALL_ASTEROID].GetMeanRadius()); //get rounded radius

            Asteroids.Add(split);
        }

        void addExplosion(Vector2 point) //add an omni-directional particle burst at specified point
        {
            ParticleEmitter temp = new ParticleEmitter(
                EXPLOSION_PARTICLES, //random amount of particles
                point, //at the point
                ExplosionParticleTextures,
                EXPLOSION_COLORS.ToList<Color>(), //colors to list
                EXPLOSION_FRAMES_TO_LIVE,
                true,
                true,
                PARTICLE_TIME_TO_EMIT,
                EXPLOSION_PARTICLES / PARTICLE_TIME_TO_EMIT, //emit max particles in one tick
                EXPLOSION_EJECTION_SPEED,
                PARTICLERANDOMIZATION,
                0f, //no direction needed
                ParticleEmitter.EXPLOSIONSPRAY); //360 degree explosion

            temp.ParticleDrawDepth = EFFECT_DRAW_DEPTH;

            emitters.Add(temp);
        }

        void addScrapeEffect(GameObjectPair objects) //add a bi-direction particle spread at the center of a pair of objects
        {
            Vector2 normal = GameObject.GetNormal(objects);

            //first emitter
            ParticleEmitter temp1 = new ParticleEmitter(
                SCRAPE_PARTICLES,
                objects.CenterPoint(), //use center of both as reference
                ExplosionParticleTextures,
                SCRAPE_COLORS.ToList<Color>(),
                SCRAPE_FRAMES_TO_LIVE,
                true,
                true, //fading enabled
                PARTICLE_TIME_TO_EMIT,
                SCRAPE_PARTICLES / PARTICLE_TIME_TO_EMIT,
                SCRAPE_EJECTION_SPEED,
                PARTICLERANDOMIZATION,
                MathHelper.ToDegrees(normal.RotateTo()), //get the velocity angle + 90 degrees
                SCRAPE_SPRAY);

            //second emitter
            ParticleEmitter temp2 = new ParticleEmitter(
                SCRAPE_PARTICLES,
                objects.CenterPoint(),
                ExplosionParticleTextures,
                SCRAPE_COLORS.ToList<Color>(),
                SCRAPE_FRAMES_TO_LIVE,
                true,
                true,
                PARTICLE_TIME_TO_EMIT,
                SCRAPE_PARTICLES / PARTICLE_TIME_TO_EMIT,
                SCRAPE_EJECTION_SPEED,
                PARTICLERANDOMIZATION,
                MathHelper.ToDegrees(normal.RotateTo() + 180),
                SCRAPE_SPRAY);

            //inherit velocities
            Vector2 temp = objects.Object1.Velocity.Center(objects.Object2.Velocity);
            temp1.VelocityToInherit = temp;
            temp2.VelocityToInherit = temp;

            //set depths
            temp1.ParticleDrawDepth = EFFECT_DRAW_DEPTH;
            temp2.ParticleDrawDepth = EFFECT_DRAW_DEPTH;

            emitters.Add(temp1);
            emitters.Add(temp2);
        }

        private Vector2 getVelocity(float rot) //get a random velocity
        {
            return Vector2.Multiply(
                rot.RotationToVector(),
                (float)rnd.NextDouble(MinVelocity, MaxVelocity));
        }

        #endregion

        #region Enums

        public enum AsteroidType
        {
            Small,
            Large, //chance of splitting into a small one on destruction
            Ore
        }

        enum SpawnSide
        {
            Up,
            Left,
            Right,
            Down
        }

        #endregion
    }
}
