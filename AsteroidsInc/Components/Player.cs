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

        public static GameObject Ship { get; set; } //main player sprite
        public static int Health { get; set; }
        public static bool StabilizeRotation { get; set; }

        public static ParticleEmitter LeftEngineTrail { get; set; }
        public static ParticleEmitter RightEngineTrail { get; set; }
        public static ParticleEmitter ExplosionEmitter { get; set; }

        public static Dictionary<string, EquipmentData> EquipmentDictionary { get; set; }
        //dictionary to hold all equipment data

        public static string Slot1 { get; set; } //keys for equipment dictionary
        public static string Slot2 { get; set; }
        public static Slots ActiveSlot { get; set; } //which key is being used

        public static event EventHandler DeadEvent;

        static int shotDelay = 0; //counter variable
        static bool dead;
        static bool play;

        #endregion

        #region Constants

        public const string SHIP_TEXTURE = "ship"; //texture indexes
        public const string MISSILE_KEY = "missile";
        public const string LASER_KEY = "laser";
        public const string CANNON_KEY = "cannon";

        public const string ENGINE_SFX = "engine"; //sfx indexes
        public const string COLLISION_SFX = "collision";
        public const string DEATH_SFX = "death";

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
        public const float FIRE_OFFSET = 20f;

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

        public const int MISSILE_FIRE_DELAY = 50;
        public const int LASER_FIRE_DELAY = 5;

        public const float MISSILE_VELOCITY = 300;
        public const float LASER_VELOCITY = 750;

        public const int MISSILE_MAX_RANGE = 2000;
        public const int LASER_MAX_RANGE = 400;

        public const int MISSILE_DAMAGE = 50;
        public const int LASER_DAMAGE = 5;

        public const float MISSILE_RANDOM = 0f;
        public const float LASER_RANDOM = 150f;

        public const int MISSILE_COL_RADIUS = 20;
        public const int LASER_COL_RADIUS = 8;

        public const float SHIP_DEPTH = 0.5f; //draw depth

        public const float ROT_VEL_BOUNCE_CHANGE = 20f; //randomization for collision

        public const int EXPLOSION_PARTICLES_TO_EMIT = 400;
        public const int EXPLOSION_FTL = 200;
        public const int EXPLOSION_TIME_TO_EMIT = 1;
        public const float EXPLOSION_EJECTION_SPEED = 50f;
        public const float EXPLOSION_RANDOMIZATION = 2f;

        public const Slots INITIAL_ACTIVE_SLOT = Slots.Slot1;

        #endregion

        public static void Initialize()
        {
            #region Equipment Definitions

            Slot1 = MISSILE_KEY; //set initial equipment
            Slot2 = LASER_KEY;
            EquipmentDictionary = new Dictionary<string, EquipmentData>(); //initialize the dictionary

            //add missiles
            EquipmentDictionary.Add(MISSILE_KEY, new EquipmentData(
                ContentHandler.Textures[MISSILE_KEY],
                MISSILE_VELOCITY,
                MISSILE_RANDOM,
                MISSILE_KEY,
                COLLISION_SFX,
                MISSILE_MAX_RANGE,
                MISSILE_DAMAGE,
                MISSILE_COL_RADIUS,
                MISSILE_FIRE_DELAY));

            //add lasers
            EquipmentDictionary.Add(LASER_KEY, new EquipmentData(
                ContentHandler.Textures[LASER_KEY],
                LASER_VELOCITY,
                LASER_RANDOM,
                LASER_KEY,
                COLLISION_SFX,
                LASER_MAX_RANGE,
                LASER_DAMAGE,
                LASER_COL_RADIUS,
                LASER_FIRE_DELAY));

            #endregion
            
            Health = STARTING_HEALTH;
            ActiveSlot = INITIAL_ACTIVE_SLOT;
            StabilizeRotation = true;

            #region Component Initialization

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

            //explosion particle textures
            List<Texture2D> temp = new List<Texture2D>();
            temp.Add(ContentHandler.Textures["junk1"]);
            temp.Add(ContentHandler.Textures["junk2"]);
            temp.Add(ContentHandler.Textures["junk3"]);

            ExplosionEmitter = new ParticleEmitter(
                EXPLOSION_PARTICLES_TO_EMIT,
                Ship.WorldCenter,
                temp,
                EXPLOSION_COLORS.ToList<Color>(),
                EXPLOSION_FTL,
                false,
                true,
                EXPLOSION_TIME_TO_EMIT,
                EXPLOSION_PARTICLES_TO_EMIT / EXPLOSION_TIME_TO_EMIT,
                EXPLOSION_EJECTION_SPEED,
                EXPLOSION_RANDOMIZATION,
                0f, 180f);

            #endregion
        }

        public static void Update(GameTime gameTime)
        {
            if (dead == false)
            {
                if (shotDelay != 0)
                    shotDelay--;

                float rotVel = Ship.RotationVelocityDegrees;
                Vector2 vel = Ship.Velocity; //temp vars, variable assignment workaround

                //LET Update
                LeftEngineTrail.DirectionInDegrees = Ship.RotationDegrees + 180;
                LeftEngineTrail.WorldPosition = GameObject.GetOffset(Ship, THRUST_OFFSET, ROTATION_OFFSET);
                //LeftEngineTrail.VelocityToInherit = Ship.Velocity / 2;

                //RET Update
                RightEngineTrail.DirectionInDegrees = Ship.RotationDegrees + 180;
                RightEngineTrail.WorldPosition = GameObject.GetOffset(Ship, THRUST_OFFSET, -ROTATION_OFFSET);
                //LeftEngineTrail.VelocityToInherit = Ship.Velocity / 2;

                #region Input

                //Handle firing
                if (shotDelay == 0 && InputHandler.IsKeyDown(Keys.Space))
                {
                    ProjectileManager.AddShot(
                        EquipmentDictionary[getActiveSlot()],
                        GameObject.GetOffset(Ship, FIRE_OFFSET),
                        Ship.Rotation,
                        Ship.Velocity,
                        FoF_Ident.Friendly);

                    shotDelay = EquipmentDictionary[getActiveSlot()].RefireDelay;
                    //set the refire delay to whatever equipment is equipped
                }

                //Handle rotational input
                if (InputHandler.IsKeyDown(Keys.Right))
                    rotVel += ROT_VEL_CHANGE; //rotate to the right
                if (InputHandler.IsKeyDown(Keys.Left))
                    rotVel -= ROT_VEL_CHANGE; //and to the left

                //Handle acceleration
                if (InputHandler.IsKeyDown(Keys.Up)) //is accelerating?
                {
                    float accelAmount;

                    if (InputHandler.IsKeyDown(Keys.LeftShift)) //BOOOOST!
                    {
                        accelAmount = VEL_CHANGE_FACTOR * 2;
                        ContentHandler.InstanceSFX[ENGINE_SFX].Pitch = 1f; //up the pitch by one octave to show boosting
                    }
                    else
                    {
                        ContentHandler.InstanceSFX[ENGINE_SFX].Pitch = 0f; //reset the pitch
                        accelAmount = VEL_CHANGE_FACTOR; //normal thrust
                    }

                    vel += Ship.Rotation.RotationToVector() * accelAmount;
                    LeftEngineTrail.Emitting = true; //enable trails
                    RightEngineTrail.Emitting = true;

                    Ship.Animating = true; //animate the ship

                    if (ContentHandler.ShouldPlaySFX)
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

                //handle equipment switching
                if (InputHandler.IsNewKeyPress(Keys.F))
                {
                    switch (ActiveSlot)
                    {
                        case Slots.Slot1:
                            ActiveSlot = Slots.Slot2;
                            break;
                        case Slots.Slot2:
                            ActiveSlot = Slots.Slot1;
                            break;
                        default:
                            throw new ArgumentException();
                    }
                    ContentHandler.PlaySFX("switch");
                }

                //return clamped rotational velocity
                Ship.RotationVelocityDegrees = MathHelper.Clamp(rotVel, -MAX_ROT_VEL, MAX_ROT_VEL);
                //return clamped velocity
                Ship.Velocity = Vector2.Clamp(vel, -VECTOR_VELOCITY_MAX, VECTOR_VELOCITY_MAX);

                #endregion

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

                Camera.CenterPosition = Vector2.Clamp(Ship.WorldCenter, Camera.UL_CORNER, Camera.BR_CORNER); //center the camera
                LeftEngineTrail.Update(gameTime); //update the trails
                RightEngineTrail.Update(gameTime);
                Ship.Update(gameTime); //update the sprite itself, make sure to do this last
            }

            if (Health <= MIN_HEALTH)
            {
                dead = true;
                Ship.Active = false;
                ExplosionEmitter.WorldPosition = Ship.WorldCenter;

                if (play == false)
                {
                    ContentHandler.StopAll();
                    ExplosionEmitter.Emitting = true;
                    ContentHandler.PlaySFX(DEATH_SFX);
                    play = true;
                }
                else
                {
                    if (ContentHandler.InstanceSFX[DEATH_SFX].State == SoundState.Stopped &&
                        DeadEvent != null)
                        DeadEvent(Ship, EventArgs.Empty); //if done playing call DeadEvent
                }

                ExplosionEmitter.Update(gameTime);
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            if (dead == false)
            {
                LeftEngineTrail.Draw(spriteBatch);
                RightEngineTrail.Draw(spriteBatch);
                Ship.Draw(spriteBatch);
            }
            ExplosionEmitter.Draw(spriteBatch);
        }

        static string getActiveSlot()
        {
            switch (ActiveSlot)
            {
                case Slots.Slot1:
                    return Slot1;
                case Slots.Slot2:
                    return Slot2;
                default:
                    throw new ArgumentException();
            }
        }
    }

    public class EquipmentData
    {
        public Texture2D Texture { get; set; }
        public float Speed { get; set; }
        public float Randomization { get; set; }
        public string LaunchSoundIndex { get; set; }
        public string HitSoundIndex { get; set; }
        public int MaxRange { get; set; }
        public int Damage { get; set; }
        public int CollisionRadius { get; set; }
        public int RefireDelay { get; set; }

        public EquipmentData(
            Texture2D texture,
            float speed,
            float randomization,
            string launchSound,
            string hitSound,
            int maxRange,
            int damage,
            int cRadius,
            int refireDelay)
        {
            Texture = texture;
            Speed = speed;
            Randomization = randomization;
            LaunchSoundIndex = launchSound;
            HitSoundIndex = hitSound;
            MaxRange = maxRange;
            Damage = damage;
            CollisionRadius = cRadius;
            RefireDelay = refireDelay;
        }
    }

    public enum Slots
    {
        Slot1,
        Slot2
    }
}
