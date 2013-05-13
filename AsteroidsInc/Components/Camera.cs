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
                return new Vector2(Position.X + (Viewport.Width >> 1),
                    Position.Y + (Viewport.Height >> 1));
            }
            set
            {
                float x = Position.X; float y = Position.Y;
                x = value.X - (Viewport.Width >> 1);
                y = value.Y - (Viewport.Height >> 1);
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
                return new Vector2(WorldRectangle.Width >> 1, WorldRectangle.Height >> 1);
            }
        }
        public static Vector2 UL_CORNER //upper-left corner
        {
            get
            {
                return new Vector2(WorldRectangle.X, WorldRectangle.Y);
            }
        }
        public static Vector2 BR_CORNER //bottom-right corner
        {
            get
            {
                return new Vector2(WorldRectangle.Width, WorldRectangle.Height);
            }
        }

        #endregion

        #region Methods

        public static void Initialize(int windowWidth, int windowHeight, int worldWidth, int worldHeight)
        {
            Camera.ScreenSize.X = windowWidth; //init the camera
            Camera.ScreenSize.Y = windowHeight;
            Camera.WorldRectangle = new Rectangle(0, 0, worldWidth, worldHeight); //create the world
            Camera.CenterPosition = new Vector2(Camera.WorldRectangle.Width >> 1, Camera.WorldRectangle.Height >> 1);
        }

        public static void Move(Vector2 offset) //Offset camera method
        {
            Position += offset;
        }

        public static bool IsObjectVisible(Rectangle obj)
        { return Viewport.Intersects(obj); } //Check if obj intersects with the viewport

        public static bool IsObjectVisible(Vector2 obj) //Overload with Vector2
        {
            //create a 2x2 rectangle at Vector2's position
            //2x2 is to account for int cast truncation
            Rectangle temp = new Rectangle(
                (int)obj.X - 1,
                (int)obj.Y - 1,
                2, 2);

            return IsObjectVisible(temp); //and check
        }

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
