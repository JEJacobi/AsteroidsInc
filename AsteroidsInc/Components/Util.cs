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
    public static class Util //Utility class, used for extensions, general functions, etc...
    {
        public static double NextDouble(this Random rnd, double min, double max) //extension for Random
        {
            return rnd.NextDouble() * (max - min) + min;
        }

        public static Vector2 RotationToVector(this float rotationRadians) //get a normalized vector from a rotation in radians
        {
            return new Vector2(
                (float)Math.Sin(rotationRadians), //sin/cos weirdness is due to me preferring a more
                (float)-Math.Cos(rotationRadians)); //normal direction system
        }

        public static int GetMeanRadius(this Texture2D texture) //get the mean radius using the texture's height and width
        {
            float temp = (((texture.Width / 2) + (texture.Height / 2)) / 2); //Get the mean and return
            return (int)Math.Round(temp, 0);
        }

        public static List<Texture2D> ToTextureList(this Texture2D texture) //turn a texture object into a List<Texture2D>
        {
            List<Texture2D> temp = new List<Texture2D>();
            temp.Add(texture);
            return temp;
        }

        public static float RotateTo(this Vector2 vector) //get the rotation to a point
        {
            return (float)Math.Atan2(vector.Y, vector.X); //gotta love simple trig
        }

        //Ripped from a StackOverflow question//
        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }
        //
        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }
        //
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }
        //
        public static T RandomEnumValue<T>()
        {
            return Enum
                .GetValues(typeof(T))
                .Cast<T>()
                .OrderBy(x => _Random.Next())
                .FirstOrDefault();
        }//Also ripped from a different StackOverflow question
        static readonly Random _Random = new Random();
        //-----------------------------------//

        public static Texture2D Get1by1ColorTexture(GraphicsDevice device, Color color) //gets a 1x1 color texture
        {
            return GetColorTexture(device, color, 1, 1);
        }

        public static Texture2D GetColorTexture(GraphicsDevice device, Color color, int width, int height) //gets a specified size color texture
        {
            Texture2D tempTexture = new Texture2D( //init a new empty texture
                device,
                width,
                height,
                true,
                SurfaceFormat.Color);

            Color[] colors = new Color[width * height]; //array of colors

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = new Color(color.ToVector3()); //assign each color
            }

            tempTexture.SetData(colors); //set colors
            return tempTexture;
        }
    }
}
