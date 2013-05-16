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
        public SpriteFont Font { get; set; } //what font to draw the value in

        public Vector2 StringLength //the vector length of the specified string in the specified font
        {
            get { return Font.MeasureString(Value.ToString()); }
        }

        public UIString(
            T initialValue,
            Vector2 relativePosition,
            SpriteFont font,
            Color color,
            bool active = false,
            float scale = 1f,
            float rotation = 0f,
            bool isCenterOrigin = true,
            SpriteEffects effects = SpriteEffects.None,
            int xPadding = 0,
            int yPadding = 0)
            : base(relativePosition, color, active, scale, rotation, isCenterOrigin, effects, xPadding, yPadding)
        {
            Value = initialValue;
            Font = font;
        }

        //Methods
        public override void Draw(SpriteBatch spriteBatch)
        {
            //spriteBatch.Draw(
            //    ContentHandler.Textures["particle"],
            //    GetBoundingBox(),
            //    Color.White);

            if (Active)
                spriteBatch.DrawString(
                    Font,
                    Value.ToString(),
                    ScreenPosition,
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
                return new Vector2((int)Math.Round(StringLength.X / 2, 0), (int)Math.Round(StringLength.Y / 2, 0));
            else
                return Vector2.Zero;
        }
        public override Rectangle GetBoundingBox()
        {
            return new Rectangle(
                ((int)ScreenPosition.X - (IsCenterOrigin ? (int)StringLength.X / 2 : 0)) + XPadding, //woo, ternary!
                ((int)ScreenPosition.Y - (IsCenterOrigin ? (int)StringLength.Y / 2 : 0)) + YPadding,
                (int)StringLength.X - (XPadding * 2),
                (int)StringLength.Y - (XPadding * 2));
        }
        public override string ToString() //override tostring, gets converted value
        {
            return Value.ToString();
        }
    }
}
