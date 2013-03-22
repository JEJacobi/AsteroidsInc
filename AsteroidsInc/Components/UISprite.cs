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
        public Texture2D Texture;

        public UISprite(
            Texture2D texture,
            Vector2 relativePosition,
            Vector2 origin,
            Color tintColor,
            bool active = false,
            float scale = 1f,
            float rotation = 0f,
            SpriteEffects effects = SpriteEffects.None)
            : base(relativePosition, origin, tintColor, active, scale, rotation, effects)
        {
            Texture = texture;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Active)
                spriteBatch.Draw(
                    Texture,
                    ScreenPosition,
                    null,
                    Color,
                    Rotation,
                    Origin,
                    Scale,
                    Effects,
                    UILayerDepth);
        }
    }
}
