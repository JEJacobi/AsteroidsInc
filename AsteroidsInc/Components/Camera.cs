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
        #region Declarations / Properties

        public static Vector2 Position //actual camera position
        {
            get { return position; }
            set
            {
                position.X = MathHelper.Clamp(value.X, WorldRectangle.X, WorldRectangle.Width - Viewport.Width);
                position.Y = MathHelper.Clamp(value.Y, WorldRectangle.Y, WorldRectangle.Height - Viewport.Height);
            }
        }
        public static Vector2 CenterPosition
        {
            get
            {
                return new Vector2(Position.X + (Viewport.Width / 2),
                    Position.Y + (Viewport.Height / 2));
            }
            set
            {
                float x = Position.X; float y = Position.Y;
                x = value.X - (Viewport.Width / 2);
                y = value.Y - (Viewport.Height / 2);
                Position = new Vector2(x, y);
            }
        }//camera position, but with a center origin
        static Vector2 position = Vector2.Zero; //position variable

        public static Rectangle WorldRectangle = Rectangle.Empty; //Default empty rectangle
        public static Vector2 ScreenSize = Vector2.Zero; //Size of the game screen, must be set at Init
        public static Rectangle Viewport //returns a rectangle showing the viewport
        {
            get 
            {
                return new Rectangle((int)Position.X, (int)Position.Y, 
                    (int)ScreenSize.X, (int)ScreenSize.Y); //cast floats to ints for rectangle init
            }
        }
        public const bool LOOPWORLD = true; //should objects going out of the world loop back in?

        public static Vector2 CENTER_OF_WORLD
        {
            get
            {
                return new Vector2(WorldRectangle.Width / 2, WorldRectangle.Height / 2);
            }
        }
        public static Vector2 UL_CORNER
        {
            get
            {
                return new Vector2(WorldRectangle.X, WorldRectangle.Y);
            }
        }
        public static Vector2 BR_CORNER
        {
            get
            {
                return new Vector2(WorldRectangle.Width, WorldRectangle.Height);
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

        public static bool IsObjectInWorld(Rectangle obj)
        { return WorldRectangle.Intersects(obj); } //Tests if the object is in the world rectangle
        #endregion
    }
}
