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
        public static Dictionary<string, SoundEffect> Effects { get; set; } //Dictionary for SFX
        public static Dictionary<string, Song> Songs { get; set; } //Dictionary for songs, played via the MediaPlayer

        public static void PlaySong(string key)
        {
            MediaPlayer.Play(Songs[key]);
        }

        public static void StopMusic()
        {
            MediaPlayer.Stop();
        }

        public static void Initialize()
        {
            Textures = new Dictionary<string, Texture2D>();
            Fonts = new Dictionary<string, SpriteFont>();
            Effects = new Dictionary<string, SoundEffect>();
            Songs = new Dictionary<string, Song>();
        }
    }
}
