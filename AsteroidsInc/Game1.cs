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
    public partial class Game1 : Microsoft.Xna.Framework.Game
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
        const int DEFAULT_WINDOWED_HEIGHT = 720;

        const int WORLD_SIZE = 3000;

        //UI constants
        readonly Color HIGHLIGHT_COLOR = Color.Yellow;
        readonly Color NORMAL_COLOR = Color.White;
        const float HIGHLIGHT_SCALE = 1f;
        const float NORMAL_SCALE = 1f;

        int selectedUpgrade = 0;
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
            ContentHandler.Textures.Add(NPCManager.MINE_KEY + NPCManager.SHIELD_TEXTURE, Content.Load<Texture2D>(TEXTURE_DIR + NPCManager.MINE_KEY + NPCManager.SHIELD_TEXTURE));
            ContentHandler.Textures.Add(NPCManager.DRONE_KEY + NPCManager.SHIELD_TEXTURE, Content.Load<Texture2D>(TEXTURE_DIR + NPCManager.DRONE_KEY + NPCManager.SHIELD_TEXTURE));
            ContentHandler.Textures.Add(NPCManager.FIGHTER_KEY + NPCManager.SHIELD_TEXTURE, Content.Load<Texture2D>(TEXTURE_DIR + NPCManager.FIGHTER_KEY + NPCManager.SHIELD_TEXTURE));
            ContentHandler.Textures.Add(NPCManager.BOMBER_KEY + NPCManager.SHIELD_TEXTURE, Content.Load<Texture2D>(TEXTURE_DIR + NPCManager.BOMBER_KEY + NPCManager.SHIELD_TEXTURE));

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

            ContentHandler.SFX.Add(NPCManager.MINE_KEY + NPCManager.DAMAGE_POSTFIX, Content.Load<SoundEffect>(SOUND_DIR + Player.IMPACT_SFX)); //TODO: Add proper SFX
            ContentHandler.SFX.Add(NPCManager.DRONE_KEY + NPCManager.DAMAGE_POSTFIX, Content.Load<SoundEffect>(SOUND_DIR + Player.IMPACT_SFX));
            ContentHandler.SFX.Add(NPCManager.FIGHTER_KEY + NPCManager.DAMAGE_POSTFIX, Content.Load<SoundEffect>(SOUND_DIR + Player.IMPACT_SFX));
            ContentHandler.SFX.Add(NPCManager.BOMBER_KEY + NPCManager.DAMAGE_POSTFIX, Content.Load<SoundEffect>(SOUND_DIR + Player.IMPACT_SFX));

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

            //UI
            init_UI();
            add_events();

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
            ContentHandler.Update(gameTime); //same with ContentHandler
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
            ((UIString<string>)GameUI["health"]).Value = "Hull Integrity: " + //clamp the health display to min/max, so no -x health
                MathHelper.Clamp(Player.Health, Player.MIN_HEALTH, Player.MAX_HEALTH).ToString();
            if (Player.Health > 66) //change color of health display based on current status
                GameUI["health"].Color = Color.Green;
            else if (Player.Health > 33 && Player.Health <= 66)
                GameUI["health"].Color = Color.Yellow;
            else
            {
                GameUI["health"].Color = Color.Red;
                GameUI["warning"].Active = true;

                if (Player.Health <= 10) //and if health is really low, play a looping warning sound
                    ContentHandler.PlaySFX("alarm");
                else
                    ContentHandler.StopInstancedSFX("alarm");
            }

            //check for all ore collected
            if (Player.CurrentOre >= LevelManager.CurrentLevel.CollectableOre)
            {
                GameUI["orecollected"].Active = true;
                ContentHandler.PlayOnceSFX("win"); //play a win sfx
            }

            //get health / set color
            ((UIString<int>)GameUI["oreCount"]).Value = Player.CurrentOre;
            //get ore count
            ((UIString<string>)GameUI["sector"]).Value = "Sector: " + 
                (LevelManager.Counter + 1).ToString() + " -" + LevelManager.CurrentLevel.Description;

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
            AsteroidManager.RegenerateAsteroids = false; //don't regenerate asteroids for this level
            ContentHandler.PlayOnceSFX(Player.WIN_SFX);
            GameUI["levelcomplete"].Active = true;

            if (InputHandler.IsKeyDown(Keys.Escape))
            {
                //reset timing flags and GameUI elements to previous state
                AsteroidManager.RegenerateAsteroids = true;
                GameUI["warning"].Active = false;
                GameUI["levelcomplete"].Active = false;
                GameUI["orecollected"].Active = false;
                LevelManager.NextLevel();
                SwitchGameState(GameState.Upgrade);
            }
        }

        //On player death
        void Player_DeadEvent(object sender, EventArgs e)
        {
            GameUI["warning"].Active = false;
            SwitchGameState(GameState.Menu); //switch the state
            ((UIString<string>)MenuUI["start"]).Value = "Start"; //player has died, reset the resume button

            LevelManager.Restart();
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
