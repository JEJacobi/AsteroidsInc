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

        public List<GameObject> Asteroids { get; set; }
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

        const int MINPARTICLES = 50;
        const int MAXPARTICLES = 200;
        const int PARTICLEFTL = 40;

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
            int x = rnd.Next(MINPARTICLES, MAXPARTICLES);
            List<Color> colors = new List<Color>();
            colors.Add(Color.White);

            emitters.Add(new ParticleEmitter( //TODO: Test
                x,
                asteroid.WorldCenter,
                ExplosionParticleTextures,
                colors,
                PARTICLEFTL,
                true,
                1,
                x,
                1f,
                0.3f,
                0f,
                180f));

            Asteroids.Remove(asteroid);
        }

        protected void addAsteroid()
        {
            GameObject tempAsteroid;
            bool isOverlap = false;

            do //Do-While to ensure that the asteroid gets generated at least once
            {
                Texture2D text = Textures.PickRandom<Texture2D>(); //TODO: More testing

                float rot = (float)rnd.NextDouble(0f, 359f);
                float rotVel = (float)rnd.NextDouble(MinRotationalVelocity, MaxRotationalVelocity);

                int colRadius = (((text.Width / 2) * (text.Height / 2)) / 2); //Get the mean of text's height & width

                Vector2 vel = Vector2.Multiply( //calculate a random velocity
                    rot.RotationToVectorFloat(), //TODO: Yep, test.
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

            } while (isOverlap); //if overlapping, loop

            Asteroids.Add(tempAsteroid); //add the temp asteroid
        }


    }
}
