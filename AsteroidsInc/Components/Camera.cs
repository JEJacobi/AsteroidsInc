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

namespace AsteroidsInc
{
    public static class Camera
    {
        #region Declarations

        public static Vector2 Position = Vector2.Zero; //Default of upper left camera position
        public static Vector2 ViewportSize = Vector2.Zero; //Default of 0 viewport size
        public static Rectangle WorldRectangle = new Rectangle(0, 0, 0, 0); //Default of empty rectangle

        public static Rectangle Viewport //returns a rectangle showing the viewport
        {
            get {
                return new Rectangle((int)Position.X, (int)Position.Y, 
                    (int)ViewportSize.X, (int)ViewportSize.Y); //cast floats to ints for rectangle init
            }
        }

        #endregion

        #region Methods

        public static void Move(Vector2 offset) //Offset camera method
        {
            Position += offset;
        }

        public static bool IsObjectVisible(Rectangle obj)
        { return Viewport.Intersects(obj); } //Check if obj intersects with the viewport

        public static Vector2 GetLocalCoords(Vector2 point)
        { return point - Position; } //Transform point to position - used in converting to screen coords

        public static Rectangle GetLocalCoords(Rectangle rectangle) //Overload of previous, takes a rectangle
        {
            return new Rectangle(
                rectangle.Left - (int)Position.X, //get the upper left value
                rectangle.Top - (int)Position.Y, //and upper right value
                rectangle.Width, //return given width
                rectangle.Height); //return given height
        }

        #endregion
    }
}
