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
            float initialDirectionDegrees = 0f,
            float sprayWidthDegrees = 30f,
            bool emitting = false)
        {
            MaxParticles = maxParticles;
            WorldPosition = worldPosition;
            Textures = textures;
            Colors = colors;
            FramesToLive = framesToLive;
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
            RemoveExpired();

            if (Particles.Count > MaxParticles) //if overflowing, remove random particle
                Particles.RemoveAt(rnd.Next(Particles.Count));
        }

        public void EmitParticle()
        {
            if (Emitting && (Particles.Count + 1 < MaxParticles)) //if emitting & not overflowing
            {
                int rndTex = rnd.Next(Textures.Count); //get random texture from the list
                int rndColor = rnd.Next(Colors.Count); //get a random color from the list

                Particles.Add(new Particle( //Add a new particle
                    Textures[rndTex],
                    WorldPosition,
                    Vector2.Zero, //TODO: Add math for converting rotation into vector
                    Colors[rndColor],
                    FramesToLive));

            }
        }

        public void RemoveExpired()
        {
            for (int i = 0; i < Particles.Count; i++) //if particle is expired, remove it
            {
                if (Particles[i].TTL <= 0)
                    Particles.RemoveAt(i);
            }
        }
    }
}
