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

        UIString<float> fpsDisplay; //TEMP
        GameObject ball; //TEMP

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
            this.IsMouseVisible = true;
            Textures = new Dictionary<string, Texture2D>();
            Fonts = new Dictionary<string, SpriteFont>();

            Camera.ScreenSize.X = GraphicsDevice.Viewport.Bounds.Width;
            Camera.ScreenSize.Y = GraphicsDevice.Viewport.Bounds.Height;
            Camera.WorldRectangle = new Rectangle(0, 0, (int)Camera.ScreenSize.X, (int)Camera.ScreenSize.Y); //TEMP
            Camera.ViewportSize = Camera.ScreenSize; //TEMP
            base.Initialize();
        }

        protected override void LoadContent()
        { 
            //Loading into content dictionaries
            Fonts.Add("fps", Content.Load<SpriteFont>("FPS"));
            Textures.Add("ball", Content.Load<Texture2D>("ballsprite"));

            ball = new GameObject( //TEMP
                Textures["ball"],
                new Vector2(300, 300),
                Color.White);

            fpsDisplay = new UIString<float>(60, Vector2.Zero, Fonts["fps"], Color.White, true, 1f, 0f, false);

            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void UnloadContent()
        {
            //Nothing yet
        }

        protected override void Update(GameTime gameTime)
        {
            InputHandler.Update(); //update InputHandler

            if(InputHandler.IsKeyDown(Keys.Left))
                Camera.Position.X--;
            if(InputHandler.IsKeyDown(Keys.Right))
                Camera.Position.X++;
            if(InputHandler.IsKeyDown(Keys.Up))
                Camera.Position.Y--;
            if(InputHandler.IsKeyDown(Keys.Down))
                Camera.Position.Y++;

            fpsDisplay.Value = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds; //calculate framerate
            ball.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            fpsDisplay.Draw(spriteBatch);
            ball.Draw(spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
