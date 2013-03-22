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
    public class UIString<T> : UIBase where T : IConvertible
    {
        public T Value; //main value
        public SpriteFont Font { get; set; }

        public Vector2 StringLength
        {
            get { return Font.MeasureString(Value.ToString()); }
        }

        public UIString(
            T initialValue,
            Vector2 relativePosition,
            Vector2 origin,
            SpriteFont font,
            Color color,
            bool active = false,
            float scale = 1f,
            float rotation = 0f,
            SpriteEffects effects = SpriteEffects.None)
            : base(relativePosition, origin, color, active, scale, rotation, effects)
        {
            Value = initialValue;
            Font = font;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Active)
                spriteBatch.DrawString(
                    Font,
                    Value.ToString(),
                    ScreenPosition,
                    Color,
                    Rotation,
                    Origin,
                    Scale,
                    Effects,
                    UILayerDepth);

        }
    }
}
