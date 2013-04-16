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
    public static class Player
    {
        #region Declarations

        public static GameObject Ship { get; set; }
        public static int Health { get; set; }

        public static ParticleEmitter EngineTrail { get; set; }
        public static ParticleEmitter ExplosionEmitter { get; set; }

        public static Slots ActiveSlot { get; set; }
        public static Equipment Slot1 { get; set; }
        public static Equipment Slot2 { get; set; }

        public static bool StabilizeRotation { get; set; }

        #endregion

        #region Constants

        public const string SHIP_TEXTURE = "ship"; //texture indexes
        public const string MISSILE_TEXTURE = "missile";
        public const string LASER_TEXTURE = "laser";
        public const string CANNON_TEXTURE = "cannon";

        public const float VELOCITY_MAX = 500f;
        public static Vector2 VECTOR_VELOCITY_MAX
        {
            get { return new Vector2(VELOCITY_MAX, VELOCITY_MAX); }
        }
        public const float MAX_ROT_VEL = 50f;
        public const float STABILIZATION_FACTOR = 0.08f;
        public const float ROT_VEL_CHANGE = 0.6f;
        public const float VEL_CHANGE_FACTOR = 4f;

        public static readonly Color[] TRAIL_COLORS = { Color.Orange, Color.OrangeRed, Color.MediumVioletRed };
        public static readonly Color[] EXPLOSION_COLORS = { Color.Gray, Color.Orange, Color.LightGray }; //effect colors

        public const int STARTING_HEALTH = 100; //health and effect thresholds
        public const int MAX_HEALTH = 100;
        public const int MIN_HEALTH = 0;
        public const int DAMAGE_THRESHOLD = 35; //threshold of damage effect

        public const Equipment STARTING_EQUIP_SLOT1 = Equipment.Laser;
        public const Equipment STARTING_EQUIP_SLOT2 = Equipment.Empty;
        public const Slots INITIAL_ACTIVE_SLOT = Slots.Slot1;

        #endregion

        public static void Initialize()
        {
            Health = STARTING_HEALTH;
            Slot1 = STARTING_EQUIP_SLOT1;
            Slot2 = STARTING_EQUIP_SLOT2;
            ActiveSlot = INITIAL_ACTIVE_SLOT;
            StabilizeRotation = true;

            Ship = new GameObject(
                ContentHandler.Textures[SHIP_TEXTURE],
                new Vector2(300, 300), //TEMP
                Vector2.Zero,
                Color.White,
                true,
                0f,
                0f,
                1f,
                0f,
                ContentHandler.Textures[SHIP_TEXTURE].GetMeanRadius());
        }

        public static void Update(GameTime gameTime)
        {
            //Stabilize rotation if wanted
            if (StabilizeRotation && Ship.RotationVelocityDegrees != 0)
            {
                float newRotVel = Ship.RotationVelocityDegrees;
                if (newRotVel > 0) //if rotating right
                    newRotVel -= STABILIZATION_FACTOR * newRotVel;
                if (newRotVel < 0) //if rotating left
                    newRotVel += STABILIZATION_FACTOR * -newRotVel;
                Ship.RotationVelocityDegrees = newRotVel; //and assign
            }

            float rotVel = Ship.RotationVelocityDegrees;
            Vector2 vel = Ship.Velocity; //temp vars, variable assignment workaround
            
            //Handle rotational input
            if (InputHandler.IsKeyDown(Keys.Right))
                rotVel += ROT_VEL_CHANGE; //rotate to the right
            if (InputHandler.IsKeyDown(Keys.Left))
                rotVel -= ROT_VEL_CHANGE; //and to the left

            //Handle acceleration
            if (InputHandler.IsKeyDown(Keys.Up))
                vel += Ship.Rotation.RotationToVectorFloat() * VEL_CHANGE_FACTOR;

            //return clamped rotational velocity
            Ship.RotationVelocityDegrees = MathHelper.Clamp(rotVel, -MAX_ROT_VEL, MAX_ROT_VEL);
            //return clamped velocity
            Ship.Velocity = Vector2.Clamp(vel, -VECTOR_VELOCITY_MAX, VECTOR_VELOCITY_MAX);

            Ship.Update(gameTime); //update the sprite itself, make sure to do this last
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            Ship.Draw(spriteBatch);
        }
    }

    public enum Slots
    {
        Slot1,
        Slot2
    }

    public enum Equipment
    {
        Empty,
        RailGun,
        Missile,
        Laser,
        Cannon
    }
}
