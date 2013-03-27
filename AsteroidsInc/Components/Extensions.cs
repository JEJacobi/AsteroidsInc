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
    public static class Extensions
    {
        public static double NextDouble(this Random rnd, double min, double max) //extension for Random
        {
            return rnd.NextDouble() * (max - min) + min;
        }

        public static Vector2 RotationToVectorDouble(this double rotationRadians) //get [1,1] vector from a rotation in radians
        {
            return new Vector2(
                (float)Math.Cos(rotationRadians),
                (float)Math.Sin(rotationRadians));
        }

        public static Vector2 RotationToVectorFloat(this float rotationRadians) //get [1,1] vector from a rotation in radians
        {
            return RotationToVectorDouble(rotationRadians);
        }
    }
}