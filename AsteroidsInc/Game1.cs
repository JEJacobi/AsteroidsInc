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

        Dictionary<string, Texture2D> Textures { get; set; } //Dictionary for textures
        Dictionary<string, SpriteFont> Fonts { get; set; } //Dictonary for SpriteFonts
        Dictionary<string, SoundEffect> Effects { get; set; } //Dictionary for SFX
        Dictionary<string, Song> Songs { get; set; } //Dictionary for songs, played via the MediaPlayer

        UIString<int> fpsDisplay;
        AsteroidManager temp;

        #endregion

        #region Properties & Constants
	    #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.IsMouseVisible = true;
            this.IsFixedTimeStep = false;
        }

        protected override void Initialize()
        {
            Textures = new Dictionary<string, Texture2D>();
            Fonts = new Dictionary<string, SpriteFont>();
            Effects = new Dictionary<string, SoundEffect>();
            Songs = new Dictionary<string, Song>();

            Camera.ScreenSize.X = GraphicsDevice.Viewport.Bounds.Width;
            Camera.ScreenSize.Y = GraphicsDevice.Viewport.Bounds.Height;
            Camera.WorldRectangle = new Rectangle(0, 0, (int)Camera.ScreenSize.X * 2, (int)Camera.ScreenSize.Y * 2); //TEMP
            Camera.ViewportSize = Camera.ScreenSize; //TEMP
            Logger.WriteLog("Initializing components...");
            base.Initialize();
        }

        protected override void LoadContent()
        {
            try
            {
                //START CONTENT LOAD

                //FONTS
                Fonts.Add("lcd", Content.Load<SpriteFont>("lcd"));

                //TEXTURES
                Textures.Add("ball", Content.Load<Texture2D>("ballsprite"));
                Textures.Add("particle", ColorTextureGenerator.GetColorTexture(GraphicsDevice, Color.White, 2, 2));

                //END CONTENT LOAD
                Logger.WriteLog("Content loaded successfully...");
            }
            catch (ContentLoadException e)
            {
                Logger.WriteLog("Content Load Error: " + e.Message);
            }

            List<Texture2D> particle = new List<Texture2D>();
            particle.Add(Textures["particle"]);

            List<Texture2D> asteroid = new List<Texture2D>();
            asteroid.Add(Textures["ball"]);

            fpsDisplay = new UIString<int>(60, Vector2.Zero, Fonts["lcd"], Color.White, true, 1f, 0f, false); //TEMP
            temp = new AsteroidManager(500, 50, 500, 1, 2, asteroid, particle, true);

            spriteBatch = new SpriteBatch(GraphicsDevice); //initialize the spriteBatch
        }

        protected override void UnloadContent()
        {
            Logger.WriteLog("Unloading Content...");
        }

        protected override void Update(GameTime gameTime)
        {
            InputHandler.Update(); //update InputHandler

            if (InputHandler.IsKeyDown(Keys.Left))
                Camera.Position += new Vector2(-3f, 0f);
            if (InputHandler.IsKeyDown(Keys.Right))
                Camera.Position += new Vector2(3f, 0f);
            if (InputHandler.IsKeyDown(Keys.Up))
                Camera.Position += new Vector2(0f, -3f);
            if (InputHandler.IsKeyDown(Keys.Down))
                Camera.Position += new Vector2(0f, 3f);

            fpsDisplay.Value = (int)Math.Round(1 / gameTime.ElapsedGameTime.TotalSeconds, 0);
            //calculate framerate to the nearest int

            temp.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin(); //BEGIN SPRITE DRAW

            fpsDisplay.Draw(spriteBatch);
            temp.Draw(spriteBatch);

            spriteBatch.End(); //END SPRITE DRAW
            base.Draw(gameTime);
        }
    }
}
