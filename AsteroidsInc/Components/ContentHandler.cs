using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace AsteroidsInc.Components
{
    public static class ContentHandler
    {
        public static Dictionary<string, Texture2D> Textures { get; set; } //Dictionary for textures
        public static Dictionary<string, SpriteFont> Fonts { get; set; } //Dictonary for SpriteFonts
        public static Dictionary<string, SoundEffect> SFX { get; set; } //Dictionary for SFX
        public static Dictionary<string, SoundEffectInstance> InstanceSFX { get; set; } //Dictionary for instanced SE
        public static Dictionary<string, Song> Songs { get; set; } //Dictionary for songs, played via the MediaPlayer

        public static bool ShouldPlaySFX { get; private set; } //essentially settings variables
        public static bool ShouldPlayMusic { get; private set; }

        static ContentHandler() //static constructor; initializes dictionaries
        {
            Textures = new Dictionary<string, Texture2D>();
            Fonts = new Dictionary<string, SpriteFont>();
            SFX = new Dictionary<string, SoundEffect>();
            InstanceSFX = new Dictionary<string, SoundEffectInstance>();
            Songs = new Dictionary<string, Song>();
            //init the dictionaries

            ShouldPlayMusic = true;
            ShouldPlaySFX = true;
            //and set the settings
        }

        #region Methods

        public static void PlaySong(string key)
        {
            if (ShouldPlayMusic)
                MediaPlayer.Play(Songs[key]);
        }

        public static void PlaySFX(string key)
        {
            if (ShouldPlaySFX)
            {
                try //hacky use of try/catch
                {
                    SFX[key].Play(); //try accessing at non-instanced static sfx dictionary
                }
                catch (Exception)
                {
                    InstanceSFX[key].Play(); //if not, try accessing the instanced dictionary
                }
            }
        }

        public static void StopInstancedSFX(string key)
        {
            InstanceSFX[key].Stop();
        }

        public static void PauseInstancedSFX(string key)
        {
            InstanceSFX[key].Pause();
        }

        public static void StopMusic()
        {
            MediaPlayer.Stop();
        }

        public static void StopSFX()
        {
            foreach (KeyValuePair<String, SoundEffectInstance> sfx in InstanceSFX)
                sfx.Value.Stop();
        }

        public static void StopAll()
        {
            StopSFX();
            StopMusic();
        }

        public static void TogglePlaySFX()
        {
            ShouldPlaySFX = !ShouldPlaySFX;

            if (ShouldPlaySFX == false)
                StopSFX();
        }

        public static void TogglePlayMusic()
        {
            ShouldPlayMusic = !ShouldPlayMusic;

            if (ShouldPlayMusic == false)
                StopMusic();
        }

        #endregion
    }
}
