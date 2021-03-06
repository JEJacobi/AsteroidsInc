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
    public static class StarField
    {
        public static List<GameObject> Stars { get; set; }
        public static List<Texture2D> Textures = new List<Texture2D>();
        public static bool Scrolling = false;

        static readonly Color starColor = Color.White;

        const float maxAlphaScalar = 0.8f;
        const int starAmountRandomization = 25;
        const float starDepth = 0.1f;

        static readonly Vector2 STAR_SCROLL = new Vector2(0, 40);

        public static void Generate(int starsToGenerate)
        {
            Stars = new List<GameObject>(); //wipe the list

            starsToGenerate += Util.rnd.Next(-starAmountRandomization, starAmountRandomization); //add a bit of randomization

            for (int i = 0; i < starsToGenerate; i++)
            {
                Texture2D tex = Util.PickRandom<Texture2D>(Textures); //get a random texture
                Color color = starColor;

                //COLOR GENERATION
                float aMix = (float)Util.rnd.NextDouble(0.0, maxAlphaScalar); //get a random transparancy scalar
                color = Color.Lerp(Color.Transparent, color, aMix); //set the transparancy

                //POSITION GENERATION
                Vector2 pos = new Vector2(
                    Util.rnd.Next(Camera.WorldRectangle.X, Camera.WorldRectangle.Width),
                    Util.rnd.Next(Camera.WorldRectangle.Y, Camera.WorldRectangle.Height));

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
            {
                if (Scrolling && Stars[i].Velocity != STAR_SCROLL)
                {
                    Stars[i].LiteMode = false;
                    Stars[i].Velocity = STAR_SCROLL;
                }
                else if (Scrolling == false && Stars[i].Velocity != Vector2.Zero)
                {
                    Stars[i].LiteMode = true;
                    Stars[i].Velocity = Vector2.Zero;
                }

                Stars[i].Update(gameTime);
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < Stars.Count; i++)
                Stars[i].Draw(spriteBatch);
        }
    }
}
