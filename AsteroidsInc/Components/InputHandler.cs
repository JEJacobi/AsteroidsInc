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

        #region Methods

        public static void Update() //get new states and move current State to last State
        {
            LastKeyboardState = CurrentKeyboardState;
            CurrentKeyboardState = Keyboard.GetState();
        }

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
    }
}
