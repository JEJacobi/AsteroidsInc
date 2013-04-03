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
    public class AsteroidManager
    {
        #region Declarations

        public List<GameObject> Asteroids;
        Vector2 lastCollisionIndex = new Vector2(-1, -1);
        public bool RegenerateAsteroids { get; set; }

        public readonly int InitialAsteroids;
        public readonly float MinVelocity;
        public readonly float MaxVelocity;
        public readonly float MinRotationalVelocity; //in degrees
        public readonly float MaxRotationalVelocity; //in degrees
        public readonly List<Texture2D> Textures;
        public readonly List<Texture2D> ExplosionParticleTextures;

        List<ParticleEmitter> emitters;
        Random rnd;

        const int SCRAPEPARTICLES = 60;
        const int SCRAPEFRAMESTOLIVE = 5;
        const float SCRAPEEJECTIONSPEED = 30f;
        const float SCRAPESPRAY = 20f;

        const int EXPLOSIONPARTICLES = 500;
        const int EXPLOSIONFRAMESTOLIVE = 20;
        const float EXPLOSIONEJECTIONSPEED = 50f;

        const int MAXTRIES = 50;
        const int PARTICLETIMETOEMIT = 1;

        const float PARTICLERANDOMIZATION = 2f;

        readonly Color[] SCRAPECOLORS = { Color.Gray, Color.DimGray, Color.LightSlateGray, Color.SandyBrown, Color.RosyBrown };
        readonly Color[] EXPLOSIONCOLORS = { Color.White, Color.LightYellow, Color.Orange, Color.OrangeRed, Color.Aquamarine, Color.LimeGreen };

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
            rnd = new Random();

            InitialAsteroids = initialAsteroids;
            MinVelocity = minVel;
            MaxVelocity = maxVel;
            MinRotationalVelocity = minRotVel;
            MaxRotationalVelocity = maxRotVel;
            Textures = textures;
            ExplosionParticleTextures = explosionParticleTextures;
            RegenerateAsteroids = regenAsteroids;

            Asteroids = new List<GameObject>();
            emitters = new List<ParticleEmitter>();

            for (int i = 0; i < InitialAsteroids; i++)
                addAsteroid();
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < Asteroids.Count; i++)
                Asteroids[i].Update(gameTime);

            for (int i = 0; i < emitters.Count; i++)
                emitters[i].Update(gameTime);

            if (Asteroids.Count < InitialAsteroids && RegenerateAsteroids)
                addAsteroid();

            for (int x = 0; x < Asteroids.Count; x++) //Pretty much brute-forcing the collision detection
                for (int y = x + 1; y < Asteroids.Count; y++)
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
                            DestroyAsteroid(temp.Object2, false);
                            addAsteroid(true);
                        }

                        addScrapeEffect(temp);
                        lastCollisionIndex = new Vector2(x, y);
                    }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < Asteroids.Count; i++)
            {
                Asteroids[i].Draw(spriteBatch);
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
            if(effect)
                addExplosion(asteroid.WorldCenter);
            Asteroids.Remove(asteroid);
        }

        protected void addAsteroid(bool offScreen = false)
        {
            GameObject tempAsteroid;
            bool isOverlap = false;
            int counter = 0;

            do //Do-While to ensure that the asteroid gets generated at least once
            {
                Texture2D text = Textures.PickRandom<Texture2D>();

                float rot = MathHelper.ToRadians((float)rnd.NextDouble(0f, 359f));
                float rotVel = MathHelper.ToRadians((float)rnd.NextDouble(MinRotationalVelocity, MaxRotationalVelocity));

                int colRadius = (((text.Width / 2) + (text.Height / 2)) / 2); //Get the mean of text's height & width

                Vector2 vel;
                Vector2 worldPos;

                if (offScreen)
                {
                    SpawnSide side = Util.RandomEnumValue<SpawnSide>();
                    switch (side)
                    {
                        case SpawnSide.Up:
                            vel = getVelocity(MathHelper.ToRadians((float)rnd.Next(140, 230))); //get a velocity pointing between 140/230 degrees

                            worldPos = new Vector2(
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
                else
                {
                    vel = getVelocity(rot);

                    worldPos = new Vector2(
                        rnd.Next(Camera.WorldRectangle.X, Camera.WorldRectangle.Width),
                        rnd.Next(Camera.WorldRectangle.Y, Camera.WorldRectangle.Height));
                }

                tempAsteroid = new GameObject( //init a temporary asteroid to check for overlaps
                    text, worldPos, vel, Color.White, false, rot, rotVel, 1f, 0f, colRadius);

                foreach (GameObject asteroid in Asteroids)
                {
                    if (tempAsteroid.BoundingBox.Intersects(asteroid.BoundingBox))
                    {
                        isOverlap = true;
                        break;
                    }
                }
                counter++;

            } while (isOverlap && counter < MAXTRIES); //if overlapping, loop, if maxtries exceeded, quit

            if (counter >= MAXTRIES)
            {
                Logger.WriteLog("Asteroid placement overflow, canceling.");
            }
            else
            {
                Asteroids.Add(tempAsteroid); //add the temp asteroid if not at maxtries
            }
        }

        protected void addExplosion(Vector2 point)
        {
            emitters.Add(new ParticleEmitter(
                EXPLOSIONPARTICLES, //random amount of particles
                point, //at the point
                ExplosionParticleTextures,
                EXPLOSIONCOLORS.ToList<Color>(), //colors to list
                EXPLOSIONFRAMESTOLIVE,
                true,
                PARTICLETIMETOEMIT,
                EXPLOSIONPARTICLES, //emit max particles in one tick
                EXPLOSIONEJECTIONSPEED,
                PARTICLERANDOMIZATION,
                0f, //no direction needed
                ParticleEmitter.EXPLOSIONSPRAY)); //360 degree explosion
        }

        protected void addScrapeEffect(GameObjectPair objects)
        {
            Vector2 normal = GameObject.GetNormal(objects);

            emitters.Add(new ParticleEmitter(
                SCRAPEPARTICLES,
                objects.CenterPoint(),
                ExplosionParticleTextures,
                SCRAPECOLORS.ToList<Color>(),
                SCRAPEFRAMESTOLIVE,
                true,
                PARTICLETIMETOEMIT,
                SCRAPEPARTICLES,
                SCRAPEEJECTIONSPEED,
                PARTICLERANDOMIZATION,
                MathHelper.ToDegrees(normal.RotateTo()) + 90, //get the velocity angle + 90 degrees
                SCRAPESPRAY));

            emitters.Add(new ParticleEmitter(
                SCRAPEPARTICLES,
                objects.CenterPoint(),
                ExplosionParticleTextures,
                SCRAPECOLORS.ToList<Color>(),
                SCRAPEFRAMESTOLIVE,
                true,
                PARTICLETIMETOEMIT,
                SCRAPEPARTICLES,
                SCRAPEEJECTIONSPEED,
                PARTICLERANDOMIZATION,
                MathHelper.ToDegrees(normal.RotateTo()) - 90,
                SCRAPESPRAY));
        }

        private Vector2 getVelocity(float rot)
        {
            return Vector2.Multiply(
                rot.RotationToVectorFloat(),
                (float)rnd.NextDouble(MinVelocity, MaxVelocity));
        }

        enum SpawnSide
        {
            Up,
            Left,
            Right,
            Down
        }
    }
}
