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
    public abstract class UIBase
    {
        public bool Active { get; set; }
        public float Scale { get; set; }
        public Color Color { get; set; }
        public bool IsCenterOrigin;
        public Vector2 RelativePos { get; set; }
        public Vector2 ScreenPosition
        {
            get //TODO: Test
            {
                return new Vector2(
                    Camera.ScreenSize.X * RelativePos.X,
                    Camera.ScreenSize.Y * RelativePos.Y);
            }
        }
        public float Rotation { get; set; }
        public SpriteEffects Effects;

        public const float UILayerDepth = 1f;

        public UIBase(
            Vector2 relativePos,
            Color color,
            bool active,
            float scale,
            float rotation,
            bool isCenterOrigin,
            SpriteEffects effects)
        {
            RelativePos = relativePos;
            Color = color;
            Active = active;
            Scale = scale;
            Rotation = rotation;
            IsCenterOrigin = isCenterOrigin;
            Effects = effects;
        }

        public abstract void Draw(SpriteBatch spriteBatch);
        public abstract Vector2 GetOrigin();
    }
}
