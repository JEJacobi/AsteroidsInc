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
        public bool Active { get; set; } //is being displayed
        public float Scale { get; set; } //scale of component
        public Color Color { get; set; } //Sprite/String color tint
        public bool IsCenterOrigin; //True = Center Origin, False = Upper Left
        public Vector2 RelativePos { get; set; } //Relative scale for position
        public Vector2 ScreenPosition
        {
            get
            {
                return new Vector2(
                    Camera.ScreenSize.X * RelativePos.X,
                    Camera.ScreenSize.Y * RelativePos.Y);
            }
            set
            {
                if (value.X == 0 || value.Y == 0)
                {
                    if (value.X != 0 && value.Y == 0)
                        RelativePos = new Vector2(Camera.ScreenSize.X / value.X, 0);

                    if (value.X == 0 && value.Y != 0)
                        RelativePos = new Vector2(0, Camera.ScreenSize.Y / value.Y);

                    if (value.X == 0 && value.Y == 0)
                        RelativePos = new Vector2(0, 0);
                }
                else
                {
                    RelativePos = new Vector2(
                        Camera.ScreenSize.X / value.X,
                        Camera.ScreenSize.Y / value.Y);
                }
            }
        } //position on the screen itself
        public float Rotation { get; set; } //in radians
        public SpriteEffects Effects;

        public const float UILAYERDEPTH = 0.3f;

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

        #region Methods

        public abstract void Draw(SpriteBatch spriteBatch); //Main draw method
        public virtual void Update(GameTime gameTime) //TODO: Test!
        {
            if (MouseOver != null && InputHandler.IsMouseOverObject(this.GetBoundingBox())) //if mouse is in the bounding box
                MouseOver(this, new EventArgs());

            if (MouseAway != null && InputHandler.HasLeftObject(this.GetBoundingBox())) //if mouse WAS in the bounding box
                MouseAway(this, new EventArgs());

            if (OnClick != null) //determine mouse button clicked and invoke event
            {
                MouseClickArgs e = new MouseClickArgs();

                if (InputHandler.IsClickingObject(this.GetBoundingBox(), MouseButtons.LeftMouseButton))
                    e.Button = MouseButtons.LeftMouseButton;

                if (InputHandler.IsClickingObject(this.GetBoundingBox(), MouseButtons.RightMouseButton))
                    e.Button = MouseButtons.RightMouseButton;

                if (InputHandler.IsClickingObject(this.GetBoundingBox(), MouseButtons.MiddleMouseButton))
                    e.Button = MouseButtons.MiddleMouseButton;

                if (e.Button != MouseButtons.None)
                    OnClick(this, e);
            }
        }
        public abstract Vector2 GetOrigin();
        public abstract Rectangle GetBoundingBox();

        #endregion

        #region Events

        public delegate void MouseClickHandler(UIBase sender, MouseClickArgs e);
        public event EventHandler MouseOver;
        public event EventHandler MouseAway;
        public event MouseClickHandler OnClick;

        #endregion
    }
}
