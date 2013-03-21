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
    public abstract class SpriteBase
    {
        public Texture2D Texture { get; set; }
        public Vector2 Origin { get; set; }
        public Color TintColor { get; set; }
        public float Rotation { get; set; } //TODO: Add range check
        public float Scale { get; set; }
        public float Depth { get; set; }
        public SpriteEffects Effects { get; set; }

        public SpriteBase( //main constructor designed to be built on top of in derived classes
            Texture2D texture,
            Vector2 origin,
            Color tintColor,
            float rotation,
            float scale,
            float depth,
            SpriteEffects effects)
        {
            if (texture == null) { throw new NullReferenceException("Null texture reference."); }
            Texture = texture;
            Origin = origin;
            TintColor = tintColor; //assign parameters
            Rotation = rotation;
            Scale = scale;
            Depth = depth; 
            Effects = effects;
        }

        public abstract void Draw(SpriteBatch spriteBatch);
        public abstract void Update(GameTime gameTime);
    }
}