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
    class GameObject : SpriteBase
    {
        #region Declarations
        
        public Vector2 WorldLocation = Vector2.Zero; //Used as position
        public Vector2 Velocity = Vector2.Zero; //No velocity
        public int CollisionRadius = 0; //Radius for bounding circle collision
        public int BoundingXPadding = 0; 
        public int BoundingYPadding = 0; //Padding for bounding box collision

        #endregion

        #region Properties
        
        public Rectangle WorldRectangle //get rectangle in world coords with width of sprite
        {
            get
            {
                return new Rectangle(
                    (int)WorldLocation.X,
                    (int)WorldLocation.Y,
                    Texture.Width,  //TODO: Replace with spritesheet animation logic
                    Texture.Height);
            }
        }

        public Rectangle BoundingBox //get bounding box for use in collision detection
        {
            get
            {
                return new Rectangle( //Get bounding box with respect to padding values
                    (int)WorldLocation.X + BoundingXPadding, 
                    (int)WorldLocation.Y + BoundingYPadding,
                    Texture.Width - (BoundingXPadding * 2), //TODO: Replace with spritesheet animation logic
                    Texture.Height - (BoundingYPadding * 2));
            }
        }

        public Vector2 ScreenLocation
        { get { return Camera.GetLocalCoords(WorldLocation); } } //get screen coordinates

        public Rectangle ScreenRectangle
        { get { return Camera.GetLocalCoords(WorldRectangle); } } //get screen rectangle

        public Vector2 WorldCenter
        { get { return WorldLocation + SpriteCenter; } } //gets the center of the sprite in world coords

        public Vector2 ScreenCenter
        { get { return Camera.GetLocalCoords(WorldLocation + SpriteCenter); } } //returns the center in screen coords

        #endregion

        public GameObject( //main constructor, /w added optional parameters and call to SpriteBase init
            Texture2D texture,
            Vector2 worldLocation,
            Vector2 origin,
            Color tintColor,
            float rotation = 0f, //default to no rotation
            float scale = 1f, //default to 1:1 scale
            float depth = 0f, //default to 0 layerDepth
            Rectangle? sourceRect = null, //default to full texture
            int collisionRadius = 0, //collision radius used in bounding circle collision, default to 0 or no bounding circle
            int xPadding = 0, //amount of x padding, used in bounding box collision, default to 0, or no bounding box
            int yPadding = 0, //amount of y padding, used in bounding box collision, default to 0, or no bounding box
            SpriteEffects effects = SpriteEffects.None)
            : base(texture, origin, tintColor, rotation, scale, depth, sourceRect, effects)
        {
            WorldLocation = worldLocation; //assign position data
            BoundingXPadding = xPadding; BoundingYPadding = yPadding; CollisionRadius = collisionRadius; //assign collision data
        }

        #region Methods

        public override void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Camera.IsObjectVisible(WorldRectangle)) //check if sprite is visible to camera
            {
                spriteBatch.Draw(
                    Texture,
                    ScreenCenter, //position in local coords
                    SourceRectangle, //source rectangle for spritesheet animation
                    TintColor,
                    Rotation,
                    Origin,
                    Scale,
                    Effects, //spriteeffects
                    Depth); //layerdepth
            }
        } 

        public bool IsBoxColliding(Rectangle obj) //bounding box collision test
        {
            return BoundingBox.Intersects(obj);
        }

        public bool IsBoxColliding(GameObject obj) //overload of previous which takes a GameObject instead of a rectangle
        {
            if (BoundingBox.Intersects(obj.BoundingBox))
                return true;
            else
                return false;
        }

        public bool IsCircleColliding(Vector2 objCenter, float objRadius)
        {
            if (Vector2.Distance(WorldCenter, objCenter) <
                (CollisionRadius + objRadius))  //if the distance between centers is greater than the sum
                return true;                    //of the radii, collision has occurred
            else
                return false; //if not, return false
        }

        public bool IsCircleColliding(GameObject obj) //overload of previous which takes a GameObject instead of loose values
        {
            if (Vector2.Distance(this.WorldCenter, obj.WorldCenter) <
                (CollisionRadius + obj.CollisionRadius))
                return true;
            else
                return false;
        }

        public void RotateTo(Vector2 point) //rotates the GameObject to a point
        {
            Rotation = (float)Math.Atan2(point.Y, point.X);
        }

        #endregion
    }
}
