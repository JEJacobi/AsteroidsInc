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
    public class NPC
    {
        public GameObject Ship;
        public GameObject Shield;
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
        public readonly int ActivationRadius;
        public readonly string DamageSoundKey;
        public readonly string DeathSoundKey;
        public bool Activated;

        ParticleEmitter trail;

        //AI data
        Action stateLogic;
        public FoF_Ident Identification;
        public Vector2 Target = Vector2.Zero;
        public Vector2 WorldTarget //sets the target from a world coordinate, instead of relative coordinates
        {
            set
            {
                Target = Vector2.Normalize(value - Ship.WorldCenter);
            }
        }
        bool attacking = false;
        bool accelerating = false;
        bool firing = false;
        readonly int firedelay;
        int dmgThreshold = 0;
        int distanceThreshold = 0;
        int reattackThreshold = 0;
        int firecounter = 0;
        int shieldfade = 0;
        int randCounter = 0;
        int randTargetTime;
        Projectile outProjectile;

        //Misc constants
        const float NPC_DEPTH = 0.5f;
        const int REMAINING_NPC_THRESHOLD_DEVISOR = 4;
        const int RAND_MAX_TIME = 1000;
        const int EVADE_DMG_THRESHOLD_MIN = 20;
        const int EVADE_DMG_THRESHOLD_MAX = 50;
        const int DISTANCE_THRESHOLD_MIN = 15;
        const int DISTANCE_THRESHOLD_MAX = 100;
        const int REATTACK_THRESHOLD_MIN = 300;
        const int REATTACK_THRESHOLD_MAX = 1000;

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
            string texturekey,
            string damagekey,
            string deathkey,
            AIState initialState,
            Vector2 initialPos,
            Vector2 initialVel,
            Vector2 initialTarget,
            Color tintColor,
            int startingHealth,
            EquipmentData equip,
            int colRadius,
            int activationRadius,
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
            FoF_Ident ident = FoF_Ident.Enemy,
            bool activated = false)
        {
            Ship = new GameObject(
                ContentHandler.Textures[texturekey],
                initialPos,
                initialVel,
                tintColor,
                (totalFrames != 0 ? true : false),
                initialRot,
                initialRotVel,
                1f,
                NPC_DEPTH,
                false,
                colRadius,
                0, 0,
                SpriteEffects.None,
                totalFrames,
                rows,
                columns,
                startingFrame,
                frameDelay);

            Shield = new GameObject(
                ContentHandler.Textures[texturekey + NPCManager.SHIELD_TEXTURE],
                Ship.WorldCenter,
                Ship.Velocity,
                Color.Transparent,
                false,
                Ship.Rotation,
                Ship.RotationalVelocity,
                Ship.Scale,
                Ship.Depth);  

            CurrentState = initialState;
            LastState = initialState;
            DamageSoundKey = damagekey;
            DeathSoundKey = deathkey;
            Weapon = equip;
            if (equip != null) { firedelay = equip.RefireDelay; }
            ActivationRadius = activationRadius;
            Activated = activated;
            TrailOffset = trailOffset;
            WeaponOffset = weaponOffset;
            MaxSpeed = maxSpeed;
            TrackSpeed = trackSpeed;
            AccelerationSpeed = maxAccelSpeed;
            Target = Vector2.Normalize(initialTarget);

            Identification = ident;
            StartingHealth = startingHealth;
            Health = StartingHealth;

            trail = new ParticleEmitter( //initialize the engine trail particle emitter
                TRAIL_MAX_PARTICLES,
                GameObject.GetOffset(Ship, TrailOffset),
                ContentHandler.Textures[TRAIL_PARTICLE].ToTextureList(),
                TRAIL_COLORS.ToList<Color>(),
                TRAIL_FTL,
                false,
                true,
                -1,
                TRAIL_PPT,
                TRAIL_EJECTION_SPEED,
                TRAIL_RANDOM_MARGIN,
                Ship.RotationDegrees + 180,
                TRAIL_SPRAYWIDTH);

            //initialize the state logic
            switchState(initialState);

            //pick a slightly randomized evasion damage threshold
            dmgThreshold = Util.rnd.Next(
                EVADE_DMG_THRESHOLD_MIN,
                EVADE_DMG_THRESHOLD_MAX);

            //pick a randomized attack run termination distance
            distanceThreshold = Util.rnd.Next(
                DISTANCE_THRESHOLD_MIN,
                DISTANCE_THRESHOLD_MAX);

            //pick a randomized attack run repeat distance
            reattackThreshold = Util.rnd.Next(
                REATTACK_THRESHOLD_MIN,
                REATTACK_THRESHOLD_MAX);
        }

        public void Update(GameTime gameTime)
        {
            //handle target tracking
            if (Target != Vector2.Zero) 
                Ship.VectorTrack(Target, TrackSpeed);

            Shield.WorldCenter = Ship.WorldCenter;
            Shield.Velocity = Ship.Velocity;

            if (shieldfade != 0) //also ripped from Player
            {
                Shield.TintColor = Color.Lerp(Color.Transparent, Color.White, (float)shieldfade / 100);
                shieldfade--;
            }

            //handle accelerating
            if (accelerating) 
            {
                Ship.Velocity += Ship.Rotation.RotationToVector() * AccelerationSpeed; //accelerate the ship
                Ship.Velocity = Vector2.Clamp( //clamp the velocity to the maxiumum speed
                    Ship.Velocity,
                    new Vector2(-MaxSpeed),
                    new Vector2(MaxSpeed));
                trail.Emitting = true; //activate the engine trail
            }
            else
            {
                trail.Emitting = false;
            }

            //handle firing
            if (firecounter > 0)
                firecounter--;

            if (firing && Weapon != null && firecounter == 0)
            {
                ProjectileManager.AddShot(
                    Weapon,
                    GameObject.GetOffset(Ship, WeaponOffset),
                    Ship.Rotation,
                    Ship.Velocity,
                    Identification);

                //reset the refire delay
                firecounter = Weapon.RefireDelay;
            }

            if(ProjectileManager.IsHit(Ship, out outProjectile, FoF_Ident.Enemy))
            {
                Activated = true;
                Health -= outProjectile.Damage;
                TriggerShield();
            }

            stateLogic(); //logic delegate
            checkActivation(); //check for activation
            handleStateChange(); //handle changes in conditionals

            Ship.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Ship.Draw(spriteBatch);
            Shield.Draw(spriteBatch);
        }

        public void TriggerShield(int fadeVal = 50, bool playSound = true)
        {
            if (playSound)
                ContentHandler.PlaySFX(DamageSoundKey);

            shieldfade = fadeVal;
        }

        private void handleStateChange() //checks if new states have been met
        {
            if (CurrentState == AIState.Ram)
                return; //no state changes while ramming

            if (Activated)
            {
                if (CurrentState == AIState.Mine)
                    switchState(AIState.Ram); //if NPC is a passive mine, switch to track/ram

                if (CurrentState == AIState.Attack && distanceThresholdReached(distanceThreshold))
                    attacking = false; //if attack run complete, evade

                if (CurrentState == AIState.Attack && distanceThresholdReached(reattackThreshold))
                    attacking = true; //if sufficient distance attained, start attack run
                //TODO: Add non distance related ways to reattain attack state

                if (Health <= dmgThreshold)
                {
                    switchState(AIState.Evade); //if health is under a threshold, evade player

                    if (NPCManager.RemainingNPCs <= LevelManager.CurrentLevel.TotalNPCs / REMAINING_NPC_THRESHOLD_DEVISOR &&
                        distanceThresholdReached(distanceThreshold))
                        switchState(AIState.Ram); 
                    //if low on health, not many NPCs left,
                    //and under a distance threshold: ram the player
                }

            }
            else
            {
                if (CurrentState != AIState.Mine && CurrentState != AIState.Wait)
                    switchState(AIState.Random); //TODO: Add something including Regroup here
            }
        }

        private void switchState(AIState target)
        {
            LastState = CurrentState;

            if (CurrentState != target)
                CurrentState = target;

            switch (target)
            {
                case AIState.Attack:
                    stateLogic = new Action(state_attack);
                    break;
                case AIState.Evade:
                    stateLogic = new Action(state_evade);
                    break;
                case AIState.Random:
                    stateLogic = new Action(state_random);
                    break;
                case AIState.Regroup:
                    stateLogic = new Action(state_regroup);
                    break;
                case AIState.Ram:
                    stateLogic = new Action(state_ram);
                    break;
                case AIState.Wait:
                    stateLogic = new Action(state_wait);
                    break;
                case AIState.Mine: //mine is functionally identical to wait, except in the state changers
                    stateLogic = new Action(state_wait);
                    break;
                default:
                    break;
            }
        }

        #region State Handlers

        private void state_attack()
        {
            if (attacking)
            {
                WorldTarget = Player.Ship.WorldCenter + Player.Ship.Velocity;
                accelerating = true;
                firing = true;
            }
            else
                state_evade();
        }
        private void state_evade()
        {
            if (Player.Ship.WorldCenter.X < Ship.WorldCenter.X)
                Target.X++;
            if (Player.Ship.WorldCenter.X > Ship.WorldCenter.X)
                Target.X--;
            if (Player.Ship.WorldCenter.Y < Ship.WorldCenter.Y)
                Target.Y++;
            if (Player.Ship.WorldCenter.Y > Ship.WorldCenter.Y)
                Target.Y--;

            Target = Vector2.Normalize(Target);

            accelerating = true;
            firing = false;
        }
        private void state_random() //fly to a random location and repeat
        {
            randCounter++;
            if (randCounter >= randTargetTime)
            {
                randCounter = 0;
                randTargetTime = Util.rnd.Next(RAND_MAX_TIME);
                Target = Vector2.Normalize(new Vector2( //get a new normalized v2 to fly to
                    (float)Util.rnd.NextDouble(-1, 1),
                    (float)Util.rnd.NextDouble(-1, 1)));
            }
            accelerating = true;
            firing = false;
            attacking = false;
        }
        private void state_regroup()
        {
            throw new NotImplementedException();
        }
        private void state_ram() //just ram the player
        {
            WorldTarget = Player.Ship.WorldCenter + Player.Ship.Velocity;
            accelerating = true;
            firing = false;
            attacking = false;
        }
        private void state_wait() //wait at a specific point
        {
            Target = new Vector2(0, -1); //just rotate to 0 degrees
            firing = false;
            accelerating = false;
            attacking = false;
        }
            
        #endregion

        private void checkActivation()
        {
            if (Identification == FoF_Ident.Enemy)
            {
                if (Circle.Intersects( //if the activation radius touches the player's ship
                    new Circle(Ship.WorldCenter, this.ActivationRadius),
                    Player.Ship.BoundingCircle))
                    Activated = true;
            }

            var enemies = //yay, LINQ!
                from ship in NPCManager.NPCs //select any ship that is an enemy and not neutral
                where ship.Identification != this.Identification && ship.Identification != FoF_Ident.Neutral
                select ship;

            foreach (NPC enemy in enemies)
            {
                if (Circle.Intersects(
                    new Circle(Ship.WorldCenter, this.ActivationRadius),
                    Player.Ship.BoundingCircle))
                    Activated = true;
            }
        }

        private bool distanceThresholdReached(int distance)
        {
            Circle threshold = new Circle(Ship.WorldCenter, distance);

            if (threshold.Intersects(Player.Ship.BoundingCircle))
                return true;
            else
                return false;
        }
    }

    public enum AIState
    {
        Attack,
        Evade,
        Random,
        Regroup,
        Ram,
        Wait,
        Mine
    }
}
