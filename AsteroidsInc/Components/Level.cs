﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsteroidsInc.Components
{
    public class Level
    {
        public int WorldSizeX, WorldSizeY;
        public int Stars;
        public int Quota;
        public int Asteroids;
        public int Mines;
        public int Drones;
        public int Fighters;
        public int Bombers;

        public Level(
            int worldX,
            int worldY,
            int stars,
            int quota,
            int asteroids,
            int mines,
            int drones,
            int fighters,
            int bombers)
        {
            WorldSizeX = worldX;
            WorldSizeY = worldY;
            Stars = stars;
            Quota = quota;
            Asteroids = asteroids;
            Mines = mines;
            Drones = drones;
            Fighters = fighters;
            Bombers = bombers;
        }
    }
}
