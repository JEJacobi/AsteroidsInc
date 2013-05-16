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
        public static int CurrentOre { get; set; }
        public static int StoredOre { get; set; }
        public static int OreWinCondition { get; set; }
        public static bool StabilizeRotation { get; set; } //slowly bring rotational velocity to zero?

        public static GameObject Shield { get; set; } //overlaid shield sprite
        public static ParticleEmitter LeftEngineTrail { get; set; } //emitter for left engine trail
        public static ParticleEmitter RightEngineTrail { get; set; } //same for right
        public static ParticleEmitter ExplosionEmitter { get; set; } //activated on death

        public static Dictionary<string, EquipmentData> EquipmentDictionary { get; set; }
        //dictionary to hold all equipment data

        public static string Slot1 { get; set; } //keys for equipment dictionary
        public static string Slot2 { get; set; }
        public static Slots ActiveSlot { get; set; } //which key is being used

        public static event EventHandler DeadEvent;
        public static event EventHandler LevelCompleteEvent;

        static List<Texture2D> explosionParticles = new List<Texture2D>();
        static int shotDelay = 0; //counter variable
        static int shieldFade = 0;
        static bool dead;
        static bool play;

        #endregion

        #region Constants

        public const string SHIP_TEXTURE = "ship"; //texture indexes
        public const string SHIELD_KEY = "shield";
        public const string MISSILE_KEY = "missile";
        public const string LASER_KEY = "laser";
        public const string CANNON_KEY = "cannon";
        public const string SHELL_KEY = "shell";
        public const string SLIVER_KEY = "sliver";

        public const string ENGINE_SFX = "engine"; //sfx indexes
        public const string COLLISION_SFX = "collision";
        public const string IMPACT_SFX = "quickimpact";
        public const string DEATH_SFX = "death";
        public const string WIN_SFX = "win";

        public const float VELOCITY_MAX = 500f; //max velocity
        public static Vector2 VECTOR_VELOCITY_MAX //max velocity to a Vector
        {
            get { return new Vector2(VELOCITY_MAX, VELOCITY_MAX); }
        }
        public const float MAX_ROT_VEL = 50f; //flight model
        public const float STABILIZATION_FACTOR = 0.08f;
        public const float ROT_VEL_CHANGE = 0.6f;
        public const float VEL_CHANGE_FACTOR = 4f;
        public const float SHIP_FRAME_DELAY = 0.7f;

        public static readonly Color[] TRAIL_COLORS = { Color.Orange, Color.OrangeRed, Color.MediumVioletRed };
        public static readonly Color[] EXPLOSION_COLORS = { Color.Gray, Color.Orange, Color.LightGray }; //effect colors

        public const float THRUST_OFFSET = -25f; //for trail offset calculation
        public const float ROTATION_OFFSET = 20f;
        public const float FIRE_OFFSET = 30f;

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

        public const int STARTING_ORE = 0;

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

            Slot1 = SHELL_KEY; //set initial equipment
            Slot2 = SLIVER_KEY;
            EquipmentDictionary = new Dictionary<string, EquipmentData>(); //initialize the dictionary

            //add missiles
            EquipmentDictionary.Add(MISSILE_KEY, new EquipmentData(
                ContentHandler.Textures[MISSILE_KEY],
                300,
                0f,
                MISSILE_KEY,
                COLLISION_SFX,
                2000,
                50,
                20,
                50,
                1,
                0,
                true, true));

            //add lasers
            EquipmentDictionary.Add(LASER_KEY, new EquipmentData(
                ContentHandler.Textures[LASER_KEY],
                750,
                150,
                LASER_KEY,
                COLLISION_SFX,
                450,
                10,
                6,
                5,
                1,
                0));

            //add shell
            EquipmentDictionary.Add(SHELL_KEY, new EquipmentData(
                ContentHandler.Textures[SHELL_KEY],
                1000,
                300,
                SHELL_KEY,
                COLLISION_SFX,
                225,
                10,
                3,
                35,
                35,
                20,
                false, true));

            //add rapid-fire sliver
            EquipmentDictionary.Add(SLIVER_KEY, new EquipmentData(
                ContentHandler.Textures[SLIVER_KEY],
                450,
                50,
                SLIVER_KEY,
                IMPACT_SFX,
                400,
                1,
                5,
                1,
                2,
                45));

            #endregion
            
            Health = STARTING_HEALTH;
            CurrentOre = STARTING_ORE;

            OreWinCondition = 20; //TEMP

            ActiveSlot = INITIAL_ACTIVE_SLOT;
            StabilizeRotation = true;

            #region Component Initialization

            //init the ship
            Ship = new GameObject(
                ContentHandler.Textures[SHIP_TEXTURE],
                Camera.CENTER_OF_WORLD,
                Vector2.Zero,
                Color.White,
                false,
                0f,
                0f,
                1f,
                SHIP_DEPTH,
                false,
                ContentHandler.Textures[SHIELD_KEY].GetMeanRadius(), //use the shield texture as a collision boundary
                0, 0, SpriteEffects.None, 8, 1, 8, 2, SHIP_FRAME_DELAY);

            //init the shield overlay
            Shield = new GameObject(
                ContentHandler.Textures[SHIELD_KEY],
                Ship.WorldCenter,
                Ship.Velocity,
                Color.Transparent);

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
            explosionParticles.Add(ContentHandler.Textures["junk1"]);
            explosionParticles.Add(ContentHandler.Textures["junk2"]);
            explosionParticles.Add(ContentHandler.Textures["junk3"]);

            ExplosionEmitter = new ParticleEmitter(
                EXPLOSION_PARTICLES_TO_EMIT,
                Ship.WorldCenter,
                explosionParticles,
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

                if (shieldFade != 0)
                {
                    Shield.TintColor = Color.Lerp(Color.Transparent, Color.White, (float)shieldFade / 100);
                    shieldFade--;
                }
                else if (shieldFade == 0) //if shield has faded, set to transparent
                    Shield.TintColor = Color.Transparent;

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
                //Shield Overlay Update
                Shield.WorldCenter = Ship.WorldCenter;
                Shield.Velocity = Ship.Velocity;
                Shield.RotationalVelocity = Ship.RotationalVelocity;
                Shield.Rotation = Ship.Rotation;

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
                Shield.Update(gameTime);
                Ship.Update(gameTime); //update the sprite itself, make sure to do this last
            }

            if (CurrentOre >= OreWinCondition) //if level complete
            {
                if (LevelCompleteEvent != null) //trigger the level complete event
                    LevelCompleteEvent(Ship, EventArgs.Empty);

                if (ContentHandler.ShouldPlaySFX)
                {
                    SoundEffectInstance temp = ContentHandler.SFX[WIN_SFX].CreateInstance();
                    temp.Play(); //play level win sound

                    while (temp.State == SoundState.Playing) ; //wait until done playing
                }

                Reset(); //reset
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
                Shield.Draw(spriteBatch);
                Ship.Draw(spriteBatch);
            }
            ExplosionEmitter.Draw(spriteBatch); //out of if dead check since activated on death
        }

        public static void Reset() //reset the player, location, equipment and explosion emitter
        {
            StoredOre += CurrentOre;

            //reset the ship object
            Ship.Animating = false;
            Ship.CurrentFrame = 0;
            Ship.WorldCenter = Camera.CENTER_OF_WORLD;
            Ship.RotationalVelocity = 0f;
            Ship.Rotation = 0f;
            Ship.Velocity = Vector2.Zero;

            //the engine trails
            LeftEngineTrail.Emitting = false;
            RightEngineTrail.Emitting = false;

            //equipment and flags
            ActiveSlot = INITIAL_ACTIVE_SLOT;
            Health = STARTING_HEALTH;
            CurrentOre = STARTING_ORE;
            dead = false;
            StabilizeRotation = true;
            Ship.Active = true;

            //Ore Manager's stuff
            OreManager.OreDrops = new List<Particle>();

            //Projectile Manager
            ProjectileManager.Projectiles = new List<Projectile>();

            //and the on-death particle emitter
            ExplosionEmitter = new ParticleEmitter(
                EXPLOSION_PARTICLES_TO_EMIT,
                Ship.WorldCenter,
                explosionParticles,
                EXPLOSION_COLORS.ToList<Color>(),
                EXPLOSION_FTL,
                false,
                true,
                EXPLOSION_TIME_TO_EMIT,
                EXPLOSION_PARTICLES_TO_EMIT / EXPLOSION_TIME_TO_EMIT,
                EXPLOSION_EJECTION_SPEED,
                EXPLOSION_RANDOMIZATION,
                0f, 180f);
            ExplosionEmitter.WorldPosition = Ship.WorldCenter;
        }

        public static void TriggerShield(int fadeVal = 100, bool playSound = true)
        {
            if(playSound)
                ContentHandler.PlaySFX(SHIELD_KEY);

            shieldFade = fadeVal;
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
        public int ShotsPerLaunch { get; set; }
        public bool DetonateEffect { get; set; }
        public bool TrackVelocity { get; set; }
        public string Description { get; set; }
        public int OreCost { get; set; }

        public EquipmentData(
            Texture2D texture,
            float speed,
            float randomization,
            string launchSound,
            string hitSound,
            int maxRange,
            int damage,
            int cRadius,
            int refireDelay,
            int shotsPerLaunch,
            int oreCost,
            bool detonateEffect = false,
            bool trackVelocity = false,
            string description = "")
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
            ShotsPerLaunch = shotsPerLaunch;
            DetonateEffect = detonateEffect;
            TrackVelocity = trackVelocity;
            Description = description;
            OreCost = oreCost;
        }
    }

    public enum Slots
    {
        Slot1,
        Slot2
    }
}
