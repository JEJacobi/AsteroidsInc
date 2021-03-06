﻿using System;
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
    public static class AsteroidManager
    {
        #region Declarations

        public static List<GameObject> Asteroids; //main list of GameObjects
        static Vector2 lastCollisionIndex = new Vector2(-1, -1); //for keeping track of stuck objects
        public static bool RegenerateAsteroids { get; set; } //regen asteroids?
        public static int AsteroidsToGenerate; //initally spawning asteroids

        public static List<Texture2D> AsteroidTextures;
        public static List<Texture2D> ExplosionParticleTextures;

        static List<ParticleEmitter> emitters; //list of particle emitters, used for effects

        #endregion

        #region Constants

        const int genRandomization = 5;

        //Asteroid config
        public const float MINVELOCITY = 50f;
        public const float MAXVELOCITY = 100f;
        public const float MINROTATIONALVELOCITY = 1f; //in degrees
        public const float MAXROTATIONALVELOCITY = 2f; //in degrees
        const int ORE_MIN_DROP = 2;
        const int ORE_MAX_DROP = 5;

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
        static readonly Color[] SCRAPE_COLORS = { Color.Gray, Color.DimGray, Color.LightSlateGray, Color.SandyBrown, Color.RosyBrown };
        static readonly Color[] EXPLOSION_COLORS = { Color.LawnGreen, Color.White, Color.LightSlateGray, Color.LightGray };

        public const int LARGE_ASTEROID_DAMAGE_THRESHOLD = 10;

        #endregion

        public static void Initialize(int asteroidsToGenerate, List<Texture2D> asteroidTextures,
            List<Texture2D> particleTextures, bool regenAsteroids)
        {
            AsteroidsToGenerate = asteroidsToGenerate;
            AsteroidsToGenerate += Util.rnd.Next(-genRandomization, genRandomization); //add a bit of randomization

            AsteroidTextures = asteroidTextures;
            ExplosionParticleTextures = particleTextures;
            RegenerateAsteroids = regenAsteroids;

            Regenerate(asteroidsToGenerate);
        }

        public static void Regenerate(int asteroids)
        {
            AsteroidsToGenerate = asteroids;
            Asteroids = new List<GameObject>();
            emitters = new List<ParticleEmitter>();

            for (int i = 0; i < asteroids; i++)
            {
                AddRandomAsteroid(); //initial placement of asteroids
            }
        }

        public static void Update(GameTime gameTime)
        {
            for (int i = 0; i < Asteroids.Count; i++)
                Asteroids[i].Update(gameTime); //update the asteroids themselves

            for (int i = 0; i < emitters.Count; i++)
            {
                emitters[i].Update(gameTime); //update all the explosion/scrape particle emitters
                if (emitters[i].TimeToEmit == 0 && emitters[i].Particles.Count == 0)
                    emitters.RemoveAt(i); //prune any inactive/expired particle emitters
            }

            if (Asteroids.Count < AsteroidsToGenerate && RegenerateAsteroids)
                AddRandomAsteroid(true); //regenerate asteroid offscreen

            if (RegenerateAsteroids == false)
            {
                for (int i = 0; i < Asteroids.Count; i++)
                    if (Asteroids[i].Texture == ContentHandler.Textures[ORE_ASTEROID] &&
                        Camera.IsObjectVisible(Asteroids[i].BoundingBox) == false)
                        Asteroids.RemoveAt(i); 
                //if asteroid contains ore, is offscreen, and the level is over, silently remove it
            }

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
                        if (p.Damage >= LARGE_ASTEROID_DAMAGE_THRESHOLD) //and the shot meets the damage threshold
                        {
                            DestroyAsteroid(Asteroids[x], true); //destroy it
                            ProjectileManager.PlayShotHitSound(p);
                            break;
                        }
                        else
                            break;
                    }
                    else
                    {
                        DestroyAsteroid(Asteroids[x], true); //any shot can destroy the smaller ones
                        ProjectileManager.PlayShotHitSound(p);
                        break;
                    }
                }

                if(Asteroids[x].IsCircleColliding(Player.Ship))
                {
                    Player.Health -= Player.ASTEROID_COLLISION_DAMAGE; //add damage
                    Player.Ship = GameObject.Bounce(Player.Ship, Asteroids[x]).Object1; //bounce the ship
                    Player.TriggerShield(); //activate the shield effect

                    Player.Ship.RotationVelocityDegrees = (float)Util.rnd.NextDouble( //add randomization to the bounce
                        -Player.ROT_VEL_BOUNCE_CHANGE, Player.ROT_VEL_BOUNCE_CHANGE);

                    DestroyAsteroid(Asteroids[x], true); //and blow up the asteroid

                    //ContentHandler.PlaySFX(Player.COLLISION_SFX);
                    break;
                }
            }


        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < Asteroids.Count; i++)
            {
                Asteroids[i].Draw(spriteBatch);

                //spriteBatch.DrawString(ContentHandler.Fonts["lcd"], //DEBUG METHOD
                //    i.ToString(),
                //    Camera.GetLocalCoords(Asteroids[i].WorldLocation),
                //    Color.White);

                //spriteBatch.Draw( //YET ANOTHER DEBUG METHOD
                //    ContentHandler.Textures["particle"],
                //    Camera.GetLocalCoords(Asteroids[i].BoundingBox),
                //    Color.White);
            }

            for (int i = 0; i < emitters.Count; i++)
                emitters[i].Draw(spriteBatch);
        }

        public static void DestroyAsteroid(GameObject asteroid, bool effect = true)
        {
            int x = Util.rnd.Next(50); //temp var

            //around 50% chance of splitting if asteroid is a large non-ore asteroid
            if (x > 25 && asteroid.Texture == ContentHandler.Textures[LARGE_ASTEROID])
                splitAsteroid(asteroid);

            if (asteroid.Texture == ContentHandler.Textures[ORE_ASTEROID])
            {
                for (int i = 0; i < Util.rnd.Next(ORE_MIN_DROP, ORE_MAX_DROP); i++)
                    OreManager.AddOreDrop(asteroid); //if it is an ore asteroid, add drop(s) on destroy
            }

            if(effect) //add a particle effect if flag is true
                addExplosion(asteroid.WorldCenter);

            Asteroids.Remove(asteroid); //remove the asteroid from the list
        }

        public static void AddRandomAsteroid(bool offScreen = false) //add a random asteroid, param for offscreen generation
        {
            AsteroidType temp = AsteroidType.Small; //temporary variables
            int x = Util.rnd.Next(100); //get a random number between 0 and 100

            if (x >= 0 && x < 33) //if 0-32, asteroid is small
                temp = AsteroidType.Small;
            if (x >= 33 && x < 66) //if 33-65, asteroid is big
                temp = AsteroidType.Large;
            if (x >= 66 && x <= 100) //if 66-100, asteroid is ore
                temp = AsteroidType.Ore;

            AddAsteroid(temp, offScreen); //call overload of self
        }

        public static void AddAsteroid(AsteroidType type, bool offScreen = false) //add a specified type asteroid
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
                float rot = MathHelper.ToRadians((float)Util.rnd.NextDouble(0f, 359f)); //random rotation

                //random rotational velocity; within constants
                float rotVel = MathHelper.ToRadians((float)Util.rnd.NextDouble(MINROTATIONALVELOCITY, MAXROTATIONALVELOCITY));

                float colRadius = text.GetMeanRadius();

                Vector2 vel; //temporary velocity
                Vector2 worldPos; //temporary worldposition

                if (offScreen) //if asteroid needs to be generated offscreen
                {
                    SpawnSide side = Util.RandomEnumValue<SpawnSide>(); //pick a random world border to spawn it from
                    switch (side)
                    {
                        case SpawnSide.Up:
                            vel = GetVelocity(MathHelper.ToRadians((float)Util.rnd.Next(140, 230))); //get a velocity pointing between 140/230 degrees

                            worldPos = new Vector2( //and add a velocity which will knock it into the world borders next tick
                                Util.rnd.Next(Camera.WorldRectangle.X, Camera.WorldRectangle.Width),
                                Camera.WorldRectangle.Y - text.Height);
                            break;
                        case SpawnSide.Left:
                            vel = GetVelocity(MathHelper.ToRadians((float)Util.rnd.Next(50, 130))); //between 50/130 degrees

                            worldPos = new Vector2(
                                Camera.WorldRectangle.X - text.Width,
                                Util.rnd.Next(Camera.WorldRectangle.Y, Camera.WorldRectangle.Height));
                            break;
                        case SpawnSide.Right:
                            vel = GetVelocity(MathHelper.ToRadians((float)Util.rnd.Next(230, 310))); //between 230/310 degrees

                            worldPos = new Vector2(
                                Camera.WorldRectangle.Width + text.Width,
                                Util.rnd.Next(Camera.WorldRectangle.Y, Camera.WorldRectangle.Height));
                            break;
                        case SpawnSide.Down:
                            vel = GetVelocity(
                                MathHelper.ToRadians(Util.rnd.Next(320, 410) % 360)); //between 320/(360 + 50) degrees

                            worldPos = new Vector2(
                                Util.rnd.Next(Camera.WorldRectangle.X, Camera.WorldRectangle.Width),
                                Camera.WorldRectangle.Height + text.Height);
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
                else //if the asteroid does not need to be generated offscreen...
                {
                    vel = GetVelocity(rot); //get a random velocity according to the rotation

                    worldPos = new Vector2( //and simply get a random position in the world to place it...
                        Util.rnd.Next(Camera.WorldRectangle.X, Camera.WorldRectangle.Width),
                        Util.rnd.Next(Camera.WorldRectangle.Y, Camera.WorldRectangle.Height));
                }

                tempAsteroid = new GameObject( //init a temporary asteroid to check for overlaps
                    text, worldPos, vel, Color.White, false, rot, rotVel, 1f, ASTEROID_DRAW_DEPTH, false, text.GetMeanRadius());

                foreach (GameObject asteroid in Asteroids)
                {
                    if (tempAsteroid.BoundingBox.Intersects(asteroid.BoundingBox) 
                        || tempAsteroid.BoundingCircle.Intersects(Player.SpawnCircle))
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

        public static Vector2 GetVelocity(float rot) //get a random velocity
        {
            return Vector2.Multiply(
                rot.RotationToVector(),
                (float)Util.rnd.NextDouble(MINVELOCITY, MAXVELOCITY));
        }

        #region Local Methods

        static void splitAsteroid(GameObject asteroidToSplit) //splits a destroyed large asteroid into a small one
        {
            //split asteroid into small, inherting most of the characteristics
            GameObject split = new GameObject(
                ContentHandler.Textures[SMALL_ASTEROID],
                asteroidToSplit.WorldCenter,
                GetVelocity(asteroidToSplit.RotationDegrees + Util.rnd.Next(90, 270)), //get a random new velocity
                asteroidToSplit.TintColor,
                false, //nothing to animate
                asteroidToSplit.Rotation,
                asteroidToSplit.RotationalVelocity,
                asteroidToSplit.Scale,
                asteroidToSplit.Depth,
                false,
                ContentHandler.Textures[SMALL_ASTEROID].GetMeanRadius()); //get rounded radius

            Asteroids.Add(split);
        }

        static void addExplosion(Vector2 point) //add an omni-directional particle burst at specified point
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

        static void addScrapeEffect(GameObjectPair objects) //add a bi-direction particle spread at the center of a pair of objects
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
