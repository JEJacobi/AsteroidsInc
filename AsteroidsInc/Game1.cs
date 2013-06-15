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

        //Core game components
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GameState gameState;

        //UI element dictionaries
        Dictionary<string, UIBase> GameUI;
        Dictionary<string, UIBase> MenuUI;
        Dictionary<string, UIBase> UpgradeUI;
        Dictionary<string, UIBase> HelpUI;

        #endregion

        #region Properties & Constants

        //Content address prefixes
        const string TEXTURE_DIR = "Textures/";
        const string FONT_DIR = "Fonts/";
        const string SOUND_DIR = "Sound/";
        const string MUSIC_DIR = "Music/";

        //Default windowed mode's resolution
        const int DEFAULT_WINDOWED_WIDTH = 1024;
        const int DEFAULT_WINDOWED_HEIGHT = 768;
        
        const int TEMP_STARS_TO_GEN = 100; //TEMP
        const int TEMP_ASTEROIDS_TO_GEN = 35;

        const int WORLD_SIZE = 3000;

        //UI constants
        readonly Color HIGHLIGHT_COLOR = Color.Yellow;
        readonly Color NORMAL_COLOR = Color.White;
        const float HIGHLIGHT_SCALE = 1f;
        const float NORMAL_SCALE = 1f;

        int selectedUpgrade = 0;
        bool played = false;
        GameState last = GameState.Upgrade;

	    #endregion

        #region Events

        public delegate void StateChangeHandler(object sender, StateArgs e);
        public event StateChangeHandler OnStateChange;

        #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = DEFAULT_WINDOWED_HEIGHT;
            graphics.PreferredBackBufferWidth = DEFAULT_WINDOWED_WIDTH; //set the window size to defaults
            graphics.PreferMultiSampling = true; //turn on multisampling
            Content.RootDirectory = "Content";
            this.IsMouseVisible = true; //mouse should be visible
        }

        protected override void Initialize()
        {
            gameState = GameState.Menu; //set the initial gamestate

            Camera.Initialize( //initialize the camera
                DEFAULT_WINDOWED_WIDTH,
                DEFAULT_WINDOWED_HEIGHT,
                LevelManager.Levels[0].WorldSizeX,
                LevelManager.Levels[0].WorldSizeY);

            Camera.Position = Vector2.Zero;

            GameUI = new Dictionary<string, UIBase>();
            MenuUI = new Dictionary<string, UIBase>();
            UpgradeUI = new Dictionary<string, UIBase>();
            HelpUI = new Dictionary<string, UIBase>();

            OnStateChange += new StateChangeHandler(handleSwitching);

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

            ContentHandler.Textures.Add(OreManager.ORE_KEY, Content.Load<Texture2D>(TEXTURE_DIR + OreManager.ORE_KEY));

            ContentHandler.Textures.Add("junk1", Content.Load<Texture2D>(TEXTURE_DIR + "SmallJunk01"));
            ContentHandler.Textures.Add("junk2", Content.Load<Texture2D>(TEXTURE_DIR + "SmallJunk02"));
            ContentHandler.Textures.Add("junk3", Content.Load<Texture2D>(TEXTURE_DIR + "SmallJunk03"));

            ContentHandler.Textures.Add(Player.SHIP_TEXTURE, Content.Load<Texture2D>(TEXTURE_DIR + Player.SHIP_TEXTURE));
            ContentHandler.Textures.Add(Player.SHIELD_KEY, Content.Load<Texture2D>(TEXTURE_DIR + Player.SHIELD_KEY));
            ContentHandler.Textures.Add(Player.TORPEDO_KEY, Content.Load<Texture2D>(TEXTURE_DIR + Player.TORPEDO_KEY));
            ContentHandler.Textures.Add(Player.LASER_KEY, Content.Load<Texture2D>(TEXTURE_DIR + Player.LASER_KEY));
            ContentHandler.Textures.Add(Player.SHELL_KEY, Content.Load<Texture2D>(TEXTURE_DIR + Player.SHELL_KEY));
            ContentHandler.Textures.Add(Player.SLIVER_KEY, Content.Load<Texture2D>(TEXTURE_DIR + Player.SLIVER_KEY));
            ContentHandler.Textures.Add(Player.MISSILE_KEY, Content.Load<Texture2D>(TEXTURE_DIR + Player.MISSILE_KEY));

            ContentHandler.Textures.Add(NPCManager.MINE_KEY, Content.Load<Texture2D>(TEXTURE_DIR + NPCManager.MINE_KEY));
            ContentHandler.Textures.Add(NPCManager.DRONE_KEY, Content.Load<Texture2D>(TEXTURE_DIR + NPCManager.DRONE_KEY));
            ContentHandler.Textures.Add(NPCManager.FIGHTER_KEY, Content.Load<Texture2D>(TEXTURE_DIR + NPCManager.FIGHTER_KEY));
            ContentHandler.Textures.Add(NPCManager.BOMBER_KEY, Content.Load<Texture2D>(TEXTURE_DIR + NPCManager.BOMBER_KEY));

            ContentHandler.Textures.Add("star1", Content.Load<Texture2D>(TEXTURE_DIR + "star1"));
            ContentHandler.Textures.Add("star2", Content.Load<Texture2D>(TEXTURE_DIR + "star2"));
            ContentHandler.Textures.Add("star3", Content.Load<Texture2D>(TEXTURE_DIR + "star3"));
            ContentHandler.Textures.Add("star4", Content.Load<Texture2D>(TEXTURE_DIR + "star4"));

            ContentHandler.Textures.Add("smallOre", Content.Load<Texture2D>(TEXTURE_DIR + "smallOre"));
            ContentHandler.Textures.Add("staticship", Content.Load<Texture2D>(TEXTURE_DIR + "staticship"));

            ContentHandler.Textures.Add("particle", //General generated particle texture
                Util.GetColorTexture(GraphicsDevice, Color.White, 2, 2));

            //SOUNDEFFECTS

            //Static SFX:
            ContentHandler.SFX.Add(Player.COLLISION_SFX, Content.Load<SoundEffect>(SOUND_DIR + Player.COLLISION_SFX));
            ContentHandler.SFX.Add(Player.IMPACT_SFX, Content.Load<SoundEffect>(SOUND_DIR + Player.IMPACT_SFX));
            ContentHandler.SFX.Add(Player.SHIELD_KEY, Content.Load<SoundEffect>(SOUND_DIR + Player.SHIELD_KEY));
            ContentHandler.SFX.Add(Player.TORPEDO_KEY, Content.Load<SoundEffect>(SOUND_DIR + Player.TORPEDO_KEY));
            ContentHandler.SFX.Add(Player.LASER_KEY, Content.Load<SoundEffect>(SOUND_DIR + Player.LASER_KEY));
            ContentHandler.SFX.Add(Player.SHELL_KEY, Content.Load<SoundEffect>(SOUND_DIR + Player.SHELL_KEY));
            ContentHandler.SFX.Add(Player.SLIVER_KEY, Content.Load<SoundEffect>(SOUND_DIR + Player.SLIVER_KEY));
            ContentHandler.SFX.Add(Player.MISSILE_KEY, Content.Load<SoundEffect>(SOUND_DIR + Player.MISSILE_KEY));
            ContentHandler.SFX.Add(Player.WIN_SFX, Content.Load<SoundEffect>(SOUND_DIR + Player.WIN_SFX));
            ContentHandler.SFX.Add("switch", Content.Load<SoundEffect>(SOUND_DIR + "switch"));
            ContentHandler.SFX.Add("click", Content.Load<SoundEffect>(SOUND_DIR + "click"));
            ContentHandler.SFX.Add("pickup", Content.Load<SoundEffect>(SOUND_DIR + "pickup"));
            ContentHandler.SFX.Add("pickup2", Content.Load<SoundEffect>(SOUND_DIR + "pickup2"));
            ContentHandler.SFX.Add("error", Content.Load<SoundEffect>(SOUND_DIR + "error"));

            //Instances:
            ContentHandler.InstanceSFX.Add(Player.ENGINE_SFX, Content.Load<SoundEffect>(SOUND_DIR + Player.ENGINE_SFX).CreateInstance());
            ContentHandler.InstanceSFX[Player.ENGINE_SFX].IsLooped = true; //init and add engine sound
            ContentHandler.InstanceSFX.Add(Player.DEATH_SFX, Content.Load<SoundEffect>(SOUND_DIR + Player.DEATH_SFX).CreateInstance());
            ContentHandler.InstanceSFX.Add("alarm", Content.Load<SoundEffect>(SOUND_DIR + "alarm").CreateInstance());
            ContentHandler.InstanceSFX["alarm"].IsLooped = true;

            //MUSIC
            ContentHandler.Songs.Add("menu", Content.Load<Song>(MUSIC_DIR + "menu"));
            ContentHandler.Songs.Add("combat1", Content.Load<Song>(MUSIC_DIR + "combat1"));
            ContentHandler.Songs.Add("combat2", Content.Load<Song>(MUSIC_DIR + "combat2"));

            //END CONTENT LOAD
            #endregion

            #region Content-Specific Component Init
            //COMPONENT INITIALIZATION

            //Player
            Player.Initialize();
            Player.DeadEvent += new EventHandler(Player_DeadEvent);
            Player.LevelCompleteEvent += new EventHandler(Player_LevelCompleteEvent);
            Player.ActiveSlotChanged += new EventHandler(updateSlots);

            //StarField
            List<Texture2D> stars = new List<Texture2D>();
            stars.Add(ContentHandler.Textures["star1"]);
            stars.Add(ContentHandler.Textures["star2"]);
            stars.Add(ContentHandler.Textures["star3"]);
            stars.Add(ContentHandler.Textures["star4"]);
            stars.Add(ContentHandler.Textures["particle"]);
            StarField.Textures = stars;
            StarField.Scrolling = true;
            StarField.Generate(LevelManager.Levels[0].Stars);

            //NPCManager
            NPCManager.EXPLOSION_TEXTURES.Add(ContentHandler.Textures["junk1"]);
            NPCManager.EXPLOSION_TEXTURES.Add(ContentHandler.Textures["junk2"]);
            NPCManager.EXPLOSION_TEXTURES.Add(ContentHandler.Textures["junk3"]);
            NPCManager.Generate(LevelManager.Levels[0]);

            //GAME UI
            GameUI.Add("oreCount", new UIString<int>(0 , new Vector2(0.057f, 0.02f), ContentHandler.Fonts["lcd"], Color.White, true, 1f, 0f, false));
            GameUI.Add("health", new UIString<string>("Hull Integrity: 100", new Vector2(0.005f, 0.95f), ContentHandler.Fonts["lcd"], Color.Green, true, 1f, 0f, false));
            GameUI.Add("oreSprite", new UISprite(ContentHandler.Textures["smallOre"], new Vector2(0.01f, 0.01f), Color.White, true, 1f, 0f, false));
            GameUI.Add("x", new UIString<string>("x", new Vector2(0.035f, 0.02f), ContentHandler.Fonts["lcd"], Color.White, true, 1f, 0f, false));
            GameUI.Add("warning", new UIString<string>("HULL INTEGRITY CRITICAL", new Vector2(0.5f, 0.2f), ContentHandler.Fonts["lcd"], Color.Red));
            GameUI.Add("sector", new UIString<string>("Sector: 1", new Vector2(0.5f, 0.03f), ContentHandler.Fonts["lcd"], Color.White, true));
            GameUI.Add("levelcomplete", new UIString<string>("Sector Clear - Press Escape to Exit", new Vector2(0.5f, 0.4f), ContentHandler.Fonts["lcd"], Color.White));

            //MENU UI
            MenuUI.Add("title", new UIString<string>("Asteroids", new Vector2(0.5f, 0.2f), ContentHandler.Fonts["title"], Color.White, true));
            MenuUI.Add("title2", new UIString<string>("INC", new Vector2(0.5f, 0.29f), ContentHandler.Fonts["title2"], Color.White, true));
            MenuUI.Add("start", new UIString<string>("Start", new Vector2(0.5f, 0.5f), ContentHandler.Fonts["menu"], Color.White, true, NORMAL_SCALE));
            MenuUI.Add("exit", new UIString<string>("Exit", new Vector2(0.5f, 0.65f), ContentHandler.Fonts["menu"], Color.White, true, NORMAL_SCALE));
            MenuUI.Add("sound", new UIString<string>("F11 - Toggle SFX", new Vector2(0.01f, 0.90f), ContentHandler.Fonts["lcd"], Color.White, true, 1f, 0f, false));
            MenuUI.Add("music", new UIString<string>("F12 - Toggle Music", new Vector2(0.01f, 0.93f), ContentHandler.Fonts["lcd"], Color.White, true, 1f, 0f, false));
            MenuUI.Add("fullscreen", new UIString<string>("F10 - Toggle Fullscreen", new Vector2(0.01f, 0.96f), ContentHandler.Fonts["lcd"], Color.White, true, 1f, 0f, false));
            MenuUI["exit"].XPadding = -10;
            MenuUI.Add("help", new UIString<string>("Controls / About", new Vector2(0.85f, 0.95f), ContentHandler.Fonts["lcd"], Color.White, true));

            //HELP UI
            HelpUI.Add("return", new UIString<string>("Return to Menu", new Vector2(0.5f, 0.97f), ContentHandler.Fonts["menu"], Color.White, true, NORMAL_SCALE));
            HelpUI.Add("controls", new UIString<string>("Controls:", new Vector2(0.5f, 0.1f), ContentHandler.Fonts["title2"], Color.White, true));
            HelpUI.Add("keys", new UIString<string>("Arrow Keys - Rotate/Thrust", new Vector2(0.5f, 0.2f), ContentHandler.Fonts["lcd"], Color.White, true));
            HelpUI.Add("keys2", new UIString<string>("Spacebar - Fire", new Vector2(0.5f, 0.25f), ContentHandler.Fonts["lcd"], Color.White, true));
            HelpUI.Add("keys3", new UIString<string>("F - Switch Weapon", new Vector2(0.5f, 0.3f), ContentHandler.Fonts["lcd"], Color.White, true));
            HelpUI.Add("keys4", new UIString<string>("Shift - Boost", new Vector2(0.5f, 0.35f), ContentHandler.Fonts["lcd"], Color.White, true));
            HelpUI.Add("thanks", new UIString<string>("Thanks To:", new Vector2(0.5f, 0.5f), ContentHandler.Fonts["title2"], Color.White, true));
            HelpUI.Add("credit", new UIString<string>("Everyone from OpenGameArt.org including:", new Vector2(0.5f, 0.6f), ContentHandler.Fonts["lcd"], Color.White, true));
            HelpUI.Add("credit2", new UIString<string>("qubodup", new Vector2(0.5f, 0.65f), ContentHandler.Fonts["lcd"], Color.White, true));
            HelpUI.Add("credit3", new UIString<string>("HaelDB", new Vector2(0.5f, 0.7f), ContentHandler.Fonts["lcd"], Color.White, true));
            HelpUI.Add("credit4", new UIString<string>("wuhu", new Vector2(0.5f, 0.75f), ContentHandler.Fonts["lcd"], Color.White, true));
            HelpUI.Add("credit5", new UIString<string>("GameArtForge", new Vector2(0.5f, 0.8f), ContentHandler.Fonts["lcd"], Color.White, true));
            HelpUI.Add("credit6", new UIString<string>("Skorpio", new Vector2(0.5f, 0.85f), ContentHandler.Fonts["lcd"], Color.White, true));
            HelpUI.Add("credit8", new UIString<string>("DST, Clearside, and Michel Baradari - Music", new Vector2(0.5f, 0.9f), ContentHandler.Fonts["lcd"], Color.White, true));

            //UPGRADE UI
            UpgradeUI.Add("ship", new UISprite(ContentHandler.Textures["staticship"], new Vector2(0.5f, 0.45f), Color.White, true));
            UpgradeUI.Add("title", new UIString<string>("Equipment:", new Vector2(0.5f, 0.18f), ContentHandler.Fonts["lcd"], Color.White, true));
            UpgradeUI.Add("description", new UIString<string>("", new Vector2(0.5f, 0.25f), ContentHandler.Fonts["lcd"], Color.White, true));
            UpgradeUI.Add("projectile1", new UISprite(ContentHandler.Textures["laser"], new Vector2(0.5f, 0.38f), Color.White, true, 1f));
            UpgradeUI.Add("projectile2", new UISprite(ContentHandler.Textures["laser"], new Vector2(0.5f, 0.315f), Color.White, true, 1f));
            UpgradeUI.Add("continue", new UIString<string>("Continue", new Vector2(0.5f, 0.8f), ContentHandler.Fonts["menu"], Color.White, true, NORMAL_SCALE));
            UpgradeUI.Add("exitToMenu", new UIString<string>("Exit to Menu", new Vector2(0.5f, 0.96f), ContentHandler.Fonts["menu"], Color.White, true, NORMAL_SCALE));
            UpgradeUI.Add("buy", new UIString<string>("Buy", new Vector2(0.5f, 0.6f), ContentHandler.Fonts["menu"], Color.White, true));
            UpgradeUI.Add("next", new UIString<string>("Next", new Vector2(0.666f, 0.6f), ContentHandler.Fonts["menu"], Color.White, true, NORMAL_SCALE));
            UpgradeUI.Add("back", new UIString<string>("Back", new Vector2(0.333f, 0.6f), ContentHandler.Fonts["menu"], Color.White, true, NORMAL_SCALE));
            UpgradeUI.Add("slot1", new UIString<string>("Slot 1", new Vector2(0.333f, 0.1f), ContentHandler.Fonts["menu"], Color.White, true, NORMAL_SCALE));
            UpgradeUI.Add("slot2", new UIString<string>("Slot 2", new Vector2(0.666f, 0.1f), ContentHandler.Fonts["menu"], Color.White, true, NORMAL_SCALE));
            UpgradeUI.Add("cost", new UIString<string>("Cost: 0", new Vector2(0.5f, 0.52f), ContentHandler.Fonts["lcd"], Color.White, true));
            UpgradeUI.Add("damage", new UIString<string>("Damage: ", new Vector2(0.333f, 0.5f), ContentHandler.Fonts["lcd"], Color.White, true));
            UpgradeUI.Add("range", new UIString<string>("Range: ", new Vector2(0.666f, 0.5f), ContentHandler.Fonts["lcd"], Color.White, true));
            UpgradeUI.Add("currentOre", new UIString<string>("Ore: ", new Vector2(0.5f, 0.67f), ContentHandler.Fonts["lcd"], Color.White, true));
            UpgradeUI.Add("rof", new UIString<string>("Refire: ", new Vector2(0.333f, 0.4f), ContentHandler.Fonts["lcd"], Color.White, true));
            UpgradeUI.Add("speed", new UIString<string>("Speed: ", new Vector2(0.666f, 0.4f), ContentHandler.Fonts["lcd"], Color.White, true));
            UpgradeUI.Add("health", new UIString<string>("Hull Status: 100", new Vector2(0.01f, 0.8f), ContentHandler.Fonts["lcd"], Color.White, true, 1f, 0f, false));
            UpgradeUI.Add("repaircost", new UIString<string>("Repair Cost: 0", new Vector2(0.01f, 0.85f), ContentHandler.Fonts["lcd"], Color.White, true, 1f, 0f, false));
            UpgradeUI.Add("repair", new UIString<string>("Repair", new Vector2(0.01f, 0.9f), ContentHandler.Fonts["menu"], Color.White, true, NORMAL_SCALE, 0f, false));

            //UI Events
            MenuUI["start"].OnClick += new UIBase.MouseClickHandler(start_OnClick);
            MenuUI["start"].MouseOver += new EventHandler(start_MouseOver);
            MenuUI["start"].MouseAway += new EventHandler(start_MouseAway);
            MenuUI["exit"].OnClick += new UIBase.MouseClickHandler(exit_OnClick);
            MenuUI["exit"].MouseOver += new EventHandler(exit_MouseOver);
            MenuUI["exit"].MouseAway += new EventHandler(exit_MouseAway);
            MenuUI["help"].MouseOver += new EventHandler(help_MouseOver);
            MenuUI["help"].MouseAway += new EventHandler(help_MouseAway);
            MenuUI["help"].OnClick += new UIBase.MouseClickHandler(help_OnClick);
            HelpUI["return"].MouseOver += new EventHandler(return_MouseOver);
            HelpUI["return"].MouseAway += new EventHandler(return_MouseAway);
            HelpUI["return"].OnClick += new UIBase.MouseClickHandler(return_OnClick);
            UpgradeUI["slot1"].MouseOver += new EventHandler(slot1_MouseOver);
            UpgradeUI["slot2"].MouseOver += new EventHandler(slot2_MouseOver);
            UpgradeUI["slot1"].MouseAway += new EventHandler(slot1_MouseAway);
            UpgradeUI["slot2"].MouseAway += new EventHandler(slot2_MouseAway);
            UpgradeUI["slot1"].OnClick += new UIBase.MouseClickHandler(slot1_OnClick);
            UpgradeUI["slot2"].OnClick += new UIBase.MouseClickHandler(slot2_OnClick);
            UpgradeUI["continue"].MouseAway += new EventHandler(continue_MouseAway);
            UpgradeUI["continue"].MouseOver += new EventHandler(continue_MouseOver);
            UpgradeUI["continue"].OnClick += new UIBase.MouseClickHandler(continue_OnClick);
            UpgradeUI["exitToMenu"].MouseOver += new EventHandler(exitToMenu_MouseOver);
            UpgradeUI["exitToMenu"].MouseAway += new EventHandler(exitToMenu_MouseAway);
            UpgradeUI["exitToMenu"].OnClick += new UIBase.MouseClickHandler(exitToMenu_OnClick);
            UpgradeUI["next"].MouseOver += new EventHandler(next_MouseOver);
            UpgradeUI["next"].MouseAway += new EventHandler(next_MouseAway);
            UpgradeUI["back"].MouseOver += new EventHandler(back_MouseOver);
            UpgradeUI["back"].MouseAway += new EventHandler(back_MouseAway);
            UpgradeUI["buy"].MouseOver += new EventHandler(buy_MouseOver);
            UpgradeUI["buy"].MouseAway += new EventHandler(buy_MouseAway);
            UpgradeUI["next"].OnClick += new UIBase.MouseClickHandler(next_OnClick);
            UpgradeUI["back"].OnClick += new UIBase.MouseClickHandler(back_OnClick);
            UpgradeUI["buy"].OnClick += new UIBase.MouseClickHandler(buy_OnClick);
            UpgradeUI["repair"].OnClick += new UIBase.MouseClickHandler(repair_OnClick);
            UpgradeUI["repair"].MouseOver += new EventHandler(repair_MouseOver);
            UpgradeUI["repair"].MouseAway += new EventHandler(repair_MouseAway);

            //AsteroidManager
            List<Texture2D> particle = new List<Texture2D>(); //particle list
            particle.Add(ContentHandler.Textures["junk1"]);
            particle.Add(ContentHandler.Textures["junk2"]);
            particle.Add(ContentHandler.Textures["junk3"]);
            List<Texture2D> asteroid = new List<Texture2D>(); //asteroid list
            asteroid.Add(ContentHandler.Textures[AsteroidManager.SMALL_ASTEROID]);
            asteroid.Add(ContentHandler.Textures[AsteroidManager.LARGE_ASTEROID]);
            asteroid.Add(ContentHandler.Textures[AsteroidManager.ORE_ASTEROID]);
            AsteroidManager.Initialize(
                LevelManager.Levels[0].Asteroids,
                asteroid,
                particle,
                true);

            //OreManager
            OreManager.Initialize(
                ContentHandler.Textures[OreManager.ORE_KEY].ToTextureList(),
                ContentHandler.Textures["particle"].ToTextureList());

            //END COMPONENT INIT
            #endregion

            spriteBatch.End();

            SwitchGameState(GameState.Menu);
        }

        protected override void UnloadContent()
        {
            //Nothing yet...
        }

        protected override void Update(GameTime gameTime)
        {
            InputHandler.Update(); //update InputHandler regardless of gamestate
            handleGlobalInputs(); //handle any global inputs

            switch (gameState) //MAIN GAMESTATE SWITCH
            {
                case GameState.Game:
                    //GAME UPDATE BEGIN

                    updateGameUI(gameTime);

                    StarField.Update(gameTime);
                    ProjectileManager.Update(gameTime);
                    AsteroidManager.Update(gameTime);
                    OreManager.Update(gameTime);
                    NPCManager.Update(gameTime);
                    Player.Update(gameTime);

                    if (InputHandler.WasKeyDown(Keys.Escape))
                        SwitchGameState(GameState.Menu); //if in game and esc is pressed, exit to menu

                    base.Update(gameTime);

                    //GAME UPDATE END
                    break;
                case GameState.Menu:
                    //MENU UPDATE BEGIN

                    foreach (KeyValuePair<string, UIBase> elementPair in MenuUI)
                        elementPair.Value.Update(gameTime); //update each UI element

                    StarField.Update(gameTime);

                    if (InputHandler.IsKeyDown(Keys.Escape))
                        this.Exit(); //if in menu and esc is pressed, exit

                    //MENU UPDATE END
                    break;
                case GameState.Upgrade:
                    //UPGRADE UPDATE BEGIN

                    foreach (KeyValuePair<string, UIBase> elementPair in UpgradeUI)
                        elementPair.Value.Update(gameTime);

                    StarField.Update(gameTime);

                    //UPGRADE UPDATE END
                    break;
                case GameState.Help:
                    //HELP UPGRADE BEGIN

                    foreach (KeyValuePair<string, UIBase> elementPair in HelpUI)
                        elementPair.Value.Update(gameTime);

                    StarField.Update(gameTime);

                    //HELP UPGRADE END
                    break;
                default:
                    break;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(); //BEGIN SPRITE DRAW

            switch (gameState) //MAIN GAMESTATE DRAW SWITCH
            {
                case GameState.Game:
                    //BEGIN GAME DRAW
                    StarField.Draw(spriteBatch);
                    ProjectileManager.Draw(spriteBatch);
                    OreManager.Draw(spriteBatch);
                    AsteroidManager.Draw(spriteBatch); //And then the rest of the game components
                    NPCManager.Draw(spriteBatch);
                    Player.Draw(spriteBatch); //Player next

                    foreach (KeyValuePair<string, UIBase> elementPair in GameUI)
                        elementPair.Value.Draw(spriteBatch); //draw each element in GameUI

                    //END GAME DRAW
                    break;
                case GameState.Menu:
                    //BEGIN MENU DRAW

                    StarField.Draw(spriteBatch);

                    foreach (KeyValuePair<string, UIBase> elementPair in MenuUI)
                        elementPair.Value.Draw(spriteBatch); //draw each element in MenuUI

                    //END MENU DRAW
                    break;
                case GameState.Upgrade:
                    //BEGIN UPGRADE DRAW

                    StarField.Draw(spriteBatch);

                    foreach (KeyValuePair<string, UIBase> elementPair in UpgradeUI)
                        elementPair.Value.Draw(spriteBatch);

                    //END UPGRADE DRAW
                    break;
                case GameState.Help:
                    //BEGIN HELP DRAW

                    StarField.Draw(spriteBatch);

                    foreach (KeyValuePair<string, UIBase> elementPair in HelpUI)
                        elementPair.Value.Draw(spriteBatch);

                    //END HELP DRAW
                    break;
                default:
                    spriteBatch.DrawString(
                        ContentHandler.Fonts["lcd"],
                        "INVALID GAME STATE",
                        Camera.ScreenSize / 2,
                        Color.Red);
                    break;
            }

            base.Draw(gameTime);
            spriteBatch.End(); //END SPRITE DRAW
        }

        #region Local Methods

        private void SwitchGameState(GameState state)
        {
            if (OnStateChange != null) //trigger the event with state data
                OnStateChange(this, new StateArgs(gameState, state));
            gameState = state; //switch the state
        }

        private void handleGlobalInputs() //handle things like fullscreen/windowed switching, toggling music on/off, etc
        {
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
        }

        private void updateGameUI(GameTime gameTime)
        {
            ((UIString<string>)GameUI["health"]).Value = "Hull Integrity: " + Player.Health.ToString();
            if (Player.Health > 66)
                GameUI["health"].Color = Color.Green;
            else if (Player.Health > 33 && Player.Health <= 66)
                GameUI["health"].Color = Color.Yellow;
            else
            {
                GameUI["health"].Color = Color.Red;
                GameUI["warning"].Active = true;

                if (Player.Health <= 10)
                    ContentHandler.PlaySFX("alarm");
                else
                    ContentHandler.StopInstancedSFX("alarm");
            }
            //get health / set color
            ((UIString<int>)GameUI["oreCount"]).Value = Player.CurrentOre;
            //get ore count
            ((UIString<string>)GameUI["sector"]).Value = "Sector: " + (LevelManager.Counter + 1).ToString();

            foreach (KeyValuePair<string, UIBase> elementPair in GameUI)
                elementPair.Value.Update(gameTime);
        }

        #endregion

        #region Event Handlers

        //Game state switch logic
        void handleSwitching(object sender, StateArgs e)
        {
            ContentHandler.StopAll(); //stop the sound

            switch (e.TargetState)
            {
                case GameState.Game:

                    Camera.CenterPosition = Player.Ship.WorldCenter; //recenter the camera
                    StarField.Scrolling = false; //cancel the scrolling

                    last = GameState.Game;

                    break;
                case GameState.Menu:

                    StarField.Scrolling = true; //set the starfield to scroll for a decorative effect
                    Camera.Position = Camera.UL_CORNER; //and set the camera to the upper-left corner of the world
                    start_MouseAway(this, null); //clear the selected button
                    ContentHandler.PlaySong("menu", true); //play menu music if target is menu

                    break;
                case GameState.Upgrade:

                    StarField.Scrolling = true;

                    ((UIString<string>)MenuUI["start"]).Value = "Resume"; //turn the start button into a resume one

                    switch (Player.ActiveSlot)
                    {
                        case Slots.Slot1:
                            UpgradeUI["slot1"].Color = Color.Red;
                            break;
                        case Slots.Slot2:
                            UpgradeUI["slot2"].Color = Color.Red;
                            break;
                        default:
                            break;
                    }

                    last = GameState.Upgrade;

                    continue_MouseAway(this, null);
                    exitToMenu_MouseAway(this, null);
                    slot1_OnClick(null, null);
                    slot2_MouseAway(this, null);

                    updateSlots(this, EventArgs.Empty);

                    break;
                default:
                    break;
            }
        }

        //On level complete
        void Player_LevelCompleteEvent(object sender, EventArgs e)
        {
            AsteroidManager.RegenerateAsteroids = false;
            if (!played)
            {
                ContentHandler.PlaySFX(Player.WIN_SFX);
                played = true;
            }
            GameUI["levelcomplete"].Active = true;

            if (InputHandler.IsKeyDown(Keys.Escape))
            {
                played = false;
                AsteroidManager.RegenerateAsteroids = true;
                GameUI["warning"].Active = false;
                GameUI["levelcomplete"].Active = false;
                LevelManager.NextLevel();
                SwitchGameState(GameState.Upgrade);
            }
        }

        //On player death
        void Player_DeadEvent(object sender, EventArgs e)
        {
            StarField.Generate(TEMP_STARS_TO_GEN); //regenerate the starfield
            SwitchGameState(GameState.Menu); //switch the state
            ((UIString<string>)MenuUI["start"]).Value = "Start"; //player has died, reset the resume button
            Player.Reset(); //reset the player
        }

        void updateSlots(object sender, EventArgs e) //update Upgrade UI's slots
        {
            var temp = Player.GetEquipmentList;
            switch (Player.ActiveSlot)
            {
                case Slots.Slot1:
                    selectedUpgrade =
                        temp.IndexOf(new KeyValuePair<string, EquipmentData>(
                        Player.Slot1, Player.EquipmentDictionary[Player.Slot1]));
                    break;
                case Slots.Slot2:
                    selectedUpgrade =
                        temp.IndexOf(new KeyValuePair<string, EquipmentData>(
                            Player.Slot2, Player.EquipmentDictionary[Player.Slot2]));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            updateSelected();
        }

        void updateSelected() //update the Upgrade UI
        {
            var temp = Player.GetEquipmentList;

            ((UIString<string>)UpgradeUI["description"]).Value = temp[selectedUpgrade].Value.Description;
            ((UISprite)UpgradeUI["projectile1"]).Texture = temp[selectedUpgrade].Value.Texture;
            ((UISprite)UpgradeUI["projectile2"]).Texture = temp[selectedUpgrade].Value.Texture;
            ((UIString<string>)UpgradeUI["cost"]).Value = "Cost: " + temp[selectedUpgrade].Value.OreCost.ToString();
            ((UIString<string>)UpgradeUI["damage"]).Value = "Damage: " + temp[selectedUpgrade].Value.Damage.ToString();
            ((UIString<string>)UpgradeUI["range"]).Value = "Range: " + temp[selectedUpgrade].Value.MaxRange.ToString();
            ((UIString<string>)UpgradeUI["currentOre"]).Value = "Current Ore: " + Player.StoredOre.ToString();
            ((UIString<string>)UpgradeUI["speed"]).Value = "Speed: " + temp[selectedUpgrade].Value.Speed.ToString();
            ((UIString<string>)UpgradeUI["rof"]).Value = "Refire: " +
                temp[selectedUpgrade].Value.RefireDelay / temp[selectedUpgrade].Value.ShotsPerLaunch;
            ((UIString<string>)UpgradeUI["health"]).Value = "Hull Status: " + Player.Health.ToString();
            ((UIString<string>)UpgradeUI["repaircost"]).Value = "Repair Cost: " + Player.RepairCost;

            if (Player.RepairCost <= Player.StoredOre && Player.Health != Player.STARTING_HEALTH)
                UpgradeUI["repair"].Active = true;
            else
                UpgradeUI["repair"].Active = false;

            if (temp[selectedUpgrade].Key == (Player.ActiveSlot == Slots.Slot1 ? Player.Slot1 : Player.Slot2))
            {
                UpgradeUI["description"].Color = HIGHLIGHT_COLOR;
                UpgradeUI["buy"].Active = false;
            }
            else
            {
                UpgradeUI["description"].Color = NORMAL_COLOR;
                UpgradeUI["buy"].Active = true;
            }

            if (temp[selectedUpgrade].Value.OreCost > Player.StoredOre)
                UpgradeUI["cost"].Color = Color.Red;
            else
                UpgradeUI["cost"].Color = Color.White;

        }

        //MENU UI - START
        void start_MouseAway(object sender, EventArgs e)
        {
            MenuUI["start"].Color = NORMAL_COLOR;
            MenuUI["start"].Scale = NORMAL_SCALE;
        }

        void start_MouseOver(object sender, EventArgs e)
        {
            MenuUI["start"].Color = HIGHLIGHT_COLOR;
            MenuUI["start"].Scale = HIGHLIGHT_SCALE;
        }

        void start_OnClick(UIBase sender, MouseClickArgs e)
        {
            ContentHandler.StopAll();

            SwitchGameState(last);
        }

        //MENU UI - EXIT
        void exit_MouseAway(object sender, EventArgs e)
        {
            MenuUI["exit"].Color = NORMAL_COLOR;
            MenuUI["exit"].Scale = NORMAL_SCALE;
        }

        void exit_MouseOver(object sender, EventArgs e)
        {
            MenuUI["exit"].Color = HIGHLIGHT_COLOR;
            MenuUI["exit"].Scale = HIGHLIGHT_SCALE;
        }

        void exit_OnClick(UIBase sender, MouseClickArgs e)
        {
            if(ContentHandler.ShouldPlaySFX)
            {
                SoundEffectInstance tempClick = ContentHandler.SFX["click"].CreateInstance();
                tempClick.Play();
                while (tempClick.State == SoundState.Playing) ; //wait until click is done playing
            }
            this.Exit(); //then exit
        }

        //UPGRADE UI - CONTINUE
        void continue_OnClick(UIBase sender, MouseClickArgs e)
        {
            Player.ActiveSlot = Slots.Slot1;

            ContentHandler.PlaySFX("switch");

            SwitchGameState(GameState.Game);
        }

        void continue_MouseAway(object sender, EventArgs e)
        {
            UpgradeUI["continue"].Color = NORMAL_COLOR;
            UpgradeUI["continue"].Scale = NORMAL_SCALE;
        }

        void continue_MouseOver(object sender, EventArgs e)
        {
            UpgradeUI["continue"].Color = HIGHLIGHT_COLOR;
            UpgradeUI["continue"].Scale = HIGHLIGHT_SCALE;
        }


        //UPGRADE UI - SLOTS
        void slot2_OnClick(UIBase sender, MouseClickArgs e)
        {
            UpgradeUI["slot2"].Color = Color.Red;
            UpgradeUI["slot2"].Scale = HIGHLIGHT_SCALE;
            Player.ActiveSlot = Slots.Slot2;
            UpgradeUI["slot1"].Color = NORMAL_COLOR;
            UpgradeUI["slot1"].Scale = NORMAL_SCALE;

            ContentHandler.PlaySFX("switch");
        }

        void slot1_OnClick(UIBase sender, MouseClickArgs e)
        {
            UpgradeUI["slot1"].Color = Color.Red;
            UpgradeUI["slot1"].Scale = HIGHLIGHT_SCALE;
            Player.ActiveSlot = Slots.Slot1;
            UpgradeUI["slot2"].Color = NORMAL_COLOR;
            UpgradeUI["slot2"].Scale = NORMAL_SCALE;

            ContentHandler.PlaySFX("switch");
        }

        void slot2_MouseOver(object sender, EventArgs e)
        {
            UpgradeUI["slot2"].Color = HIGHLIGHT_COLOR;
        }

        void slot1_MouseOver(object sender, EventArgs e)
        {
            UpgradeUI["slot1"].Color = HIGHLIGHT_COLOR;
        }

        void slot1_MouseAway(object sender, EventArgs e)
        {
            if (Player.ActiveSlot == Slots.Slot1)
                UpgradeUI["slot1"].Color = Color.Red;
            else
                UpgradeUI["slot1"].Color = NORMAL_COLOR;
        }

        void slot2_MouseAway(object sender, EventArgs e)
        {
            if (Player.ActiveSlot == Slots.Slot2)
                UpgradeUI["slot2"].Color = Color.Red;
            else
                UpgradeUI["slot2"].Color = NORMAL_COLOR;
        }

        //UPGRADE UI - EXIT TO MENU
        void exitToMenu_MouseAway(object sender, EventArgs e)
        {
            UpgradeUI["exitToMenu"].Color = NORMAL_COLOR;
            UpgradeUI["exitToMenu"].Scale = NORMAL_SCALE;
        }

        void exitToMenu_MouseOver(object sender, EventArgs e)
        {
            UpgradeUI["exitToMenu"].Color = HIGHLIGHT_COLOR;
            UpgradeUI["exitToMenu"].Scale = 1.2f;
        }

        void exitToMenu_OnClick(UIBase sender, MouseClickArgs e)
        {
            ContentHandler.PlaySFX("switch");

            SwitchGameState(GameState.Menu);
        }

        //UPGRADE UI - NEXT / BACK / BUY
        void next_MouseAway(object sender, EventArgs e)
        {
            UpgradeUI["next"].Color = NORMAL_COLOR;
            UpgradeUI["next"].Scale = NORMAL_SCALE;
        }

        void next_MouseOver(object sender, EventArgs e)
        {
            UpgradeUI["next"].Color = HIGHLIGHT_COLOR;
            UpgradeUI["next"].Scale = HIGHLIGHT_SCALE;
        }

        void back_MouseAway(object sender, EventArgs e)
        {
            UpgradeUI["back"].Color = NORMAL_COLOR;
            UpgradeUI["back"].Scale = NORMAL_SCALE;
        }

        void back_MouseOver(object sender, EventArgs e)
        {
            UpgradeUI["back"].Color = HIGHLIGHT_COLOR;
            UpgradeUI["back"].Scale = HIGHLIGHT_SCALE;
        }

        void buy_MouseAway(object sender, EventArgs e)
        {
            UpgradeUI["buy"].Color = NORMAL_COLOR;
            UpgradeUI["buy"].Scale = NORMAL_SCALE;
        }

        void buy_MouseOver(object sender, EventArgs e)
        {
            UpgradeUI["buy"].Color = HIGHLIGHT_COLOR;
            UpgradeUI["buy"].Scale = HIGHLIGHT_SCALE;
        }

        void next_OnClick(UIBase sender, MouseClickArgs e)
        {
            selectedUpgrade++;
            selectedUpgrade %= Player.GetEquipmentList.Count;
            updateSelected();

            ContentHandler.PlaySFX("switch");
        }

        void back_OnClick(UIBase sender, MouseClickArgs e)
        {
            selectedUpgrade--;
            if (selectedUpgrade < 0)
                selectedUpgrade = Player.GetEquipmentList.Count - 1;
            updateSelected();

            ContentHandler.PlaySFX("switch");
        }

        void buy_OnClick(UIBase sender, MouseClickArgs e)
        {
            var temp = Player.GetEquipmentList;

            if (Player.StoredOre >= temp[selectedUpgrade].Value.OreCost)
            {
                switch (Player.ActiveSlot)
                {
                    case Slots.Slot1:
                        Player.Slot1 = temp[selectedUpgrade].Key;
                        Player.StoredOre -= temp[selectedUpgrade].Value.OreCost;
                        updateSlots(sender, EventArgs.Empty);
                        break;
                    case Slots.Slot2:
                        Player.Slot2 = temp[selectedUpgrade].Key;
                        Player.StoredOre -= temp[selectedUpgrade].Value.OreCost;
                        updateSlots(sender, EventArgs.Empty);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                ContentHandler.PlaySFX("switch");
            }
            else
                ContentHandler.PlaySFX("error");
        }

        void help_OnClick(UIBase sender, MouseClickArgs e)
        {
            help_MouseAway(null, EventArgs.Empty);
            ContentHandler.PlaySFX("switch");
            SwitchGameState(GameState.Help);
        }

        void help_MouseOver(object sender, EventArgs e)
        {
            MenuUI["help"].Color = HIGHLIGHT_COLOR;
            MenuUI["help"].Scale = HIGHLIGHT_SCALE;
        }

        void help_MouseAway(object sender, EventArgs e)
        {
            MenuUI["help"].Color = NORMAL_COLOR;
            MenuUI["help"].Scale = NORMAL_SCALE;
        }


        void return_OnClick(UIBase sender, MouseClickArgs e)
        {
            ContentHandler.PlaySFX("switch");
            return_MouseAway(null, EventArgs.Empty);
            SwitchGameState(GameState.Menu);
        }

        void return_MouseOver(object sender, EventArgs e)
        {
            HelpUI["return"].Color = HIGHLIGHT_COLOR;
            HelpUI["return"].Scale = HIGHLIGHT_SCALE;
        }

        void return_MouseAway(object sender, EventArgs e)
        {
            HelpUI["return"].Color = NORMAL_COLOR;
            HelpUI["return"].Scale = NORMAL_SCALE;
        }

        void repair_MouseOver(object sender, EventArgs e)
        {
            UpgradeUI["repair"].Color = HIGHLIGHT_COLOR;
            UpgradeUI["repair"].Scale = HIGHLIGHT_SCALE;
        }

        void repair_MouseAway(object sender, EventArgs e)
        {
            UpgradeUI["repair"].Color = NORMAL_COLOR;
            UpgradeUI["repair"].Scale = NORMAL_SCALE;
        }

        void repair_OnClick(UIBase sender, MouseClickArgs e)
        {
            if (Player.RepairCost <= Player.StoredOre)
            {
                Player.StoredOre -= Player.RepairCost;
                Player.Health = Player.STARTING_HEALTH;
                ContentHandler.PlaySFX("click");
            }
            else
                ContentHandler.PlaySFX("error");

            updateSelected();
        }

        #endregion
    }

    public class StateArgs : EventArgs
    {
        public GameState StartingState;
        public GameState TargetState;

        public StateArgs(GameState startState, GameState targetState)
        {
            StartingState = startState;
            TargetState = targetState;
        }
    }

    public enum GameState
    {
        Game,
        Menu,
        Upgrade,
        Help
    }
}
