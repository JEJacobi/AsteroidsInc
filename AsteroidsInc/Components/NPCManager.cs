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
    public static class NPCManager
    {
        public static List<NPC> NPCs; //Main NPC list
        public static List<ParticleEmitter> Effects;
        public static Dictionary<string, EquipmentData> NPCEquipment;
        public static List<Texture2D> EXPLOSION_TEXTURES;

        public const string MINE_KEY = "mine";
        public const string DRONE_KEY = "drone";
        public const string FIGHTER_KEY = "fighter";
        public const string BOMBER_KEY = "bomber";

        static readonly Color[] EXPLOSION_COLORS = { Color.Gray, Color.LightGray, Color.White, Color.DarkSlateGray };
        const int EXPLOSION_MAX_PARTICLES = 3;
        const int EXPLOSION_FTL = 20;
        const float EXPLOSION_EJECTION_SPEED = 20f;
        const float EXPLOSION_RANDOM_MARGIN = 0.1f;
        const float EXPLOSION_SPRAYWIDTH = ParticleEmitter.EXPLOSIONSPRAY;

        const int maxTries = 50;
        static Random rnd = new Random();

        static NPCManager()
        {
            NPCs = new List<NPC>();
            NPCEquipment = new Dictionary<string, EquipmentData>();
            Effects = new List<ParticleEmitter>();
            EXPLOSION_TEXTURES = new List<Texture2D>();
        }

        public static void Generate(Level level)
        {
            NPCs = new List<NPC>();
            Effects = new List<ParticleEmitter>();

            for (int i = 0; i < level.Mines; i++)
                NPCs.Add(new NPC(
                    ContentHandler.Textures[MINE_KEY],
                    AIState.Passive,
                    getOffscreenPos(ContentHandler.Textures[MINE_KEY].Width, ContentHandler.Textures[MINE_KEY].Height),
                    Vector2.Zero,
                    Vector2.Zero,
                    Color.White,
                    10,
                    NPCEquipment[MINE_KEY],
                    ContentHandler.Textures[MINE_KEY].GetMeanRadius(),
                    0, //TODO: Add attributes for all.
                    0,
                    60,
                    1,
                    10));

            for (int i = 0; i < level.Drones; i++)
                NPCs.Add(new NPC(
                    ContentHandler.Textures[DRONE_KEY],
                    AIState.Random,
                    getOffscreenPos(ContentHandler.Textures[DRONE_KEY].Width, ContentHandler.Textures[DRONE_KEY].Height),
                    Vector2.Zero,
                    Vector2.Zero,
                    Color.White,
                    50,
                    NPCEquipment[DRONE_KEY],
                    ContentHandler.Textures[DRONE_KEY].GetMeanRadius(),
                    0,
                    0,
                    100,
                    40,
                    40));

            for (int i = 0; i < level.Fighters; i++)
                NPCs.Add(new NPC(
                    ContentHandler.Textures[FIGHTER_KEY],
                    AIState.Random,
                    getOffscreenPos(ContentHandler.Textures[FIGHTER_KEY].Width, ContentHandler.Textures[FIGHTER_KEY].Height),
                    Vector2.Zero,
                    Vector2.Zero,
                    Color.White,
                    100,
                    NPCEquipment[FIGHTER_KEY],
                    0,
                    0,
                    0,
                    0,
                    0,
                    0));

            for (int i = 0; i < level.Bombers; i++)
                NPCs.Add(new NPC(
                    ContentHandler.Textures[BOMBER_KEY],
                    AIState.Random,
                    getOffscreenPos(ContentHandler.Textures[BOMBER_KEY].Width, ContentHandler.Textures[BOMBER_KEY].Height),
                    Vector2.Zero,
                    Vector2.Zero,
                    Color.White,
                    500,
                    NPCEquipment[BOMBER_KEY],
                    0,
                    0,
                    0,
                    0,
                    0,
                    0));
        }

        public static void Update(GameTime gameTime)
        {
            for (int i = 0; i < NPCs.Count; i++)
            {
                NPCs[i].Update(gameTime);

                if (NPCs[i].Health <= 0)
                {
                    addExplosion(NPCs[i].WorldCenter);
                    NPCs.RemoveAt(i);
                }
            }

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

        static void addExplosion(Vector2 pos)
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
                EXPLOSION_SPRAYWIDTH));
        }

        static Vector2 getOffscreenPos(int textX, int textY)
        {
            if (Camera.WorldRectangle.X < Camera.Viewport.X || Camera.WorldRectangle.Y < Camera.Viewport.Y)
                throw new ArgumentOutOfRangeException("WorldSize", "Cannot generate enemies, world smaller than screen.");

            int counter = 0;
            bool flag = false;
            Vector2 output = new Vector2();

            do
            {
                output = new Vector2(
                    (float)rnd.NextDouble(Camera.UL_CORNER.X, Camera.BR_CORNER.X),
                    (float)rnd.NextDouble(Camera.UL_CORNER.Y, Camera.BR_CORNER.Y));

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
