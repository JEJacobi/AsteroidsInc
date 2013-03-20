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
    public static class InputHandler
    {
        public static KeyboardState CurrentKeyboardState = new KeyboardState();
        public static KeyboardState LastKeyboardState = new KeyboardState();
        public static MouseState MouseState = new MouseState();

        public static void Update() //get new states and move current State to last State
        {
            LastKeyboardState = CurrentKeyboardState;
            CurrentKeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();
        }

        #region Keyboard Methods

        public static bool IsNewKeyPress(Keys key)
        {
            if (CurrentKeyboardState.IsKeyDown(key) && LastKeyboardState.IsKeyUp(key))
                return true; //If new state is pressed and last state is unpressed, return true
            else
                return false; //else return false
        }

        public static bool IsKeyUp(Keys key) //convenience method
        {
            if (CurrentKeyboardState.IsKeyUp(key))
                return true;
            else
                return false;
        }

        public static bool IsKeyDown(Keys key) //convenience method
        {
            if (CurrentKeyboardState.IsKeyDown(key))
                return true;
            else
                return false;
        }
        
        #endregion

        #region Mouse Methods

        public static bool IsLeftClicking()
        {
            if (MouseState.LeftButton == ButtonState.Pressed)
                return true;
            else
                return false;
        }

        public static bool IsRightClicking()
        {
            if (MouseState.RightButton == ButtonState.Pressed)
                return true;
            else
                return false;
        }

        public static bool IsMiddleClicking()
        {
            if (MouseState.MiddleButton == ButtonState.Pressed)
                return true;
            else
                return false;
        }

        public static bool IsMouseOverObject(Rectangle obj) //checks if the mouse is over the object
        {
            if (GetMousePos().Intersects(obj))
                return true;
            else
                return false;
        }

        public static bool IsClickingObject(Rectangle obj, MouseButtons button) //checks if the mouse is over the object
        {
            switch (button)
            {
                case MouseButtons.LeftMouseButton:
                    if (IsMouseOverObject(obj) && MouseState.LeftButton == ButtonState.Pressed)
                        return true;
                    return false;
                case MouseButtons.MiddleMouseButton:
                    if (IsMouseOverObject(obj) && MouseState.MiddleButton == ButtonState.Pressed)
                        return true;
                    return false;
                case MouseButtons.RightMouseButton:
                    if (IsMouseOverObject(obj) && MouseState.RightButton == ButtonState.Pressed)
                        return true;
                    return false;
                default:
                    throw new InvalidOperationException("Invalid button specified.");
            }
        }

        public static Rectangle GetMousePos() //returns a 1 by 1 rectangle of the mouse location
        {
            return new Rectangle(
                MouseState.X,
                MouseState.Y,
                1, 1);
        }

        public enum MouseButtons
        {
            LeftMouseButton,
            MiddleMouseButton,
            RightMouseButton
        }

        #endregion
    }
}
