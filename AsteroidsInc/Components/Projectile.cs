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
    public class Projectile : GameObject
    {
        #region Declarations
		
        public FoF_Ident Identification { get; set; }
        public string HitSound { get; set; }
        public float DistanceTravelled { get; private set; }
        public bool DetonateEffect { get; set; }

        public readonly int Damage;
        public readonly float MaxRange;

        const float velTrackSpeed = 0.3f;
        const float stabilization = 0.1f;

        Vector2 lastLoc;
        
        #endregion

        #region Constants
		
        const float PROJECTILE_DEPTH = 0.5f;

	    #endregion

        public Projectile(
            Texture2D texture,
            Vector2 initialLocation,
            Vector2 velocity,
            float rotation,
            string hitSound,
            FoF_Ident ident,
            int range,
            int dmg,
            bool detEffect,
            int collisionRadius = 1)
            : base(texture, initialLocation, velocity, Color.White, false, rotation, 0f, 1f, PROJECTILE_DEPTH, collisionRadius)
        {
            Identification = ident;
            Damage = dmg;
            MaxRange = range;
            HitSound = hitSound;
            DetonateEffect = detEffect;

            DistanceTravelled = 0f;
            lastLoc = initialLocation;
        }

        public override void Update(GameTime gameTime)
        {
            lastLoc = WorldCenter;
            //update the last location

            float target = Vector2.Normalize(Velocity).RotateTo(); //get the target angle from this projectile's velocity
            target = MathHelper.ToDegrees(target);

            if (target < 0) //convert from Atan2's -180/180 degree range to a 0/360 degree range
            {
                target += 360;
            }

            if (target < RotationDegrees - 10)
                RotationVelocityDegrees -= velTrackSpeed;
            if (target > RotationDegrees + 10)
                RotationVelocityDegrees += velTrackSpeed;

            //stabilize rotation; ripped from Player class
            float newRotVel = RotationVelocityDegrees;
            if (newRotVel > 0) //if rotating right
                newRotVel -= stabilization * newRotVel;
            if (newRotVel < 0) //if rotating left
                newRotVel += stabilization * -newRotVel;
            RotationVelocityDegrees = newRotVel; //and assign

            base.Update(gameTime);
            //GameObject update

            DistanceTravelled += Vector2.Distance(WorldCenter, lastLoc);
            //update the distance travelled

            if (DistanceTravelled > MaxRange)
                Active = false; //if max range has been exceeded, projectile is no longer active
        }

        //Don't need to override the Draw method
    }

    public enum FoF_Ident
    {
        Friendly,
        Neutral,
        Enemy
    }
}
