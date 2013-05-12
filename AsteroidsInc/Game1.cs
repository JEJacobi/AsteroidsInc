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

        //UI element dictionaries
        Dictionary<string, UIBase> GameUI;
        Dictionary<string, UIBase> MenuUI;

        AsteroidManager temp; //TODO: Staticify
        #endregion

        #region Properties & Constants

        const string TEXTURE_DIR = "Textures/";
        const string FONT_DIR = "Fonts/";
        const string SOUND_DIR = "Sound/";
        const string MUSIC_DIR = "Music/";

        const int DEFAULT_WINDOWED_WIDTH = 1024;
        const int DEFAULT_WINDOWED_HEIGHT = 768;

        readonly Color HIGHLIGHT_COLOR = Color.Yellow;
        readonly Color NORMAL_COLOR = Color.White;

	    #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = DEFAULT_WINDOWED_HEIGHT;
            graphics.PreferredBackBufferWidth = DEFAULT_WINDOWED_WIDTH; //set the window size to defaults
            Content.RootDirectory = "Content";
            this.IsMouseVisible = true; //mouse should be visible
        }

        protected override void Initialize()
        {
            gameState = GameState.Menu; //set the initial gamestate

            Camera.Initialize( //initialize the camera
                DEFAULT_WINDOWED_WIDTH,
                DEFAULT_WINDOWED_HEIGHT,
                8000, 8000);

            GameUI = new Dictionary<string, UIBase>();
            MenuUI = new Dictionary<string, UIBase>();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice); //initialize the spriteBatch
            spriteBatch.Begin();

            GraphicsDevice.Clear(Color.Black);

            #region Content Loading
            //START CONTENT LOAD

            //FONTS
            ContentHandler.Fonts.Add("lcd", Content.Load<SpriteFont>(FONT_DIR + "lcd"));
            ContentHandler.Fonts.Add("title", Content.Load<SpriteFont>(FONT_DIR + "title"));
            ContentHandler.Fonts.Add("title2", Content.Load<SpriteFont>(FONT_DIR + "title2"));
            ContentHandler.Fonts.Add("menu", Content.Load<SpriteFont>(FONT_DIR + "menu"));

            //TEXTURES
            ContentHandler.Textures.Add(AsteroidManager.SMALL_ASTEROID, Content.Load<Texture2D>(TEXTURE_DIR + "Asteroid1"));
            ContentHandler.Textures.Add(AsteroidManager.ORE_ASTEROID, Content.Load<Texture2D>(TEXTURE_DIR + "Asteroid2"));
            ContentHandler.Textures.Add(AsteroidManager.LARGE_ASTEROID, Content.Load<Texture2D>(TEXTURE_DIR + "Asteroid3"));

            ContentHandler.Textures.Add("junk1", Content.Load<Texture2D>(TEXTURE_DIR + "SmallJunk01"));
            ContentHandler.Textures.Add("junk2", Content.Load<Texture2D>(TEXTURE_DIR + "SmallJunk02"));
            ContentHandler.Textures.Add("junk3", Content.Load<Texture2D>(TEXTURE_DIR + "SmallJunk03"));

            ContentHandler.Textures.Add(Player.SHIP_TEXTURE, Content.Load<Texture2D>(TEXTURE_DIR + Player.SHIP_TEXTURE));
            ContentHandler.Textures.Add(Player.SHIELD_KEY, Content.Load<Texture2D>(TEXTURE_DIR + Player.SHIELD_KEY));
            ContentHandler.Textures.Add(Player.MISSILE_KEY, Content.Load<Texture2D>(TEXTURE_DIR + Player.MISSILE_KEY));
            ContentHandler.Textures.Add(Player.LASER_KEY, Content.Load<Texture2D>(TEXTURE_DIR + Player.LASER_KEY));

            ContentHandler.Textures.Add("star1", Content.Load<Texture2D>(TEXTURE_DIR + "star1"));
            ContentHandler.Textures.Add("star2", Content.Load<Texture2D>(TEXTURE_DIR + "star2"));
            ContentHandler.Textures.Add("star3", Content.Load<Texture2D>(TEXTURE_DIR + "star3"));
            ContentHandler.Textures.Add("star4", Content.Load<Texture2D>(TEXTURE_DIR + "star4"));

            ContentHandler.Textures.Add("particle", //General generated particle texture
                Util.GetColorTexture(GraphicsDevice, Color.White, 2, 2));

            //SOUNDEFFECTS

            //Static SFX:
            ContentHandler.SFX.Add(Player.COLLISION_SFX, Content.Load<SoundEffect>(SOUND_DIR + Player.COLLISION_SFX));
            ContentHandler.SFX.Add(Player.SHIELD_KEY, Content.Load<SoundEffect>(SOUND_DIR + Player.SHIELD_KEY));
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
            #endregion

            #region Content-Specific Component Init
            //COMPONENT INITIALIZATION

            //Player
            Player.Initialize();
            Player.DeadEvent += new EventHandler(Player_DeadEvent);

            //StarField
            List<Texture2D> stars = new List<Texture2D>();
            stars.Add(ContentHandler.Textures["star1"]);
            stars.Add(ContentHandler.Textures["star2"]);
            stars.Add(ContentHandler.Textures["star3"]);
            stars.Add(ContentHandler.Textures["star4"]);
            stars.Add(ContentHandler.Textures["particle"]);
            StarField.Textures = stars;
            StarField.Generate();

            //GAME UI
            GameUI.Add("fpsDisplay", new UIString<int>(60, Vector2.Zero, ContentHandler.Fonts["lcd"], Color.White, true, 1f, 0f, false)); //TEMP
            GameUI.Add("health", new UIString<string>("Health: 100", new Vector2(0f, 0.9f), ContentHandler.Fonts["lcd"], Color.White, true, 1f, 0f, false));
            GameUI.Add("loc", new UIString<string>("", new Vector2(0, 0.8f), ContentHandler.Fonts["lcd"], Color.White, true, 1f, 0f, false));

            //MENU UI
            MenuUI.Add("title", new UIString<string>("Asteroids", new Vector2(0.5f, 0.2f), ContentHandler.Fonts["title"], Color.White, true));
            MenuUI.Add("title2", new UIString<string>("INC", new Vector2(0.5f, 0.29f), ContentHandler.Fonts["title2"], Color.White, true));
            MenuUI.Add("start", new UIString<string>("Start", new Vector2(0.5f, 0.5f), ContentHandler.Fonts["menu"], Color.White, true));
            MenuUI.Add("exit", new UIString<string>("Exit", new Vector2(0.5f, 0.65f), ContentHandler.Fonts["menu"], Color.White, true));
            MenuUI.Add("sound", new UIString<string>("F11 - Toggle SFX", new Vector2(0.01f, 0.87f), ContentHandler.Fonts["lcd"], Color.White, true, 1f, 0f, false));
            MenuUI.Add("music", new UIString<string>("F12 - Toggle Music", new Vector2(0.01f, 0.93f), ContentHandler.Fonts["lcd"], Color.White, true, 1f, 0f, false));
            MenuUI.Add("fullscreen", new UIString<string>("F10 - Toggle Fullscreen", new Vector2(0.01f, 0.81f), ContentHandler.Fonts["lcd"], Color.White, true, 1f, 0f, false));

            //UI Events
            MenuUI["start"].OnClick += new UIBase.MouseClickHandler(start_OnClick);
            MenuUI["start"].MouseOver += new EventHandler(start_MouseOver);
            MenuUI["start"].MouseAway += new EventHandler(start_MouseAway);
            MenuUI["exit"].OnClick += new UIBase.MouseClickHandler(exit_OnClick);
            MenuUI["exit"].MouseOver += new EventHandler(exit_MouseOver);
            MenuUI["exit"].MouseAway += new EventHandler(exit_MouseAway);

            //AsteroidManager
            List<Texture2D> particle = new List<Texture2D>(); //particle list
            particle.Add(ContentHandler.Textures["junk1"]);
            particle.Add(ContentHandler.Textures["junk2"]);
            particle.Add(ContentHandler.Textures["junk3"]);
            List<Texture2D> asteroid = new List<Texture2D>(); //asteroid list
            asteroid.Add(ContentHandler.Textures[AsteroidManager.SMALL_ASTEROID]);
            asteroid.Add(ContentHandler.Textures[AsteroidManager.LARGE_ASTEROID]);
            asteroid.Add(ContentHandler.Textures[AsteroidManager.ORE_ASTEROID]);
            temp = new AsteroidManager(50, 100, 1, 2, asteroid, particle, true);
            //END COMPONENT INIT
            #endregion

            spriteBatch.End();
        }

        protected override void UnloadContent()
        {
            //Nothing yet...
        }

        protected override void Update(GameTime gameTime)
        {
            InputHandler.Update(); //update InputHandler regardless of gamestate

            if (InputHandler.IsNewKeyPress(Keys.F10)) //handle fullscreen switching
            {
                if (graphics.IsFullScreen)
                {
                    graphics.ToggleFullScreen();
                    graphics.PreferredBackBufferWidth = DEFAULT_WINDOWED_WIDTH;
                    graphics.PreferredBackBufferHeight = DEFAULT_WINDOWED_HEIGHT;
                    //reset to default windowed resolution and toggle

                    graphics.ApplyChanges();
                    Camera.Initialize( //reinit the camera to fit windowed mode
                        DEFAULT_WINDOWED_WIDTH,
                        DEFAULT_WINDOWED_HEIGHT,
                        Camera.WorldRectangle.Width,
                        Camera.WorldRectangle.Height);
                }
                else
                {
                    graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                    graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                    graphics.ApplyChanges();

                    graphics.ToggleFullScreen(); //set the fullscreen window to native resolution and toggle

                    Camera.Initialize( //reinit the camera to fit fullscreen
                        GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
                        GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height,
                        Camera.WorldRectangle.Width,
                        Camera.WorldRectangle.Height);
                }
            }

            if (InputHandler.IsNewKeyPress(Keys.F11))
                ContentHandler.TogglePlaySFX(); //toggle play SFX

            if (InputHandler.IsNewKeyPress(Keys.F12))
                ContentHandler.TogglePlayMusic(); //toggle play music

            switch (gameState) //MAIN GAMESTATE SWITCH
            {
                case GameState.Game:
                    //GAME UPDATE BEGIN

                    if (InputHandler.WasKeyDown(Keys.Escape))
                        SwitchGameState(GameState.Menu); //if in game and esc is pressed, exit to menu

                    StarField.Update(gameTime);
                    StarField.Scrolling = false;

                    ProjectileManager.Update(gameTime);
                    temp.Update(gameTime); //TEMP
                    Player.Update(gameTime);

                    //UI Stuff:
                    ((UIString<int>)GameUI["fpsDisplay"]).Value = (int)Math.Round(1 / gameTime.ElapsedGameTime.TotalSeconds, 0);
                    //calculate framerate to the nearest int
                    ((UIString<string>)GameUI["health"]).Value = "Health: " + Player.Health.ToString();
                    //get health
                    ((UIString<string>)GameUI["loc"]).Value = Player.Ship.WorldCenter.ToString();
                    //get loc value
                    foreach (KeyValuePair<string, UIBase> elementPair in GameUI)
                        elementPair.Value.Update(gameTime);

                    base.Update(gameTime);

                    //GAME UPDATE END
                    break;

                case GameState.Menu:

                    StarField.Scrolling = true;

                    ContentHandler.PlaySong("menu", true);

                    foreach (KeyValuePair<string, UIBase> elementPair in MenuUI)
                        elementPair.Value.Update(gameTime); //update each UI element

                    StarField.Update(gameTime);

                    if (InputHandler.IsKeyDown(Keys.Escape))
                        this.Exit(); //if in menu and esc is pressed, exit

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

                    StarField.Draw(spriteBatch);

                    ProjectileManager.Draw(spriteBatch);
                    temp.Draw(spriteBatch); //And then the rest of the game components
                    Player.Draw(spriteBatch); //Player next

                    foreach (KeyValuePair<string, UIBase> elementPair in GameUI)
                        elementPair.Value.Draw(spriteBatch);

                    spriteBatch.End(); //END SPRITE DRAW
                    base.Draw(gameTime);

                    //END GAME DRAW
                    break;
                case GameState.Menu:

                    GraphicsDevice.Clear(Color.Black);
                    spriteBatch.Begin();
                    //BEGIN DRAW

                    StarField.Draw(spriteBatch);

                    foreach (KeyValuePair<string, UIBase> elementPair in MenuUI)
                        elementPair.Value.Draw(spriteBatch);

                    //END DRAW
                    spriteBatch.End();

                    break;
                default:
                    throw new ArgumentException();
            }
        }

        #region Local Methods

        private void SwitchGameState(GameState state)
        {
            StarField.Generate();
            gameState = state; //switch the state
            ContentHandler.StopAll(); //and stop the sound
        }

        #endregion

        #region Event Handlers

        void start_OnClick(UIBase sender, MouseClickArgs e)
        {
            ContentHandler.StopAll();
            SwitchGameState(GameState.Game);
        }

        void exit_MouseAway(object sender, EventArgs e)
        {
            MenuUI["exit"].Color = NORMAL_COLOR;
        }

        void exit_MouseOver(object sender, EventArgs e)
        {
            MenuUI["exit"].Color = HIGHLIGHT_COLOR;
        }

        void exit_OnClick(UIBase sender, MouseClickArgs e)
        {
            this.Exit();
        }

        void start_MouseAway(object sender, EventArgs e)
        {
            MenuUI["start"].Color = NORMAL_COLOR;
        }

        void start_MouseOver(object sender, EventArgs e)
        {
            MenuUI["start"].Color = HIGHLIGHT_COLOR;
        }

        void Player_DeadEvent(object sender, EventArgs e)
        {
            SwitchGameState(GameState.Menu); //switch the state
            ContentHandler.PlaySong("menu"); //play menu music
            Player.Initialize(); //reset the player
        }

        #endregion
    }

    public enum GameState
    {
        Game,
        Menu
    }
}
