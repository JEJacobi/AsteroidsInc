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
    public static class Util
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

        public static float RotateTo(this Vector2 vector)
        {
            return (float)Math.Atan2(vector.Y, vector.X);
        }

        public static T RandomEnumValue<T>()
        {
            return Enum
                .GetValues(typeof(T))
                .Cast<T>()
                .OrderBy(x => _Random.Next())
                .FirstOrDefault();
        }//Also ripped from a StackOverflow question
        static readonly Random _Random = new Random();

        //
        //Ripped from a StackOverflow question//
        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }

        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }
        //-----------------------------------//
        //
    }
}
