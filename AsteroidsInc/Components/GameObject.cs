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
    public class GameObject
    {
        #region Declarations

        public Texture2D Texture { get; set; }
        public Vector2 Origin { get; set; }
        public Color TintColor { get; set; }
        public float Rotation { get; set; }
        public float RotationalVelocity { get; set; }
        public float Scale { get; set; }
        public float Depth { get; set; }
        public bool Active { get; set; }
        public SpriteEffects Effects { get; set; }

        public Vector2 WorldLocation { get; set; }
        public Vector2 Velocity { get; set; }

        public int CollisionRadius { get; set; } //Radius for bounding circle collision
        public int BoundingXPadding { get; set; }
        public int BoundingYPadding { get; set; } //Padding for bounding box collision

        public int TotalFrames
        {
            get //simple get
            { return totalFrames; }
            set //check if given totalFrames is in possible range
            {
                if (value <= (Rows * Columns))
                    totalFrames = value;
                else
                    throw new ArgumentOutOfRangeException();
            }
        } //Used in spritesheet animation
        private int totalFrames;

        public int CurrentFrame
        {
            get { return currentFrame; }
            set
            {
                currentFrame = (int)MathHelper.Clamp(value, 0, totalFrames);
            }
        }
        private int currentFrame;

        public int Rows { get; set; }
        public int Columns { get; set; }
        public bool Animating { get; set; }

        public const float VELOCITYSCALAR = 0.166667f; //Default to around 60fps standard movement

        #endregion

        #region Properties

        public int GetWidth { get { return Texture.Width / Columns; } } //Width of a frame
        public int GetHeight { get { return Texture.Height / Rows; } } //Height of a frame
        public int GetRow { get { return (int)((float)CurrentFrame / (float)Columns); } } //Current row
        public int GetColumn { get { return CurrentFrame % Columns; } } //Current column

        public Vector2 SpriteCenter
        { get { return new Vector2(GetWidth / 2, GetHeight / 2); } } //Get this Sprite's center
        
        public Rectangle WorldRectangle //get rectangle in world coords with width of sprite
        {
            get
            {
                return new Rectangle(
                    (int)WorldLocation.X,
                    (int)WorldLocation.Y,
                    GetWidth,
                    GetHeight);
            }
        }

        public Rectangle BoundingBox //get bounding box for use in collision detection
        {
            get
            {
                return new Rectangle( //Get bounding box with respect to padding values
                    (int)WorldLocation.X + BoundingXPadding, 
                    (int)WorldLocation.Y + BoundingYPadding,
                    GetWidth - (BoundingXPadding * 2),
                    GetHeight - (BoundingYPadding * 2));
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
            Vector2 velocity,
            Color tintColor,
            bool animating = false,
            float rotation = 0f, //default to no rotation
            float rotationalVelocity = 0f,
            float scale = 1f, //default to 1:1 scale
            float depth = 0f, //default to 0 layerDepth
            int collisionRadius = 0, //collision radius used in bounding circle collision, default to 0 or no bounding circle
            int xPadding = 0, //amount of x padding, used in bounding box collision, default to 0, or no bounding box
            int yPadding = 0, //amount of y padding, used in bounding box collision, default to 0, or no bounding box
            SpriteEffects effects = SpriteEffects.None,
            int totalFrames = 0,
            int rows = 1,
            int columns = 1)

        {
            if (texture == null) { throw new NullReferenceException("Null texture reference."); }
            Texture = texture; //assign parameters
            WorldLocation = worldLocation; 
            TintColor = tintColor; 
            Rotation = rotation;
            RotationalVelocity = rotationalVelocity;
            Scale = scale;
            Depth = depth;
            Effects = effects;
            Velocity = velocity;
            Animating = animating;
            Active = true;

            BoundingXPadding = xPadding; BoundingYPadding = yPadding; CollisionRadius = collisionRadius; //assign collision data
            Rows = rows; Columns = columns; this.TotalFrames = totalFrames; //assign animation data

            Origin = SpriteCenter; //assign origin to the center of a frame
        }

        #region Methods

        public virtual void Update(GameTime gameTime)
        {
            if (Active) //if object is active
            {
                WorldLocation += Vector2.Multiply(Velocity, (gameTime.ElapsedGameTime.Milliseconds / VELOCITYSCALAR));
                Rotation += RotationalVelocity; //Rotate according to the velocity
                //Move by Velocity times a roughly 60FPS scalar

                if (TotalFrames > 1 && Animating)
                {
                    CurrentFrame++;
                    if (CurrentFrame >= TotalFrames)
                        CurrentFrame = 0; //Loop animation
                }

                if (Camera.LOOPWORLD && !Camera.IsObjectInWorld(this.WorldRectangle)) //if world is looping and the object is out of bounds
                {
                    Vector2 temp = WorldLocation; //temporary Vector2 used for updated position

                    //X-Axis Component
                    if (WorldLocation.X > Camera.WorldRectangle.Width)
                        temp.X = Camera.WorldRectangle.X; //If X is out of bounds to the right, move X to the left side
                    if (WorldLocation.X < WorldLocation.X)
                        temp.X = Camera.WorldRectangle.Width; //If X is out of bound to the left, move X to the right side

                    //Y-Axis Component
                    if (WorldLocation.Y > Camera.WorldRectangle.Height)
                        temp.Y = Camera.WorldRectangle.Y; //If Y is out of bounds to the bottom, move Y to the top
                    if (WorldLocation.Y < Camera.WorldRectangle.Y)
                        temp.Y = Camera.WorldRectangle.Height; //If Y is out of bounds to the top, move Y to the bottom

                    WorldLocation = temp; //Assign updated position
                    Velocity = Vector2.Negate(Velocity); //Negate the velocity
                }
                else if (!Camera.LOOPWORLD && !Camera.IsObjectInWorld(this.WorldRectangle))
                {
                    Active = false; //if the object is outside the world but the LOOPWORLD constant is false, set inactive
                }
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (Active)
            {
                if (TotalFrames > 1 && Camera.IsObjectVisible(WorldRectangle)) //if multi-frame animation & object is visible
                {
                    Rectangle sourceRectangle = new Rectangle(GetWidth * GetColumn,
                        GetHeight * GetRow, GetWidth, GetHeight); //get source rectangle to use

                    spriteBatch.Draw(
                        Texture,
                        ScreenCenter,
                        sourceRectangle, //use generated source rectangle
                        TintColor,
                        Rotation,
                        Origin,
                        Scale,
                        Effects,
                        Depth);
                }
                else //if single frame sprite
                {
                    if (Camera.IsObjectVisible(WorldRectangle)) //check if sprite is visible to camera
                    {
                        spriteBatch.Draw(
                            Texture,
                            ScreenCenter, //center of the sprite in local coords
                            null, //full sprite
                            TintColor,
                            Rotation,
                            Origin,
                            Scale,
                            Effects, //spriteeffects
                            Depth); //layerdepth
                    }
                }
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

        protected Vector2 rotationToVector()
        {
            return Rotation.RotationToVectorFloat();
        } //local version of above

        #endregion
    }
}
