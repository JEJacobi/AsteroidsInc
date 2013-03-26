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
    public class ParticleEmitter
    {
        #region Declarations

        public List<Particle> Particles { get; set; } //List of the particles themselves
        public List<Texture2D> Textures { get; set; } //List of textures, random one is pulled for
        public readonly int MaxParticles; //maximum amount of particles for this emitter
        public readonly float EjectionSpeed; // inital speed of particles
        public readonly float RandomMargin;
        public bool Emitting { get; set; } //is emitting particles?
        public List<Color> Colors { get; set; } //list of possible colors
        public int FramesToLive { get; set; } //frames to live, passed down to particles

        public Vector2 WorldPosition { get; set; } //world position
        public float Direction { get; set; } //direction in radians
        public float DirectionInDegrees
        {
            get { return MathHelper.ToDegrees(Direction); }
            set { Direction = MathHelper.ToRadians(value); }
        }
        public float SprayWidth { get; set; } //spray width in radians
        public float SprayWidthInDegrees
        {
            get { return MathHelper.ToDegrees(SprayWidth); }
            set { SprayWidth = MathHelper.ToRadians(value); }
        }

        Random rnd; //random, used for particle spray, random colors, and random textures

        #endregion

        public ParticleEmitter(
            int maxParticles,
            Vector2 worldPosition,
            List<Texture2D> textures,
            List<Color> colors,
            int framesToLive,
            bool emitting = false,
            float ejectionSpeed = 1f,
            float randomMargin = 0.1f,
            float initialDirectionDegrees = 0f,
            float sprayWidthDegrees = 30f)
        {
            MaxParticles = maxParticles;
            WorldPosition = worldPosition;
            Textures = textures;
            Colors = colors;
            FramesToLive = framesToLive;
            EjectionSpeed = ejectionSpeed;
            RandomMargin = randomMargin;
            DirectionInDegrees = initialDirectionDegrees;
            SprayWidthInDegrees = sprayWidthDegrees;
            Emitting = emitting;

            Particles = new List<Particle>();
            rnd = new Random();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < Particles.Count; i++)
            {
                if (Camera.IsObjectVisible(Particles[i].WorldRectangle))
                    Particles[i].Draw(spriteBatch); //If I is visible, draw...
            }
        }

        public void Update(GameTime gameTime)
        {
            EmitParticle();

            for (int i = 0; i < Particles.Count; i++) //update particles
            {
                Particles[i].Update(gameTime);

                if (Particles[i].TTL <= 0) //if particle is expired
                    Particles.RemoveAt(i); //remove
            }

            if (Particles.Count > MaxParticles) //if overflowing, remove random particle
                Particles.RemoveAt(rnd.Next(Particles.Count));
        }

        public void EmitParticle() //emits a particle
        {
            if (Emitting && (Particles.Count + 1 < MaxParticles)) //if emitting & not overflowing
            {
                int rndTex = rnd.Next(Textures.Count); //get random texture from the list
                int rndColor = rnd.Next(Colors.Count); //get a random color from the list

                Particles.Add(new Particle( //Add a new particle
                    Textures[rndTex],
                    WorldPosition,
                    GetVelocity(),
                    Colors[rndColor],
                    FramesToLive));

            }
        }

        protected Vector2 GetVelocity() //gets a random starting velocity
        {
            float rndSpray = (float)rnd.NextDouble(-1, 1) * SprayWidth; //gets a random amount of spraywidth
            Vector2 tempVect = (rndSpray + Direction).RotationToVectorFloat(); //gets a Vector from the result
            //TODO: Test
            tempVect = Vector2.Multiply(tempVect, RandomMultiplier()); //and multiplies the X&Y by the random margin
            tempVect = Vector2.Multiply(tempVect, EjectionSpeed); //adds the ejection speed multiplier
            return tempVect;
        }

        protected float RandomMultiplier()
        {
            return (float)rnd.NextDouble(1 - RandomMargin, 1 + RandomMargin);
        }
    }
}
