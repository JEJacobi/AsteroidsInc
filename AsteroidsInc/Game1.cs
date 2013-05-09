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

        //GAME UI
        UIString<int> fpsDisplay;
        UIString<string> health;

        //MENU UI
        UIString<string> title;
        UIString<string> exit;
        UIString<string> start;

        AsteroidManager temp; //TODO: Staticify
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
        }

        protected override void Initialize()
        {
            gameState = GameState.Menu; //set the initial gamestate

            Camera.ScreenSize.X = GraphicsDevice.Viewport.Bounds.Width; //init the camera
            Camera.ScreenSize.Y = GraphicsDevice.Viewport.Bounds.Height;
            Camera.WorldRectangle = new Rectangle(0, 0, (int)Camera.ScreenSize.X * 3, (int)Camera.ScreenSize.Y * 3); //create the world
            Camera.CenterPosition = new Vector2(Camera.WorldRectangle.Width / 2, Camera.WorldRectangle.Height / 2);
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
            ContentHandler.Fonts.Add("title", Content.Load<SpriteFont>(FONT_DIR + "/title"));

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
            fpsDisplay = new UIString<int>(60, Vector2.Zero, ContentHandler.Fonts["lcd"], Color.White, true, 1f, 0f, false); //TEMP
            health = new UIString<string>("Health: 100", new Vector2(0f, 0.9f), ContentHandler.Fonts["lcd"], Color.White, true, 1f, 0f, false);

            //MENU UI
            title = new UIString<string>("Asteroids Inc.", new Vector2(0.5f, 0.2f), ContentHandler.Fonts["title"], Color.White, true);
            start = new UIString<string>("Start!", new Vector2(0.5f, 0.5f), ContentHandler.Fonts["lcd"], Color.White, true);
            exit = new UIString<string>("Exit?", new Vector2(0.5f, 0.55f), ContentHandler.Fonts["lcd"], Color.White, true);

            //UI Events
            start.OnClick += new UIBase.MouseClickHandler(start_OnClick);
            start.MouseOver += new EventHandler(start_MouseOver);
            start.MouseAway += new EventHandler(start_MouseAway);
            exit.OnClick += new UIBase.MouseClickHandler(exit_OnClick);
            exit.MouseOver += new EventHandler(exit_MouseOver);
            exit.MouseAway += new EventHandler(exit_MouseAway);

            //AsteroidManager
            List<Texture2D> particle = new List<Texture2D>(); //particle list
            particle.Add(ContentHandler.Textures["junk1"]);
            particle.Add(ContentHandler.Textures["junk2"]);
            particle.Add(ContentHandler.Textures["junk3"]);
            List<Texture2D> asteroid = new List<Texture2D>(); //asteroid list
            asteroid.Add(ContentHandler.Textures[AsteroidManager.SMALL_ASTEROID]);
            asteroid.Add(ContentHandler.Textures[AsteroidManager.LARGE_ASTEROID]);
            asteroid.Add(ContentHandler.Textures[AsteroidManager.ORE_ASTEROID]);
            temp = new AsteroidManager(15, 50, 100, 1, 2, asteroid, particle, true);
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

            switch (gameState) //MAIN GAMESTATE SWITCH
            {
                case GameState.Game:
                    //GAME UPDATE BEGIN

                    if (InputHandler.WasKeyDown(Keys.Escape))
                        SwitchGameState(GameState.Menu); //if in game and esc is pressed, exit to menu

                    StarField.Update(gameTime);
                    StarField.Scrolling = false;

                    ProjectileManager.Update(gameTime);
                    Player.Update(gameTime);

                    //UI Stuff:
                    fpsDisplay.Value = (int)Math.Round(1 / gameTime.ElapsedGameTime.TotalSeconds, 0);
                    //calculate framerate to the nearest int
                    health.Value = "Health: " + Player.Health.ToString();
                    //get health
                    temp.Update(gameTime);
                    title.Update(gameTime);

                    base.Update(gameTime);

                    //GAME UPDATE END
                    break;

                case GameState.Menu:

                    StarField.Scrolling = true;

                    ContentHandler.PlaySong("menu", true);

                    title.Update(gameTime);
                    start.Update(gameTime);
                    exit.Update(gameTime);

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
                    fpsDisplay.Draw(spriteBatch);
                    health.Draw(spriteBatch); //UI elements last

                    spriteBatch.End(); //END SPRITE DRAW
                    base.Draw(gameTime);

                    //END GAME DRAW
                    break;
                case GameState.Menu:

                    GraphicsDevice.Clear(Color.Black);
                    spriteBatch.Begin();
                    //BEGIN DRAW

                    StarField.Draw(spriteBatch);

                    title.Draw(spriteBatch);
                    start.Draw(spriteBatch);
                    exit.Draw(spriteBatch);

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
            exit.Color = Color.White;
        }

        void exit_MouseOver(object sender, EventArgs e)
        {
            exit.Color = Color.Yellow;
        }

        void exit_OnClick(UIBase sender, MouseClickArgs e)
        {
            this.Exit();
        }

        void start_MouseAway(object sender, EventArgs e)
        {
            start.Color = Color.White;
        }

        void start_MouseOver(object sender, EventArgs e)
        {
            start.Color = Color.Yellow;
        }

        void Player_DeadEvent(object sender, EventArgs e)
        {
            SwitchGameState(GameState.Menu);
            ContentHandler.PlaySong("menu");
        }

        #endregion
    }

    public enum GameState
    {
        Game,
        Menu
    }
}
