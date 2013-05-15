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
    public static class OreManager
    {
        public static List<Particle> OreDrops;
        static List<ParticleEmitter> emitters;
        static Random rnd = new Random();

        public static List<Texture2D> OreTextures;
        public static List<Texture2D> OreEffectTextures;

        //Ore drop/effect config
        public const string ORE_KEY = "ore";
        const float ORE_FRAME_DELAY = 300f;
        const int ORE_DROP_FTL = 5000;
        const int ORE_EFFECT_FTL = 70;
        const int ORE_EFFECT_PARTICLES = 10;
        const float ORE_EFFECT_EJECTION_SPEED = 50f;
        static readonly Color[] ORE_PICKUP_COLORS = { Color.Pink, Color.Yellow, Color.White };

        const int PARTICLE_TIME_TO_EMIT = 1;
        const float PARTICLERANDOMIZATION = 2f;

        public static void Initialize(List<Texture2D> oreTextures, List<Texture2D> oreParticleTextures)
        {
            OreTextures = oreTextures;
            OreEffectTextures = oreParticleTextures;

            OreDrops = new List<Particle>();
            emitters = new List<ParticleEmitter>();
        }

        public static void Update(GameTime gameTime)
        {
            for (int i = 0; i < OreDrops.Count; i++)
            {
                if (Player.Ship.IsCircleColliding(OreDrops[i]))
                {
                    Player.CurrentOre++; //increase the ore counter
                    addOrePickupEffect(OreDrops[i]); //add an effect
                    if (Player.CurrentOre >= Player.OreWinCondition >> 1)
                        ContentHandler.PlaySFX("pickup2"); //play the second variaton
                    else
                        ContentHandler.PlaySFX("pickup"); //play a pickup sound
                    Player.TriggerShield(50, false);
                    OreDrops.RemoveAt(i); //and remove the drop
                    continue;
                }

                if (OreDrops[i].TTL <= 0)
                {
                    OreDrops.RemoveAt(i); //trim any inactive particles
                    continue;
                }

                OreDrops[i].Update(gameTime);
            }

            for (int i = 0; i < emitters.Count; i++)
                emitters[i].Update(gameTime);
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < OreDrops.Count; i++)
                OreDrops[i].Draw(spriteBatch);

            for (int i = 0; i < emitters.Count; i++)
                emitters[i].Draw(spriteBatch);
        }

        public static void AddOreDrop(GameObject origin) //add an ore drop using a gameobject's positional/velocity properties
        {
            Particle temp = new Particle( //generate the ore drop
               ContentHandler.Textures[ORE_KEY],
               origin.WorldCenter,
               origin.Velocity +
               Vector2.Multiply(AsteroidManager.GetVelocity(origin.Rotation + MathHelper.ToRadians(rnd.Next(-90, 90))), 0.5f),
               Color.White,
               ORE_DROP_FTL,
               true,
               origin.Rotation,
               (float)rnd.NextDouble(-0.1f, 0.1f),
               1f, 1f, SpriteEffects.None,
               ContentHandler.Textures[ORE_KEY].GetMeanRadius(8),
               0, 0, 8, 1, 8, 0, false, ORE_FRAME_DELAY);
            temp.Animating = true;

            OreDrops.Add(temp);
        }

        static void addOrePickupEffect(Particle ore)
        {
            ParticleEmitter temp = new ParticleEmitter(
                ORE_EFFECT_PARTICLES,
                ore.WorldCenter,
                OreEffectTextures,
                ORE_PICKUP_COLORS.ToList<Color>(),
                ORE_EFFECT_FTL,
                true,
                true,
                PARTICLE_TIME_TO_EMIT,
                ORE_EFFECT_PARTICLES,
                ORE_EFFECT_EJECTION_SPEED,
                PARTICLERANDOMIZATION,
                0f, ParticleEmitter.EXPLOSIONSPRAY);

            emitters.Add(temp);
        }
    }
}
