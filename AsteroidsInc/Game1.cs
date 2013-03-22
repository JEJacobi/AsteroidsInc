using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using AsteroidsInc.Components;

namespace AsteroidsInc
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region Declarations

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Dictionary<string, Texture2D> Textures; //Dictionary for textures
        Dictionary<string, SpriteFont> Fonts; //Dictonary for SpriteFonts
        
        #endregion

        #region Properties & Constants
	    #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            Textures = new Dictionary<string, Texture2D>();
            Fonts = new Dictionary<string, SpriteFont>();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Fonts.Add("FPS", Content.Load<SpriteFont>("FPS"));

            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void UnloadContent()
        {
            //Nothing yet
        }

        protected override void Update(GameTime gameTime)
        {
            InputHandler.Update(); //update InputHandler

            // = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds; //calculate framerate
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
        }
    }
}
