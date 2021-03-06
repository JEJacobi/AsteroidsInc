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
    public class ParticleEmitter
    {
        #region Declarations

        public List<Particle> Particles { get; set; } //List of the particles themselves
        public List<Texture2D> Textures { get; set; } //List of textures, random one is pulled for

        public Vector2 VelocityToInherit { get; set; } 
        //what velocity should the particles inherit, in addition to the ejection speed

        public readonly int MaxParticles; //maximum amount of particles for this emitter
        public readonly float EjectionSpeed; //inital speed of particles
        public readonly float RandomMargin; //how much to randomize the particles' paths
        public bool Emitting { get; set; } //is emitting particles?
        public List<Color> Colors { get; set; } //list of possible colors
        public int FramesToLive { get; set; } //frames to live, passed down to particles
        public int TimeToEmit { get; set; } //time to emit particles
        public int ParticlesPerTick { get; set; } //how many particles to emit per tick
        public bool ParticleFading { get; set; } //should they fade or vanish?
        public float PositionRandomization { get; set; } //the amount of distance an emitted particle is randomized by

        public float ParticleDrawDepth = 1f;

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

        public const float EXPLOSIONSPRAY = 180f;
        public const float MINROTVEL = -0.1f;
        public const float MAXROTVEL = 0.1f;

        #endregion

        public ParticleEmitter(
            int maxParticles,
            Vector2 worldPosition,
            List<Texture2D> textures,
            List<Color> colors,
            int framesToLive,
            bool emitting = false,
            bool particleFading = true,
            int timeToEmit = -1,
            int particlesPerTick = 1,
            float ejectionSpeed = 1f,
            float randomMargin = 0.1f,
            float initialDirectionDegrees = 0f,
            float sprayWidthDegrees = 30f,
            float positionRandomization = 0f)
        {
            MaxParticles = maxParticles;
            WorldPosition = worldPosition;
            Textures = textures;
            Colors = colors;
            FramesToLive = framesToLive;
            TimeToEmit = timeToEmit;
            ParticlesPerTick = particlesPerTick;
            EjectionSpeed = ejectionSpeed;
            RandomMargin = randomMargin;
            DirectionInDegrees = initialDirectionDegrees;
            SprayWidthInDegrees = sprayWidthDegrees;
            Emitting = emitting;
            ParticleFading = particleFading;
            VelocityToInherit = Vector2.Zero;
            PositionRandomization = positionRandomization;

            Particles = new List<Particle>();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Camera.IsObjectVisible(WorldPosition)) //if emitter isn't visible, don't bother drawing
            {
                for (int i = 0; i < Particles.Count; i++)
                    Particles[i].Draw(spriteBatch);
            }
        }

        public void Update(GameTime gameTime)
        {
            if ((TimeToEmit >= 1 || TimeToEmit == -1) //if timetoemit is positive/infinite and emitting, and emitter is visible
                && Emitting && Camera.IsObjectVisible(WorldPosition))
            {
                for (int i = 0; i < ParticlesPerTick; i++)
                    EmitParticle();

                if (TimeToEmit > 0)
                    TimeToEmit--;
            }

            for (int i = 0; i < Particles.Count; i++) //update particles
            {
                Particles[i].Update(gameTime);

                if (Particles[i].TTL == 0) //if particle is expired
                    Particles.RemoveAt(i); //remove
            }

            if (Particles.Count > MaxParticles) //if overflowing, remove random particle
                Particles.RemoveAt(Util.rnd.Next(Particles.Count));
        }

        protected void EmitParticle() //emits a particle
        {
            if (Particles.Count + 1 < MaxParticles) //if not overflowing
            {
                int rndTex = Util.rnd.Next(Textures.Count); //get random texture from the list
                int rndColor = Util.rnd.Next(Colors.Count); //get a random color from the list

                Particles.Add(new Particle( //Add a new particle
                    Textures[rndTex],
                    RandomizedPosition(WorldPosition),
                    GetVelocity(),
                    Colors[rndColor],
                    FramesToLive,
                    ParticleFading,
                    MathHelper.ToRadians(Util.rnd.Next(0, 359)),
                    (float)Util.rnd.NextDouble(MINROTVEL, MAXROTVEL),
                    1f,
                    ParticleDrawDepth));

            }
        }

        protected Vector2 GetVelocity() //gets a random starting velocity
        {
            float rndSpray = (float)Util.rnd.NextDouble(-1, 1) * SprayWidth; //gets a random amount of spraywidth
            Vector2 tempVect = (rndSpray + Direction).RotationToVector(); //gets a Vector from the result
            tempVect = Vector2.Multiply(tempVect, RandomMultiplier()); //and multiplies the X&Y by the random margin
            tempVect = Vector2.Multiply(tempVect, EjectionSpeed); //adds the ejection speed multiplier
            return tempVect + VelocityToInherit;
        }

        protected float RandomMultiplier()
        {
            return (float)Util.rnd.NextDouble(1 - RandomMargin, 1 + RandomMargin);
        }

        protected Vector2 RandomizedPosition(Vector2 pos)
        {
            if (PositionRandomization == 0)
                return pos; //if randomization is set to zero, return base value

            //and randomize the position for both x/y axes
            pos.X += (float)Util.rnd.NextDouble(-PositionRandomization, PositionRandomization);
            pos.Y += (float)Util.rnd.NextDouble(-PositionRandomization, PositionRandomization);
            return pos;
        }
    }

    public enum EmissionType
    {
        Scrape,
        Explosion,
        Trail
    }
}
