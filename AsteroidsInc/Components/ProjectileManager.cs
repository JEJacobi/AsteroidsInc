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
    public static class ProjectileManager
    {
        public static List<Projectile> Projectiles { get; set; }
        public static List<ParticleEmitter> HitEffects { get; set; }

        static readonly Color[] EFFECT_COLORS = { Color.Orange, Color.White };

        const int EFFECT_MAX_PARTICLES = 10;
        const int EFFECT_FRAMES_TO_LIVE = 200;
        const int EFFECT_TIME_TO_EMIT = 1;
        const int EFFECT_EJECTION_SPEED = 200;
        const float EFFECT_RANDOMIZATION = 0.2f;
        const float EFFECT_SPRAY_WIDTH = 50f;

        static Random rnd;

        static ProjectileManager()
        {
            Projectiles = new List<Projectile>();
            HitEffects = new List<ParticleEmitter>();
            rnd = new Random();
        }

        public static void Update(GameTime gameTime)
        {
            for (int i = 0; i < Projectiles.Count; i++)
            {
                Projectiles[i].Update(gameTime); //update the projectile

                if (Projectiles[i].Active == false)
                    Projectiles.RemoveAt(i); //if inactive; remove
            }

            for (int i = 0; i < HitEffects.Count; i++)
            {
                HitEffects[i].Update(gameTime); //update the particleEmitter

                if (HitEffects[i].TimeToEmit == 0 && HitEffects[i].Particles.Count == 0)
                    HitEffects.RemoveAt(i); //if inactive; remove
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (Projectile shot in Projectiles)
                shot.Draw(spriteBatch); //draw the shots

            foreach (ParticleEmitter emitter in HitEffects)
                emitter.Draw(spriteBatch); //and draw any effects
        }

        public static bool IsHit(GameObject obj, out Projectile shotHit, FoF_Ident alignment = FoF_Ident.Neutral)
        {
            foreach (Projectile shot in Projectiles) //loop through each shot to check if hit
            {
                if (shot.Identification != alignment) //check if friendly
                {
                    if (obj.IsPixelColliding(shot)) //check if colliding
                    {
                        HitEffects.Add(new ParticleEmitter( //add a hit effect
                            EFFECT_MAX_PARTICLES,
                            obj.WorldCenter.Center(shot.WorldCenter),
                            ContentHandler.Textures["particle"].ToTextureList(),
                            EFFECT_COLORS.ToList<Color>(),
                            EFFECT_FRAMES_TO_LIVE,
                            true,
                            true,
                            EFFECT_TIME_TO_EMIT,
                            EFFECT_MAX_PARTICLES,
                            EFFECT_EJECTION_SPEED,
                            EFFECT_RANDOMIZATION,
                            shot.RotationDegrees + 180,
                            EFFECT_SPRAY_WIDTH));

                        shotHit = shot; //set the out param
                        shot.Active = false;

                        ContentHandler.PlaySFX(shot.HitSound);

                        return true; //return true
                    }
                }
            }
            shotHit = null;
            return false;
        }

        public static void AddShot(
            EquipmentData shotData, //projectile data
            Vector2 initialLocation,
            float rotation,
            Vector2 inheritVelocity,
            FoF_Ident senderIdent)
        {
            Vector2 shotVel = Vector2.Multiply(rotation.RotationToVector(), shotData.Speed) + inheritVelocity;
            //get the scaled and inherited shot velocity

            if (shotData.Randomization != 0)
            {
                float tempX = (float)rnd.NextDouble(-shotData.Randomization, shotData.Randomization);
                float tempY = (float)rnd.NextDouble(-shotData.Randomization, shotData.Randomization);

                shotVel.X += tempX;
                shotVel.Y += tempY;
            }

            Projectiles.Add(new Projectile( //and generate a new projectile according to shotData
                shotData.Texture,
                initialLocation,
                shotVel,
                rotation,
                shotData.HitSoundIndex,
                senderIdent,
                shotData.MaxRange,
                shotData.Damage,
                shotData.CollisionRadius));

            ContentHandler.PlaySFX(shotData.LaunchSoundIndex); //play the launch sound
        }
    }
}
