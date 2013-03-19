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
    class Sprite //TODO: Add animation logic/methods
    {
        #region Declarations

        public Texture2D Texture;
        public Vector2 WorldLocation = Vector2.Zero; //Upper left world location
        public Vector2 Velocity = Vector2.Zero; //No velocity
        public Vector2 Origin = Vector2.Zero; //Default upper left origin
        public Rectangle? SourceRectangle = null; //Default null to use entire texture
        public Color TintColor = Color.White; //Default no tint
        public float Rotation = 0f; //Zero rotation
        public float Scale = 1f; //1:1 scale
        public float Depth = 0f; //Default depth
        public SpriteEffects Effects = SpriteEffects.None; //Default to no effects
        public int CollisionRadius = 0;
        public int BoundingXPadding = 0;
        public int BoundingYPadding = 0;

        #endregion

        #region Properties

        public Rectangle WorldRectangle
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

        public Rectangle BoundingBox
        {
            get
            {
                return new Rectangle(
                    (int)WorldLocation.X,
                    (int)WorldLocation.Y,
                    Texture.Width, //TODO: Replace with spritesheet animation logic
                    Texture.Height);
            }
        }

        public Vector2 ScreenLocation
        { get { return Camera.GetLocalCoords(WorldLocation); } } //get screen coordinates

        public Rectangle ScreenRectangle
        { get { return Camera.GetLocalCoords(WorldRectangle); } } //get screen rectangle

        public Vector2 SpriteCenter
        { get { return new Vector2(Texture.Width, Texture.Height); } } //get sprite's center

        public Vector2 WorldCenter
        { get { return WorldLocation + SpriteCenter; } } //gets the center of the sprite in world coords

        public Vector2 ScreenCenter
        { get { return Camera.GetLocalCoords(WorldLocation + SpriteCenter); } } //returns the center in screen coords

        #endregion

        #region Constructors

        public Sprite(Texture2D texture, Vector2 worldLocation) //Quick-n-dirty simple constructor
        {
            if (texture == null) { throw new NullReferenceException("Null texture reference."); } //check if texture is null
            Texture = texture; WorldLocation = worldLocation; //and assign
        }

        public Sprite( //Full constructor with optional parameters
            Texture2D texture,
            Vector2 worldLocation,
            Vector2 origin,
            Color tintColor,
            float rotation = 0f, //default to no rotation
            float scale = 1f, //default to 1:1 scale
            float depth = 0f, //default to 0 layerDepth
            Rectangle? sourceRect = null, //default to full texture
            int collisionRadius = 0, //collision radius used in bounding circle collision
            int xPadding = 0, //amount of x padding, used in bounding box collision
            int yPadding = 0, //amount of y padding, used in bounding box collision
            SpriteEffects effects = SpriteEffects.None) //default to no spriteeffects
        {
            if (texture == null) { throw new NullReferenceException("Null texture reference."); }
            Texture = texture; WorldLocation = worldLocation; //assign basics
            Origin = origin; TintColor = tintColor; //assign required params
            Rotation = rotation; Scale = scale; Depth = depth;
            BoundingXPadding = xPadding; BoundingYPadding = yPadding; CollisionRadius = collisionRadius;
            SourceRectangle = sourceRect; Effects = effects; //assign optional params
        }

        #endregion

        #region Methods

        public bool IsBoxColliding(Rectangle obj) //TODO: Add padding values
        {
            return BoundingBox.Intersects(obj);
        }

        public bool IsCircleColliding(Vector2 objCenter, float objRadius)
        {
            if (Vector2.Distance(WorldCenter, objCenter) <
                (CollisionRadius + objRadius))  //if the distance between centers is greater than the sum
                return true;                    //of the radii, collision has occurred
            else
                return false; //if not, return false
        }

        #endregion
    }
}
