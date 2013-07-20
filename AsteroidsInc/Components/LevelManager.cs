using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace AsteroidsInc.Components
{
    public static class LevelManager
    {
        public static Level[] Levels;
        public static int Counter = 0;
        public const string LEVEL_DATA = "LEVELS.DAT";
        public const int TOTAL_LEVELS = 10;
        public static event EventHandler GameComplete;

        public static Level CurrentLevel
        {
            get { return Levels[Counter]; }
        }

        static LevelManager() //parse level data from file
        {
            Levels = new Level[TOTAL_LEVELS];

            FileStream input = new FileStream(
                LEVEL_DATA,
                FileMode.Open,
                FileAccess.Read);

            StreamReader reader = new StreamReader(input);

            for (int i = 0; i < TOTAL_LEVELS; i++)
            {
                string line = reader.ReadLine();
                if (line == null)
                    break;

                string[] values = line.Split(','); //split into component values

                Levels[i] = new Level(
                    int.Parse(values[0]),
                    int.Parse(values[1]),
                    int.Parse(values[2]),
                    int.Parse(values[3]),
                    int.Parse(values[4]),
                    int.Parse(values[5]),
                    int.Parse(values[6]),
                    int.Parse(values[7]),
                    int.Parse(values[8]),
                    int.Parse(values[9]),
                    values[10]);
            }

            reader.Close();
        }

        public static void NextLevel() //regenerate/reset managers to next level's data
        {
            Counter++;

            if (Counter > TOTAL_LEVELS && GameComplete != null)
                GameComplete(Levels, EventArgs.Empty); //if max levels reached, trigger game over event

            AsteroidManager.Regenerate(CurrentLevel.Asteroids);
            Player.CollectableOre = CurrentLevel.CollectableOre;
            Player.Reset();
            StarField.Generate(CurrentLevel.Stars);
            //TODO: Rest of the things.

            //Ore Manager's stuff
            OreManager.OreDrops = new List<Particle>();

            //Projectile Manager
            ProjectileManager.Projectiles = new List<Projectile>();

            //NPCs
            NPCManager.Generate(CurrentLevel);

            //ContentHandler
            ContentHandler.ClearPlayOnce();
        }

        public static void Restart()
        {
            Counter = -1;
            NextLevel();
            Player.Health = Player.STARTING_HEALTH; //reset player health on death
            Player.Slot1 = Player.SLOT_1_INITIAL;
            Player.Slot2 = Player.SLOT_2_INITIAL; //as well as equipment
        }
    }
}
