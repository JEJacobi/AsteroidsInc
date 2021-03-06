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
    public class Projectile : GameObject
    {
        #region Declarations
		
        public FoF_Ident Identification { get; set; }
        public string HitSound { get; set; }
        public float DistanceTravelled { get; private set; }
        public bool DetonateEffect { get; set; }
        public bool TrackVelocity { get; set; }
        public bool IsDummyProjectile { get; set; }

        public readonly int Damage;
        public readonly float MaxRange;

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
            bool trackVelocity,
            bool isDummy,
            int collisionRadius = 1)
            : base(texture, initialLocation, velocity, Color.White, false, rotation, 0f, 1f, PROJECTILE_DEPTH, false, collisionRadius)
        {
            Identification = ident;
            Damage = dmg;
            MaxRange = range;
            HitSound = hitSound;
            DetonateEffect = detEffect;
            TrackVelocity = trackVelocity;
            IsDummyProjectile = isDummy;

            DistanceTravelled = 0f;
            lastLoc = initialLocation;
        }

        public override void Update(GameTime gameTime)
        {
            lastLoc = WorldCenter;
            //update the last location

            if (TrackVelocity) //rotate to the velocity vector
                VectorTrack(Velocity);

            base.Update(gameTime);
            //GameObject update
            
            if(Camera.IsObjectInWorld(this.BoundingBox))
                DistanceTravelled += Vector2.Distance(WorldCenter, lastLoc);
            //update the distance travelled as long as the projectile is in world

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
