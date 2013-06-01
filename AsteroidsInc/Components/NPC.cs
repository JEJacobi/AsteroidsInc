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
    public class NPC : GameObject
    {
        public AIState CurrentState;
        public AIState LastState;
        public int Health;
        public EquipmentData Weapon;
        public readonly int StartingHealth;
        public readonly float MaxSpeed;
        public readonly float AccelerationSpeed;
        public readonly float TrackSpeed;
        public readonly float TrailOffset;
        public readonly float WeaponOffset;
        public bool Activated;

        ParticleEmitter trail;

        //AI data
        public Vector2 Target = Vector2.Zero;
        public Vector2 WorldTarget //sets the target from a world coordinate, instead of relative coordinates
        {
            set
            {
                Target = Vector2.Normalize(value - WorldCenter);
            }
        }
        bool attacking = false;
        bool accelerating = false;
        bool firing = false;
        readonly int firedelay;
        int firecounter;
        Projectile outProjectile;

        const float NPC_DEPTH = 0.5f;

        //Engine trail particle emitter config
        const string TRAIL_PARTICLE = "particle";
        static readonly Color[] TRAIL_COLORS = { Color.Blue, Color.DarkBlue, Color.LightSteelBlue, Color.White };
        const int TRAIL_MAX_PARTICLES = Int32.MaxValue;
        const int TRAIL_FTL = 20;
        const int TRAIL_PPT = 1;
        const float TRAIL_EJECTION_SPEED = 300f;
        const float TRAIL_RANDOM_MARGIN = 0.1f;
        const float TRAIL_SPRAYWIDTH = 5f;

        public NPC(
            Texture2D texture,
            AIState initialState,
            Vector2 initialPos,
            Vector2 initialVel,
            Vector2 initialTarget,
            Color tintColor,
            int startingHealth,
            EquipmentData equip,
            int colRadius,
            float trailOffset,
            float weaponOffset,
            float maxSpeed,
            float trackSpeed,
            float maxAccelSpeed,
            float initialRot = 0f,
            float initialRotVel = 0f,
            int totalFrames = 0,
            int rows = 1,
            int columns = 1,
            int startingFrame = 0,
            float frameDelay = 0,
            bool activated = false)
            : base(texture, initialPos, initialVel, tintColor, false, initialRot, initialRotVel, 1f,
                NPC_DEPTH, false, colRadius, 0, 0, SpriteEffects.None, totalFrames, rows, columns, startingFrame, frameDelay)
        {
            CurrentState = initialState;
            LastState = initialState;
            Weapon = equip;
            Activated = activated;
            TrailOffset = trailOffset;
            WeaponOffset = weaponOffset;
            MaxSpeed = maxSpeed;
            TrackSpeed = trackSpeed;
            AccelerationSpeed = maxAccelSpeed;
            firedelay = equip.RefireDelay;
            Target = Vector2.Normalize(initialTarget);

            StartingHealth = startingHealth;
            Health = StartingHealth;

            trail = new ParticleEmitter( //initialize the engine trail particle emitter
                TRAIL_MAX_PARTICLES,
                GameObject.GetOffset(this, TrailOffset),
                ContentHandler.Textures[TRAIL_PARTICLE].ToTextureList(),
                TRAIL_COLORS.ToList<Color>(),
                TRAIL_FTL,
                false,
                true,
                -1,
                TRAIL_PPT,
                TRAIL_EJECTION_SPEED,
                TRAIL_RANDOM_MARGIN,
                this.RotationDegrees + 180,
                TRAIL_SPRAYWIDTH);
        }

        public override void Update(GameTime gameTime)
        {
            //handle target tracking
            if (Target != Vector2.Zero) 
                VectorTrack(Target, TrackSpeed);

            //handle accelerating
            if (accelerating) 
            {
                Velocity += Rotation.RotationToVector() * AccelerationSpeed; //accelerate the ship
                trail.Emitting = true; //activate the engine trail
            }
            else
            {
                trail.Emitting = false;
            }

            //handle firing
            if (firecounter > 0)
                firecounter--;

            if (firing && firecounter == 0)
            {
                ProjectileManager.AddShot(
                    Weapon,
                    GameObject.GetOffset(this, WeaponOffset),
                    this.Rotation,
                    this.Velocity,
                    FoF_Ident.Enemy);

                //reset the refire delay
                firecounter = Weapon.RefireDelay;
            }

            if(ProjectileManager.IsHit(this, out outProjectile, FoF_Ident.Enemy))
            {
                Health -= outProjectile.Damage;
            }

            handleStateLogic();

            base.Update(gameTime);
        }

        private void handleStateLogic() //main AI method
        {
            //TODO: Add logic.
        }
    }

    public enum AIState
    {
        Attack,
        Evade,
        Random,
        Regroup,
        Passive,
        Ram
    }
}
