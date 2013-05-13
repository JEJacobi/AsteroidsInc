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
        static readonly Color[] DETONATE_COLORS = { Color.White, Color.LightYellow, Color.PaleVioletRed };

        //Hit effect particle emission constants
        const int EFFECT_MAX_PARTICLES = 10;
        const int EFFECT_FRAMES_TO_LIVE = 100;
        const int EFFECT_TIME_TO_EMIT = 1;
        const int EFFECT_EJECTION_SPEED = 200;
        const float EFFECT_RANDOMIZATION = 0.3f;
        const float EFFECT_SPRAY_WIDTH = 50f;

        const int DETONATE_MAX_PARTICLES = 250;
        const int DETONATE_FRAMES_TO_LIVE = 25;
        const int DETONATE_TIME_TO_EMIT = 1;
        const int DETONATE_EJECTION_SPEED = 200;
        const float DETONATE_RANDOMIZATION = 0.1f;
        const float DETONATE_SPRAY_WIDTH = ParticleEmitter.EXPLOSIONSPRAY;

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
                {
                    if (Projectiles[i].DetonateEffect && Camera.IsObjectVisible(Projectiles[i].BoundingBox))
                        addDetonateEffect(Projectiles[i].WorldCenter); //add a detonation effect if visible and wanted

                    Projectiles.RemoveAt(i); //if inactive; remove
                }
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
                    if (obj.IsCircleColliding(shot)) //check if colliding
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
                            EFFECT_MAX_PARTICLES / DETONATE_TIME_TO_EMIT,
                            EFFECT_EJECTION_SPEED,
                            EFFECT_RANDOMIZATION,
                            shot.RotationDegrees + 180,
                            EFFECT_SPRAY_WIDTH));

                        shotHit = shot; //set the out param
                        shot.Active = false;

                        if (shot.DetonateEffect)
                            addDetonateEffect(shot.WorldCenter);

                        return true; //return true
                    }
                }
            }
            shotHit = null; //set the shot to null
            return false; //and return false
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

            if (shotData.Randomization != 0) //randomize the velocity by the provided factor
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
                shotData.DetonateEffect,
                shotData.CollisionRadius));
            
            if(shotData.LaunchSoundIndex != "" || shotData.LaunchSoundIndex != null)
                ContentHandler.PlaySFX(shotData.LaunchSoundIndex); //play the launch sound
        }

        public static void PlayShotHitSound(Projectile shot)
        {
            ContentHandler.PlaySFX(shot.HitSound);
        }

        private static void addDetonateEffect(Vector2 worldPos)
        {
            HitEffects.Add(new ParticleEmitter(
                DETONATE_MAX_PARTICLES,
                worldPos,
                ContentHandler.Textures["particle"].ToTextureList(),
                DETONATE_COLORS.ToList<Color>(),
                DETONATE_FRAMES_TO_LIVE,
                true,
                true,
                DETONATE_TIME_TO_EMIT,
                DETONATE_MAX_PARTICLES / DETONATE_TIME_TO_EMIT,
                DETONATE_EJECTION_SPEED,
                DETONATE_RANDOMIZATION,
                0,
                DETONATE_SPRAY_WIDTH));  
        }
    }
}
