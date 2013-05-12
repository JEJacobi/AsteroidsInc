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
        static float[] sinLook = new float[360];
        static float[] cosLook = new float[360];

        static Util()
        {
            for (int i = 0; i < sinLook.Length; i++) //fill the sin lookup table
                sinLook[i] = (float)Math.Sin(MathHelper.ToRadians(i));
            for (int i = 0; i < cosLook.Length; i++) //and the cos one
                cosLook[i] = (float)Math.Cos(MathHelper.ToRadians(i));
        }

        public static float FastSin(float radian) //sin lookup
        {
            int index = (int)Math.Round(MathHelper.ToDegrees(radian), 0); //get the rounded degree index

            index = (int)VerifyAngle(index); //verify the angle

            return sinLook[index];
        }

        public static float FastCos(float radian) //cos lookup
        {
            int index = (int)Math.Round(MathHelper.ToDegrees(radian), 0); //get the rounded degree index

            index = (int)VerifyAngle(index);

            return cosLook[index];
        }

        public static double NextDouble(this Random rnd, double min, double max) //extension for Random
        {
            return rnd.NextDouble() * (max - min) + min;
        }

        public static Vector2 Center(this Vector2 vector, Vector2 otherVector)
        {
            Vector2 temp = vector + otherVector;
            return temp / 2;
        }

        public static Vector2 RotationToVector(this float rotationRadians) //get a normalized vector from a rotation in radians
        {
            return new Vector2(
                FastSin(rotationRadians), //sin/cos weirdness is due to me preferring a more
                -FastCos(rotationRadians)); //normal direction system
        }

        public static int GetMeanRadius(this Texture2D texture, int columns = 1, int rows = 1) //get the mean radius using the texture's height and width
        {
            int width = texture.Width / columns; //get width/height
            int height = texture.Height / rows;

            float temp = (((width / 2) + (height / 2)) / 2); //Get the mean and return
            return (int)Math.Round(temp, 0); //SINGLE FRAME SPRITES ONLY
        }

        public static List<Texture2D> ToTextureList(this Texture2D texture) //turn a texture object into a List<Texture2D>
        {
            List<Texture2D> temp = new List<Texture2D>();
            temp.Add(texture);
            return temp;
        }

        public static float RotateTo(this Vector2 vector) //get the rotation to a point
        {
            return (float)Math.Atan2(vector.X, -vector.Y); //gotta love simple trig
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

        public static float VerifyAngle(float input) //verify that the provided angle is non-negative 0-359 degrees
        {
            if (input < 0)
                input += 360;
            if (input > 359)
                input %= 360;

            return input;
        }

        public static void VerifyAngle(ref float input) //overload of previous with ref param instead
        {
            input = VerifyAngle(input);
        }
    }
}
