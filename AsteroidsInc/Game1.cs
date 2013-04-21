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
        UIString<string> health;
        AsteroidManager temp;
        UIString<string> title;
        //TEMP

        #endregion

        #region Properties & Constants

        const string TEXTURE_DIR = "Textures/";
        const string FONT_DIR = "Fonts/";
        const string SOUND_DIR = "Sound/";

	    #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            //graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";
            this.IsMouseVisible = true; //mouse should be visible
            this.IsFixedTimeStep = false; //variable timestep
        }

        protected override void Initialize()
        {
            Camera.ScreenSize.X = GraphicsDevice.Viewport.Bounds.Width; //init the camera
            Camera.ScreenSize.Y = GraphicsDevice.Viewport.Bounds.Height;
            Camera.WorldRectangle = new Rectangle(0, 0, (int)Camera.ScreenSize.X * 2, (int)Camera.ScreenSize.Y * 2); //create the world
            Camera.CenterPosition = new Vector2(Camera.WorldRectangle.Width / 2, Camera.WorldRectangle.Height / 2);

            Logger.WriteLog("\nInitializing components..."); //log it
            base.Initialize();
        }

        protected override void LoadContent()
        {
            //START CONTENT LOAD

            //FONTS
            ContentHandler.Fonts.Add("lcd", Content.Load<SpriteFont>(FONT_DIR + "lcd"));
            ContentHandler.Fonts.Add("title", Content.Load<SpriteFont>(FONT_DIR + "/title"));

            //TEXTURES
            ContentHandler.Textures.Add(AsteroidManager.SMALL_ASTEROID, Content.Load<Texture2D>(TEXTURE_DIR + "Asteroid1"));
            ContentHandler.Textures.Add(AsteroidManager.ORE_ASTEROID, Content.Load<Texture2D>(TEXTURE_DIR + "Asteroid2"));
            ContentHandler.Textures.Add(AsteroidManager.LARGE_ASTEROID, Content.Load<Texture2D>(TEXTURE_DIR + "Asteroid3"));
            ContentHandler.Textures.Add("junk1", Content.Load<Texture2D>(TEXTURE_DIR + "SmallJunk01"));
            ContentHandler.Textures.Add("junk2", Content.Load<Texture2D>(TEXTURE_DIR + "SmallJunk02"));
            ContentHandler.Textures.Add("junk3", Content.Load<Texture2D>(TEXTURE_DIR + "SmallJunk03"));
            ContentHandler.Textures.Add(Player.SHIP_TEXTURE, Content.Load<Texture2D>(TEXTURE_DIR + "Ship"));

            ContentHandler.Textures.Add("particle", //General generated particle texture
                Util.GetColorTexture(GraphicsDevice, Color.White, 2, 2));

            //SOUNDEFFECTS

            //Static SFX:
            ContentHandler.SFX.Add(Player.COLLISION_SFX, Content.Load<SoundEffect>(SOUND_DIR + Player.COLLISION_SFX));

            //Instances:
            ContentHandler.InstanceSFX.Add(Player.ENGINE_SFX, Content.Load<SoundEffect>(SOUND_DIR + Player.ENGINE_SFX).CreateInstance());
            ContentHandler.InstanceSFX[Player.ENGINE_SFX].IsLooped = true; //init and add engine sound

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
            health = new UIString<string>("Health: 100", new Vector2(0f, 0.9f), ContentHandler.Fonts["lcd"], Color.White, true, 1f, 0f, false);
            temp = new AsteroidManager(70, 50, 100, 1, 2, asteroid, particle, true);

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

            if(InputHandler.IsNewKeyPress(Keys.Space)) //delete a random asteroid
            {
                Random rnd = new Random();
                temp.DestroyAsteroid(temp.Asteroids[rnd.Next(temp.Asteroids.Count)]);
            }

            if (InputHandler.IsNewKeyPress(Keys.S))
                ContentHandler.TogglePlaySFX(); //toggle play SFX

            if (InputHandler.IsNewKeyPress(Keys.M))
                ContentHandler.TogglePlayMusic(); //toggle play music

            if (InputHandler.IsKeyDown(Keys.Escape))
                this.Exit(); //exit on escape

            fpsDisplay.Value = (int)Math.Round(1 / gameTime.ElapsedGameTime.TotalSeconds, 0);
            //calculate framerate to the nearest int
            health.Value = "Health: " + Player.Health.ToString();
            //get health

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
            health.Draw(spriteBatch);

            spriteBatch.End(); //END SPRITE DRAW
            base.Draw(gameTime);
        }
    }
}
