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
    public static class Player
    {
        #region Declarations

        public static GameObject Ship { get; set; }
        public static int Health { get; set; }

        public static ParticleEmitter LeftEngineTrail { get; set; }
        public static ParticleEmitter RightEngineTrail { get; set; }
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

        public const string ENGINE_SFX = "engine"; //sfx indexes
        public const string COLLISION_SFX = "collision";

        public const float VELOCITY_MAX = 500f; //max velocity
        public static Vector2 VECTOR_VELOCITY_MAX //max velocity to a Vector
        {
            get { return new Vector2(VELOCITY_MAX, VELOCITY_MAX); }
        }
        public const float MAX_ROT_VEL = 50f; //flight model
        public const float STABILIZATION_FACTOR = 0.08f;
        public const float ROT_VEL_CHANGE = 0.6f;
        public const float VEL_CHANGE_FACTOR = 4f;

        public static readonly Color[] TRAIL_COLORS = { Color.Orange, Color.OrangeRed, Color.MediumVioletRed };
        public static readonly Color[] EXPLOSION_COLORS = { Color.Gray, Color.Orange, Color.LightGray }; //effect colors

        public const float THRUST_OFFSET = -25f; //for trail offset calculation
        public const float ROTATION_OFFSET = 20f;

        public const int TRAIL_FTL = 30;
        public const int TRAIL_PPT = 1;
        public const float TRAIL_EJECTION_SPEED = 500f; //high ejection speed since braking looks weird otherwise
        public const float TRAIL_RANDOM_MARGIN = 0.01f;
        public const float TRAIL_SPRAYWIDTH = 1f; //only 1 degree spraywidth

        public const int STARTING_HEALTH = 100; //health and effect thresholds
        public const int MAX_HEALTH = 100;
        public const int MIN_HEALTH = 0;
        public const int DAMAGE_THRESHOLD = 35; //threshold of damage effect
        public const int ASTEROID_COLLISION_DAMAGE = 10;

        public const float SHIP_DEPTH = 0.5f; //draw depth

        public const float ROT_VEL_BOUNCE_CHANGE = 20f; //randomization for collision

        public const Equipment STARTING_EQUIP_SLOT1 = Equipment.Laser; //what to start with
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

            //init the ship
            Ship = new GameObject(
                ContentHandler.Textures[SHIP_TEXTURE],
                new Vector2(300, 300), //TEMP
                Vector2.Zero,
                Color.White,
                false,
                0f,
                0f,
                1f,
                SHIP_DEPTH,
                ContentHandler.Textures[SHIP_TEXTURE].GetMeanRadius(4, 1),
                0, 0, SpriteEffects.None, 8, 1, 8, 2);

            //init the left-side particle engine trail
            LeftEngineTrail = new ParticleEmitter(
                Int32.MaxValue,
                GameObject.GetOffset(Ship, THRUST_OFFSET, ROTATION_OFFSET),
                ContentHandler.Textures["particle"].ToTextureList(),
                TRAIL_COLORS.ToList<Color>(),
                TRAIL_FTL,
                false,
                true,
                -1,
                TRAIL_PPT,
                TRAIL_EJECTION_SPEED,
                TRAIL_RANDOM_MARGIN,
                0f,
                TRAIL_SPRAYWIDTH);

            //same for right side
            RightEngineTrail = new ParticleEmitter(
                Int32.MaxValue,
                GameObject.GetOffset(Ship, THRUST_OFFSET, -ROTATION_OFFSET),
                ContentHandler.Textures["particle"].ToTextureList(),
                TRAIL_COLORS.ToList<Color>(),
                TRAIL_FTL,
                false,
                true,
                -1,
                TRAIL_PPT,
                TRAIL_EJECTION_SPEED,
                TRAIL_RANDOM_MARGIN,
                0f,
                TRAIL_SPRAYWIDTH);
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
            if (InputHandler.IsKeyDown(Keys.Up)) //is accelerating?
            {
                vel += Ship.Rotation.RotationToVector() * VEL_CHANGE_FACTOR;
                LeftEngineTrail.Emitting = true; //enable trails
                RightEngineTrail.Emitting = true;

                Ship.Animating = true; //animate the ship
                if(ContentHandler.ShouldPlaySFX)
                    ContentHandler.PlaySFX(ENGINE_SFX); //start engine sound effect
            }
            else if (InputHandler.WasKeyDown(Keys.Up)) //just stopped accelerating?
            {
                LeftEngineTrail.Emitting = false; //turn off trails
                RightEngineTrail.Emitting = false;

                Ship.Animating = false; //stop animating
                Ship.CurrentFrame = 0; //set first frame

                ContentHandler.PauseInstancedSFX(ENGINE_SFX); //pause engine sound effect
            }

            //return clamped rotational velocity
            Ship.RotationVelocityDegrees = MathHelper.Clamp(rotVel, -MAX_ROT_VEL, MAX_ROT_VEL);
            //return clamped velocity
            Ship.Velocity = Vector2.Clamp(vel, -VECTOR_VELOCITY_MAX, VECTOR_VELOCITY_MAX);

            //LET Update
            LeftEngineTrail.DirectionInDegrees = Ship.RotationDegrees + 180;
            LeftEngineTrail.WorldPosition = GameObject.GetOffset(Ship, THRUST_OFFSET, ROTATION_OFFSET);
            //LeftEngineTrail.VelocityToInherit = Ship.Velocity / 2;

            //RET Update
            RightEngineTrail.DirectionInDegrees = Ship.RotationDegrees + 180;
            RightEngineTrail.WorldPosition = GameObject.GetOffset(Ship, THRUST_OFFSET, -ROTATION_OFFSET);
            //LeftEngineTrail.VelocityToInherit = Ship.Velocity / 2;

            LeftEngineTrail.Update(gameTime);
            RightEngineTrail.Update(gameTime);
            Ship.Update(gameTime); //update the sprite itself, make sure to do this last

            Camera.CenterPosition = Vector2.Clamp(Ship.WorldCenter, Camera.UL_CORNER, Camera.BR_CORNER);
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            LeftEngineTrail.Draw(spriteBatch);
            RightEngineTrail.Draw(spriteBatch);
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
