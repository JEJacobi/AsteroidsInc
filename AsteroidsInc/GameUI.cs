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
        #region UI Initialization

        void init_UI() //initialize UI components and add to element dictionaries
        {
            //GAME UI
            GameUI.Add("oreCount", new UIString<int>(0, new Vector2(0.057f, 0.02f), ContentHandler.Fonts["lcd"], Color.White, true, 1f, 0f, false));
            GameUI.Add("health", new UIString<string>("Hull Integrity: 100", new Vector2(0.005f, 0.95f), ContentHandler.Fonts["lcd"], Color.Green, true, 1f, 0f, false));
            GameUI.Add("oreSprite", new UISprite(ContentHandler.Textures["smallOre"], new Vector2(0.01f, 0.01f), Color.White, true, 1f, 0f, false));
            GameUI.Add("x", new UIString<string>("x", new Vector2(0.035f, 0.02f), ContentHandler.Fonts["lcd"], Color.White, true, 1f, 0f, false));
            GameUI.Add("warning", new UIString<string>("HULL INTEGRITY CRITICAL", new Vector2(0.5f, 0.95f), ContentHandler.Fonts["lcd"], Color.Red));
            GameUI.Add("sector", new UIString<string>("Sector: 1/8", new Vector2(0.5f, 0.03f), ContentHandler.Fonts["lcd"], Color.White, true));
            GameUI.Add("levelcomplete", new UIString<string>("Sector Clear - Press Escape to Exit", new Vector2(0.5f, 0.4f), ContentHandler.Fonts["lcd"], Color.White));
            GameUI.Add("pointer", new UISprite(ContentHandler.Textures["arrow"], new Vector2(0.8f, 0.9f), Color.White, true));
            GameUI.Add("distance", new UIString<int>(0, new Vector2(0.85f, 0.9f), ContentHandler.Fonts["lcd"], Color.White, true));

            //MENU UI
            MenuUI.Add("title", new UIString<string>("Asteroids", new Vector2(0.5f, 0.2f), ContentHandler.Fonts["title"], Color.White, true));
            MenuUI.Add("title2", new UIString<string>("INC", new Vector2(0.5f, 0.29f), ContentHandler.Fonts["title2"], Color.White, true));
            MenuUI.Add("start", new UIString<string>("Start", new Vector2(0.5f, 0.5f), ContentHandler.Fonts["menu"], Color.White, true, NORMAL_SCALE));
            MenuUI.Add("exit", new UIString<string>("Exit", new Vector2(0.5f, 0.65f), ContentHandler.Fonts["menu"], Color.White, true, NORMAL_SCALE));
            MenuUI.Add("sound", new UIString<string>("F11 - Toggle SFX", new Vector2(0.01f, 0.93f), ContentHandler.Fonts["lcd"], Color.White, true, 1f, 0f, false));
            MenuUI.Add("music", new UIString<string>("F12 - Toggle Music", new Vector2(0.01f, 0.96f), ContentHandler.Fonts["lcd"], Color.White, true, 1f, 0f, false));
            MenuUI.Add("fullscreen", new UIString<string>("F10 - Toggle Fullscreen", new Vector2(0.01f, 0.90f), ContentHandler.Fonts["lcd"], Color.White, true, 1f, 0f, false));
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
            HelpUI.Add("credit2", new UIString<string>("qubodup", new Vector2(0.33f, 0.65f), ContentHandler.Fonts["lcd"], Color.White, true));
            HelpUI.Add("credit3", new UIString<string>("HaelDB", new Vector2(0.33f, 0.7f), ContentHandler.Fonts["lcd"], Color.White, true));
            HelpUI.Add("credit4", new UIString<string>("wuhu", new Vector2(0.33f, 0.75f), ContentHandler.Fonts["lcd"], Color.White, true));
            HelpUI.Add("credit5", new UIString<string>("GameArtForge", new Vector2(0.33f, 0.8f), ContentHandler.Fonts["lcd"], Color.White, true));
            HelpUI.Add("credit6", new UIString<string>("Skorpio", new Vector2(0.33f, 0.85f), ContentHandler.Fonts["lcd"], Color.White, true));
            HelpUI.Add("credit7", new UIString<string>("DST", new Vector2(0.66f, 0.65f), ContentHandler.Fonts["lcd"], Color.White, true));
            HelpUI.Add("credit8", new UIString<string>("Clearside", new Vector2(0.66f, 0.7f), ContentHandler.Fonts["lcd"], Color.White, true));
            HelpUI.Add("credit9", new UIString<string>("Michel Baradari", new Vector2(0.66f, 0.75f), ContentHandler.Fonts["lcd"], Color.White, true));
            HelpUI.Add("credit10", new UIString<string>("ObsydianX", new Vector2(0.66f, 0.8f), ContentHandler.Fonts["lcd"], Color.White, true));
            HelpUI.Add("credit11", new UIString<string>("Dravenx", new Vector2(0.66f, 0.85f), ContentHandler.Fonts["lcd"], Color.White, true));

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
        }

        void add_events() //subscribe to UI events
        {
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
        }

        #endregion

        #region UI Event Handlers

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

        //HELP UI - ENTER/EXIT
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

        //UPGRADE UI - REPAIR
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
}
