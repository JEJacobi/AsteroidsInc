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

namespace AsteroidsInc
{
    public abstract class SpriteBase //TODO: Add animation logic/methods
    {
        public Texture2D Texture;
        public Vector2 Origin = Vector2.Zero; //Default upper left origin
        public Rectangle? SourceRectangle = null; //Default null to use entire texture
        public Color TintColor = Color.White; //Default no tint
        public float Rotation = 0f; //Zero rotation
        public float Scale = 1f; //1:1 scale
        public float Depth = 0f; //Default depth
        public SpriteEffects Effects = SpriteEffects.None; //Default to no effects

        public Vector2 SpriteCenter //TODO: Add support for animation
        { get { return new Vector2(Texture.Width, Texture.Height); } } //Get this Sprite's center

        public SpriteBase( //main constructor designed to be built on top of in derived classes
            Texture2D texture,
            Vector2 origin,
            Color tintColor,
            float rotation,
            float scale,
            float depth,
            Rectangle? sourceRect,
            SpriteEffects effects)
        {
            if (texture == null) { throw new NullReferenceException("Null texture reference."); }
            Texture = texture;
            Origin = origin;
            TintColor = tintColor; //assign parameters
            Rotation = rotation;
            Scale = scale;
            Depth = depth; 
            SourceRectangle = sourceRect;
            Effects = effects;
        }

        public abstract void Draw(SpriteBatch spriteBatch);
        public abstract void Update(GameTime gameTime);
    }
}