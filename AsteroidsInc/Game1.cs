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

        //TEMP
        UIString<int> fpsDisplay;
        UIString<string> cameraData;
        AsteroidManager temp;
        UIString<string> title;
        //TEMP

        #endregion

        #region Properties & Constants
	    #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.IsMouseVisible = true; //mouse should be visible
            this.IsFixedTimeStep = false; //variable timestep
        }

        protected override void Initialize()
        {
            Camera.ScreenSize.X = GraphicsDevice.Viewport.Bounds.Width; //init the camera
            Camera.ScreenSize.Y = GraphicsDevice.Viewport.Bounds.Height;
            Camera.WorldRectangle = new Rectangle(0, 0, (int)Camera.ScreenSize.X, (int)Camera.ScreenSize.Y); //create the world
            Camera.CenterPosition = new Vector2(Camera.WorldRectangle.Width / 2, Camera.WorldRectangle.Height / 2);

            Logger.WriteLog("\nInitializing components..."); //log it
            base.Initialize();
        }

        protected override void LoadContent()
        {
            //START CONTENT LOAD

            //FONTS
            ContentHandler.Fonts.Add("lcd", Content.Load<SpriteFont>("Fonts/lcd"));
            ContentHandler.Fonts.Add("title", Content.Load<SpriteFont>("Fonts/title"));

            //TEXTURES
            ContentHandler.Textures.Add(AsteroidManager.SMALL_ASTEROID, Content.Load<Texture2D>("Textures/Asteroid1"));
            ContentHandler.Textures.Add(AsteroidManager.ORE_ASTEROID, Content.Load<Texture2D>("Textures/Asteroid2"));
            ContentHandler.Textures.Add(AsteroidManager.LARGE_ASTEROID, Content.Load<Texture2D>("Textures/Asteroid3"));
            ContentHandler.Textures.Add("junk1", Content.Load<Texture2D>("Textures/SmallJunk01"));
            ContentHandler.Textures.Add("junk2", Content.Load<Texture2D>("Textures/SmallJunk02"));
            ContentHandler.Textures.Add("junk3", Content.Load<Texture2D>("Textures/SmallJunk03"));
            ContentHandler.Textures.Add(Player.SHIP_TEXTURE, Content.Load<Texture2D>("Textures/Ship"));

            ContentHandler.Textures.Add("particle", //Generated texture
                Util.GetColorTexture(GraphicsDevice, Color.White, 2, 2));

            //SOUNDEFFECTS

            //MUSIC

            //END CONTENT LOAD
            Logger.WriteLog("Content loaded successfully..."); //log content load


            List<Texture2D> particle = new List<Texture2D>(); //particle list
            particle.Add(ContentHandler.Textures["junk1"]);
            particle.Add(ContentHandler.Textures["junk2"]);
            particle.Add(ContentHandler.Textures["junk3"]);

            List<Texture2D> asteroid = new List<Texture2D>(); //asteroid list
            asteroid.Add(ContentHandler.Textures[AsteroidManager.SMALL_ASTEROID]);
            asteroid.Add(ContentHandler.Textures[AsteroidManager.LARGE_ASTEROID]);
            asteroid.Add(ContentHandler.Textures[AsteroidManager.ORE_ASTEROID]);


            //COMPONENT INITIALIZATION

            Player.Initialize();
            fpsDisplay = new UIString<int>(60, Vector2.Zero, ContentHandler.Fonts["lcd"], Color.White, true, 1f, 0f, false); //TEMP
            title = new UIString<string>("Asteroids Inc.", new Vector2(0.5f, 0.2f), ContentHandler.Fonts["title"], Color.White);
            cameraData = new UIString<string>("", new Vector2(0f, 0.9f), ContentHandler.Fonts["lcd"], Color.White, true, 1f, 0f, false);
            temp = new AsteroidManager(20, 50, 100, 1, 2, asteroid, particle, true);

            spriteBatch = new SpriteBatch(GraphicsDevice); //initialize the spriteBatch

            //END COMPONENT INIT
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
            if(InputHandler.IsNewKeyPress(Keys.Space)) //delete a random asteroid
            {
                Random rnd = new Random();
                temp.DestroyAsteroid(temp.Asteroids[rnd.Next(temp.Asteroids.Count)]);
            }
            if (InputHandler.IsKeyDown(Keys.Escape))
                this.Exit(); //exit on escape

            fpsDisplay.Value = (int)Math.Round(1 / gameTime.ElapsedGameTime.TotalSeconds, 0);
            //calculate framerate to the nearest int
            cameraData.Value = " " + Camera.CenterPosition.ToString() + " - " + Camera.ScreenSize.ToString();
            //get camera pos & size

            temp.Update(gameTime);
            Player.Update(gameTime);
            title.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(); //BEGIN SPRITE DRAW

            fpsDisplay.Draw(spriteBatch);
            temp.Draw(spriteBatch);
            Player.Draw(spriteBatch);
            title.Draw(spriteBatch);
            cameraData.Draw(spriteBatch);

            spriteBatch.End(); //END SPRITE DRAW
            base.Draw(gameTime);
        }
    }
}
