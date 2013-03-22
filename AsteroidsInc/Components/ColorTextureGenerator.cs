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
    public static class ColorTextureGenerator
    {
        public static Texture2D Get1by1ColorTexture(GraphicsDevice device, Color color)
        {
            return GetColorTexture(device, color, 1, 1);
        }

        public static Texture2D GetColorTexture(GraphicsDevice device, Color color, int width, int height)
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
