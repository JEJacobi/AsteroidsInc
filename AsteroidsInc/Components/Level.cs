using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteroidsInc.Components
{
    public struct Level
    {
        public int WorldSizeX, WorldSizeY;
        public int Stars;
        public int CollectableOre;
        public int Asteroids;
        public int Mines;
        public int Drones;
        public int Fighters;
        public int Bombers;
        public int Music;

        public int TotalNPCs { get { return Mines + Drones + Fighters + Bombers; } }

        public Level(
            int worldX,
            int worldY,
            int stars,
            int quota,
            int asteroids,
            int mines,
            int drones,
            int fighters,
            int bombers,
            int music)
        {
            WorldSizeX = worldX;
            WorldSizeY = worldY;
            Stars = stars;
            CollectableOre = quota;
            Asteroids = asteroids;
            Mines = mines;
            Drones = drones;
            Fighters = fighters;
            Bombers = bombers;
            Music = music;
        }
    }
}
