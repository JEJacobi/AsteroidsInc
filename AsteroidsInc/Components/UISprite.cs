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
    class UISprite : UIBase
    {
        public Texture2D Texture; //Sprite part of UISprite

        public UISprite(
            Texture2D texture,
            Vector2 relativePosition,
            Color tintColor,
            bool active = false,
            float scale = 1f,
            float rotation = 0f,
            bool isCenterOrigin = true,
            SpriteEffects effects = SpriteEffects.None)
            : base(relativePosition, tintColor, active, scale, rotation, isCenterOrigin, effects)
        {
            Texture = texture;
        }

        //Override Methods
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Active) //if its active, draw
                spriteBatch.Draw(
                    Texture,
                    ScreenPosition,
                    null,
                    Color,
                    Rotation,
                    GetOrigin(),
                    Scale,
                    Effects,
                    UILAYERDEPTH);
        }
        public override Vector2 GetOrigin()
        {
            if (IsCenterOrigin)
                return new Vector2(Texture.Width / 2, Texture.Height / 2);
            else
                return Vector2.Zero;
        }
        public override Rectangle GetBoundingBox()
        {
            return new Rectangle(
                (int)ScreenPosition.X - (IsCenterOrigin ? (int)Texture.Width / 2 : 0),
                (int)ScreenPosition.Y - (IsCenterOrigin ? (int)Texture.Height / 2 : 0),
                Texture.Width,
                Texture.Height);
        }
    }
}
