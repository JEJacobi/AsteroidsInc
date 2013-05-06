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
        GameState gameState;

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
        const string MUSIC_DIR = "Music/";

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
            gameState = GameState.Game;

            Camera.ScreenSize.X = GraphicsDevice.Viewport.Bounds.Width; //init the camera
            Camera.ScreenSize.Y = GraphicsDevice.Viewport.Bounds.Height;
            Camera.WorldRectangle = new Rectangle(0, 0, (int)Camera.ScreenSize.X * 2, (int)Camera.ScreenSize.Y * 2); //create the world
            Camera.CenterPosition = new Vector2(Camera.WorldRectangle.Width / 2, Camera.WorldRectangle.Height / 2);
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
            ContentHandler.Textures.Add(Player.SHIP_TEXTURE, Content.Load<Texture2D>(TEXTURE_DIR + Player.SHIP_TEXTURE));
            ContentHandler.Textures.Add(Player.MISSILE_KEY, Content.Load<Texture2D>(TEXTURE_DIR + Player.MISSILE_KEY));
            ContentHandler.Textures.Add(Player.LASER_KEY, Content.Load<Texture2D>(TEXTURE_DIR + Player.LASER_KEY));

            ContentHandler.Textures.Add("particle", //General generated particle texture
                Util.GetColorTexture(GraphicsDevice, Color.White, 2, 2));

            //SOUNDEFFECTS

            //Static SFX:
            ContentHandler.SFX.Add(Player.COLLISION_SFX, Content.Load<SoundEffect>(SOUND_DIR + Player.COLLISION_SFX));
            ContentHandler.SFX.Add(Player.MISSILE_KEY, Content.Load<SoundEffect>(SOUND_DIR + Player.MISSILE_KEY));
            ContentHandler.SFX.Add(Player.LASER_KEY, Content.Load<SoundEffect>(SOUND_DIR + Player.LASER_KEY));
            ContentHandler.SFX.Add("switch", Content.Load<SoundEffect>(SOUND_DIR + "switch"));

            //Instances:
            ContentHandler.InstanceSFX.Add(Player.ENGINE_SFX, Content.Load<SoundEffect>(SOUND_DIR + Player.ENGINE_SFX).CreateInstance());
            ContentHandler.InstanceSFX[Player.ENGINE_SFX].IsLooped = true; //init and add engine sound

            ContentHandler.InstanceSFX.Add(Player.DEATH_SFX, Content.Load<SoundEffect>(SOUND_DIR + Player.DEATH_SFX).CreateInstance());

            //MUSIC
            ContentHandler.Songs.Add("menu", Content.Load<Song>(MUSIC_DIR + "menu"));

            //END CONTENT LOAD

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
            Player.DeadEvent += new EventHandler(Player_DeadEvent);

            fpsDisplay = new UIString<int>(60, Vector2.Zero, ContentHandler.Fonts["lcd"], Color.White, true, 1f, 0f, false); //TEMP
            title = new UIString<string>("Asteroids Inc.", new Vector2(0.5f, 0.2f), ContentHandler.Fonts["title"], Color.White, true);
            health = new UIString<string>("Health: 100", new Vector2(0f, 0.9f), ContentHandler.Fonts["lcd"], Color.White, true, 1f, 0f, false);
            temp = new AsteroidManager(70, 50, 100, 1, 2, asteroid, particle, true);

            spriteBatch = new SpriteBatch(GraphicsDevice); //initialize the spriteBatch

            //END COMPONENT INIT
        }

        protected override void UnloadContent()
        {
            //Nothing yet...
        }

        protected override void Update(GameTime gameTime)
        {
            InputHandler.Update(); //update InputHandler regardless of gamestate

            if (InputHandler.IsKeyDown(Keys.Escape))
                this.Exit(); //exit on escape, regardless of gamestate

            switch (gameState) //MAIN GAMESTATE SWITCH
            {
                case GameState.Game:
                    //GAME UPDATE BEGIN

                    ProjectileManager.Update(gameTime);
                    temp.Update(gameTime);
                    Player.Update(gameTime);
                    title.Update(gameTime);

                    fpsDisplay.Value = (int)Math.Round(1 / gameTime.ElapsedGameTime.TotalSeconds, 0);
                    //calculate framerate to the nearest int
                    health.Value = "Health: " + Player.Health.ToString();
                    //get health

                    base.Update(gameTime);

                    //GAME UPDATE END
                    break;

                case GameState.Dead:
                    //TEMP
                    break;

                default:
                    throw new ArgumentException();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            switch (gameState) //MAIN GAMESTATE DRAW SWITCH
            {
                case GameState.Game:
                    //BEGIN GAME DRAW

                    GraphicsDevice.Clear(Color.Black);
                    spriteBatch.Begin(); //BEGIN SPRITE DRAW

                    fpsDisplay.Draw(spriteBatch);
                    health.Draw(spriteBatch); //UI elements first

                    Player.Draw(spriteBatch); //Player next

                    ProjectileManager.Draw(spriteBatch);
                    temp.Draw(spriteBatch); //And then the rest of the game components

                    spriteBatch.End(); //END SPRITE DRAW
                    base.Draw(gameTime);

                    //END GAME DRAW
                    break;
                case GameState.Dead:

                    GraphicsDevice.Clear(Color.Black);

                    spriteBatch.Begin();

                    title.Draw(spriteBatch);

                    spriteBatch.End();

                    break;
                default:
                    throw new ArgumentException();
            }
        }

        #region Local Methods

        private void SwitchGameState(GameState state)
        {
            gameState = state; //switch the state
            ContentHandler.StopAll(); //and stop the sound
        }

        #endregion

        #region Event Handlers

        void Player_DeadEvent(object sender, EventArgs e)
        {
            SwitchGameState(GameState.Dead);
            ContentHandler.PlaySong("menu");
        }

        #endregion
    }

    public enum GameState
    {
        Game,
        Dead
    }
}
