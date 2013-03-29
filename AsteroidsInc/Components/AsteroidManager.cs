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

        const int SCRAPEPARTICLES = 50;
        const int SCRAPEFRAMESTOLIVE = 6;
        const float SCRAPEEJECTIONSPEED = 30f;

        const int EXPLOSIONPARTICLES = 5000;
        const int EXPLOSIONFRAMESTOLIVE = 30;
        const float EXPLOSIONEJECTIONSPEED = 50f;

        const int MAXTRIES = 50;
        const int PARTICLETIMETOEMIT = 1;

        const float PARTICLERANDOMIZATION = 2f;

        readonly Color[] SCRAPECOLORS = { Color.Brown, Color.SaddleBrown, Color.RosyBrown };
        readonly Color[] EXPLOSIONCOLORS = { Color.White, Color.LightYellow, Color.Orange, Color.OrangeRed, Color.Aquamarine };

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

                        addScrapeEffect(temp);
                    }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < Asteroids.Count; i++)
                Asteroids[i].Draw(spriteBatch);

            for (int i = 0; i < emitters.Count; i++)
                emitters[i].Draw(spriteBatch);
        }

        public void DestroyAsteroid(GameObject asteroid)
        {
            addExplosion(asteroid.WorldCenter);
            Asteroids.Remove(asteroid);
        }

        protected void addAsteroid()
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

                Vector2 vel = Vector2.Multiply( //calculate a random velocity
                    rot.RotationToVectorFloat(),
                    (float)rnd.NextDouble(MinVelocity, MaxVelocity));

                Vector2 worldPos = new Vector2(
                    rnd.Next(Camera.WorldRectangle.X, Camera.WorldRectangle.Width),
                    rnd.Next(Camera.WorldRectangle.Y, Camera.WorldRectangle.Height));

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
            emitters.Add(new ParticleEmitter( //TODO: Test
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
                10f));

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
                10f));
        }
    }
}
