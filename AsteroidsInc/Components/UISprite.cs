﻿using System;
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

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Active)
                spriteBatch.Draw(
                    Texture,
                    ScreenPosition,
                    null,
                    Color,
                    Rotation,
                    GetOrigin(),
                    Scale,
                    Effects,
                    UILayerDepth);
        }

        public override Vector2 GetOrigin()
        {
            if (IsCenterOrigin)
                return new Vector2(Texture.Width / 2, Texture.Height / 2);
            else
                return Vector2.Zero;
        }
    }
}