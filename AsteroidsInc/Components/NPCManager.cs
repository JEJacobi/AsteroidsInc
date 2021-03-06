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
    public static class NPCManager
    {
        public static List<NPC> NPCs; //Main NPC list
        public static List<ParticleEmitter> Effects;
        public static Dictionary<string, EquipmentData> NPCEquipment;
        public static List<Texture2D> EXPLOSION_TEXTURES;
        public static int RemainingNPCs { get { return NPCs.Count; } }

        public const string MINE_KEY = "mine";
        public const string DRONE_KEY = "drone";
        public const string FIGHTER_KEY = "fighter";
        public const string BOMBER_KEY = "bomber";
        public const string MUSIC_PREFIX = "combat";
        public const string DAMAGE_POSTFIX = "dmg";
        public const string SHIELD_TEXTURE = "shield";
        public const string DEATH_SFX = "death";

        const int WORLD_BORDER_GENERATION_BUFFER = 50;
        const int COLLISION_DAMAGE = 25;
        static readonly Color[] EXPLOSION_COLORS = { Color.Gray, Color.LightGray, Color.White, Color.DarkSlateGray };
        const int EXPLOSION_MAX_PARTICLES = 20;
        const int EXPLOSION_FTL = 50;
        const float EXPLOSION_EJECTION_SPEED = 20f;
        const float EXPLOSION_RANDOM_MARGIN = 0.1f;
        const float EXPLOSION_SPRAYWIDTH = ParticleEmitter.EXPLOSIONSPRAY;

        const int maxTries = 50;

        static NPCManager()
        {
            NPCs = new List<NPC>();
            Effects = new List<ParticleEmitter>();
            EXPLOSION_TEXTURES = new List<Texture2D>();
            NPCEquipment = new Dictionary<string, EquipmentData>();

            //TODO: Add actual AI weapons
            NPCEquipment.Add(DRONE_KEY, new EquipmentData(
                ContentHandler.Textures[Player.LASER_KEY],
                750,
                150,
                Player.LASER_KEY,
                Player.COLLISION_SFX,
                450,
                10,
                6,
                25,
                1,
                0,
                "Pulse Laser"));
            //TODO: Add other AI class weapons
        }

        public static void Generate(Level level)
        {
            NPCs = new List<NPC>();
            Effects = new List<ParticleEmitter>();

            //TODO: CHANGE
            EquipmentData temp = NPCEquipment[DRONE_KEY];

            for (int i = 0; i < level.Mines; i++)
                NPCs.Add(new NPC(
                    MINE_KEY,
                    MINE_KEY + DAMAGE_POSTFIX,
                    DEATH_SFX,
                    AIState.Mine,
                    getOffscreenPos(ContentHandler.Textures[MINE_KEY].Width, ContentHandler.Textures[MINE_KEY].Height),
                    Vector2.Zero,
                    Vector2.Zero,
                    Color.White,
                    50,
                    null,
                    ContentHandler.Textures[MINE_KEY].GetMeanRadius(3, 1),
                    200,
                    0, //TODO: Add attributes for all.
                    0,
                    100,
                    0.5f,
                    10,
                    0f,
                    0f,
                    3,
                    1,
                    3));

            for (int i = 0; i < level.Drones; i++)
                NPCs.Add(new NPC(
                    DRONE_KEY,
                    DRONE_KEY + DAMAGE_POSTFIX,
                    DEATH_SFX,
                    AIState.Attack,
                    getOffscreenPos(ContentHandler.Textures[DRONE_KEY].Width, ContentHandler.Textures[DRONE_KEY].Height),
                    Vector2.Zero,
                    Vector2.Zero,
                    Color.White,
                    50,
                    temp,
                    ContentHandler.Textures[DRONE_KEY].GetMeanRadius(),
                    500,
                    0,
                    0,
                    100,
                    5,
                    40));

            for (int i = 0; i < level.Fighters; i++)
                NPCs.Add(new NPC(
                    FIGHTER_KEY,
                    FIGHTER_KEY + DAMAGE_POSTFIX,
                    DEATH_SFX,
                    AIState.Random,
                    getOffscreenPos(ContentHandler.Textures[FIGHTER_KEY].Width, ContentHandler.Textures[FIGHTER_KEY].Height),
                    Vector2.Zero,
                    Vector2.Zero,
                    Color.White,
                    100,
                    NPCEquipment[FIGHTER_KEY],
                    ContentHandler.Textures[FIGHTER_KEY].GetMeanRadius(),
                    1000,
                    0,
                    0,
                    0,
                    8,
                    0,
                    0));

            for (int i = 0; i < level.Bombers; i++)
                NPCs.Add(new NPC(
                    BOMBER_KEY,
                    BOMBER_KEY + DAMAGE_POSTFIX,
                    DEATH_SFX,
                    AIState.Random,
                    getOffscreenPos(ContentHandler.Textures[BOMBER_KEY].Width, ContentHandler.Textures[BOMBER_KEY].Height),
                    Vector2.Zero,
                    Vector2.Zero,
                    Color.White,
                    500,
                    NPCEquipment[BOMBER_KEY],
                    ContentHandler.Textures[BOMBER_KEY].GetMeanRadius(),
                    1000,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0));
        }

        public static void Update(GameTime gameTime)
        {
            bool play = false;

            for (int i = 0; i < NPCs.Count; i++)
            {
                NPCs[i].Update(gameTime);

                if (NPCs[i].Ship.BoundingCircle.Intersects(Player.Ship.BoundingCircle))
                {
                    Player.Health -= COLLISION_DAMAGE;
                    Player.TriggerShield();
                    DestroyNPC(i);
                    break;
                }

                if (NPCs[i].Health <= 0)
                {
                    DestroyNPC(i);
                    break;
                }

                if (NPCs[i].Activated == true)
                    play = true;
            }

            if (play)
                ContentHandler.PlaySong(MUSIC_PREFIX + LevelManager.CurrentLevel.Music);
            else
                ContentHandler.StopMusic(true);

            for (int i = 0; i < Effects.Count; i++)
            {
                Effects[i].Update(gameTime);

                if (Effects[i].TimeToEmit == 0 && Effects[i].Particles.Count == 0)
                    Effects.RemoveAt(i);
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < NPCs.Count; i++)
                NPCs[i].Draw(spriteBatch);

            for (int i = 0; i < Effects.Count; i++)
                Effects[i].Draw(spriteBatch);
        }

        public static void DestroyNPC(int npcIndex)
        {
            if (npcIndex < 0 || npcIndex > NPCs.Count)
                throw new ArgumentOutOfRangeException();

            ContentHandler.PlaySFX(NPCs[npcIndex].DeathSoundKey);
            addExplosion(NPCs[npcIndex].Ship.WorldCenter, NPCs[npcIndex].Ship.BoundingCircle.Radius);
            NPCs.RemoveAt(npcIndex);
        }

        static void addExplosion(Vector2 pos, float effectRandom)
        {
            Effects.Add(new ParticleEmitter(
                EXPLOSION_MAX_PARTICLES,
                pos,
                EXPLOSION_TEXTURES,
                EXPLOSION_COLORS.ToList<Color>(),
                EXPLOSION_FTL,
                true,
                true,
                1,
                EXPLOSION_MAX_PARTICLES,
                EXPLOSION_EJECTION_SPEED,
                EXPLOSION_RANDOM_MARGIN,
                0f,
                EXPLOSION_SPRAYWIDTH,
                effectRandom));
        }

        static Vector2 getOffscreenPos(int textX, int textY)
        {
            if (Camera.WorldRectangle.Width < Camera.Viewport.Width || Camera.WorldRectangle.Height < Camera.Viewport.Height)
                throw new ArgumentOutOfRangeException("WorldSize", "Cannot generate enemies, world smaller than screen.");

            int counter = 0;
            bool flag = false;
            Vector2 output = new Vector2();

            do
            {
                output = new Vector2(
                    (float)Util.rnd.NextDouble(Camera.UL_CORNER.X + WORLD_BORDER_GENERATION_BUFFER, Camera.BR_CORNER.X - WORLD_BORDER_GENERATION_BUFFER),
                    (float)Util.rnd.NextDouble(Camera.UL_CORNER.Y + WORLD_BORDER_GENERATION_BUFFER, Camera.BR_CORNER.Y - WORLD_BORDER_GENERATION_BUFFER));

                if (counter > maxTries)
                    return output;

                Rectangle spawnBox = new Rectangle(
                    (int)Math.Round(output.X, 0) - (textX / 2),
                    (int)Math.Round(output.Y, 0) - (textY / 2),
                    textX,
                    textY);

                if (Camera.IsObjectVisible(spawnBox) == false)
                    flag = true;
            } while (!flag);

            return output;
        }
    }
}
