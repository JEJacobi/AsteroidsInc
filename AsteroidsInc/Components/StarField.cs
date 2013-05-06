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
    public static class StarField
    {
        public static List<GameObject> Stars { get; set; }

        static Random rnd = new Random();
        static readonly Color starColor = Color.White;

        const float maxAlphaScalar = 0.8f;
        const int minStars = 200;
        const int maxStars = 6000;
        const float starDepth = 0.1f;

        public static void Generate(List<Texture2D> Textures)
        {
            Stars = new List<GameObject>(); //wipe the list

            for (int i = 0; i < rnd.Next(minStars, maxStars); i++)
            {
                Texture2D tex = Util.PickRandom<Texture2D>(Textures); //get a random texture
                Color color = starColor;

                //COLOR GENERATION
                float aMix = (float)rnd.NextDouble(0.0, maxAlphaScalar); //get a random transparancy scalar
                color = Color.Lerp(Color.Transparent, color, aMix); //set the transparancy

                //POSITION GENERATION
                Vector2 pos = new Vector2(
                    rnd.Next(Camera.WorldRectangle.X, Camera.WorldRectangle.Width),
                    rnd.Next(Camera.WorldRectangle.Y, Camera.WorldRectangle.Height));

                Stars.Add(new GameObject( //and add the object with generated values
                    tex,
                    pos,
                    Vector2.Zero,
                    color));
            }
        }

        public static void Update(GameTime gameTime)
        {
            for (int i = 0; i < Stars.Count; i++)
                Stars[i].Update(gameTime);
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < Stars.Count; i++)
                Stars[i].Draw(spriteBatch);
        }
    }
}
