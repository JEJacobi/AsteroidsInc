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
    public static class InputHandler //static input handler class, must be updated each tick
    {
        public static KeyboardState CurrentKeyboardState = new KeyboardState(); //what is happening on the keyboard
        public static KeyboardState LastKeyboardState = new KeyboardState(); //what happened last tick
        public static MouseState MouseState = new MouseState(); //what is happening for the mouse
        public static MouseState LastMouseState = new MouseState(); //and same, but last tick

        public static void Update() //get new states and move current State to last State
        {
            LastKeyboardState = CurrentKeyboardState; //move & get new keyboard states
            CurrentKeyboardState = Keyboard.GetState();

            LastMouseState = MouseState; //move & get new mouse states
            MouseState = Mouse.GetState();

            if (InputHandler.IsNewKeyPress(Keys.S))
                ContentHandler.TogglePlaySFX(); //toggle play SFX

            if (InputHandler.IsNewKeyPress(Keys.M))
                ContentHandler.TogglePlayMusic(); //toggle play music
        }

        #region Keyboard Methods

        public static bool IsNewKeyPress(Keys key) //used for menu movement, etc
        {
            if (CurrentKeyboardState.IsKeyDown(key) && LastKeyboardState.IsKeyUp(key))
                return true; //If new state is pressed and last state is unpressed, return true
            else
                return false; //else return false
        }

        public static bool WasKeyDown(Keys key) //was the key down last tick, but up now?
        {
            if (CurrentKeyboardState.IsKeyUp(key) && LastKeyboardState.IsKeyDown(key))
                return true;
            else
                return false;
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

        public static bool IsClickingObject(Rectangle obj) //overload of previous which defaults to LMB
        {
            return IsClickingObject(obj, MouseButtons.LeftMouseButton);
        }

        public static bool HasLeftObject(Rectangle obj)
        {
            if (GetLastMousePos().Intersects(obj) &&
                IsMouseOverObject(obj) == false) //if mouse was over object, but now isn't, return true
                return true;
            else
                return false;
        } //checks if the mouse has left a rectangle object

        public static Rectangle GetMousePos() //returns a 1 by 1 rectangle of the mouse location
        {
            return new Rectangle(
                MouseState.X,
                MouseState.Y,
                1, 1);
        }

        public static Rectangle GetLastMousePos()
        {
            return new Rectangle(
                LastMouseState.X,
                LastMouseState.Y,
                1, 1);
        } //returns a 1x1 rectangle of the last mouse pos

        #endregion
    }

    public class MouseClickArgs : EventArgs //simple eventargs to use in input-related events
    {
        public MouseButtons Button { get; set; }

        public MouseClickArgs()
        {
            Button = MouseButtons.None;
        }

        public MouseClickArgs(MouseButtons button)
        {
            Button = button;
        }
    }

    public enum MouseButtons
    {
        None,
        LeftMouseButton,
        MiddleMouseButton,
        RightMouseButton
    }
}
