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

        public Texture2D Texture //what to draw
        {
            get { return text; }
            set
            {
                text = value; //whenever a new texture is assigned, recalculate the texture data
                RecalcTextureData();
            }
        }
        Texture2D text;

        public Vector2 Origin; //where to rotate from
        public Color TintColor; //what to tint the texture with

        public float Rotation
        {
            get { return rotation; } //mod by 2pi in case of direction overflow
            set
            {
                rotation = value % MathHelper.TwoPi;
                if (rotation < 0)
                    rotation += MathHelper.TwoPi; //if going into negative, overflow to equal positive position
            }
        }
        float rotation;

        public float RotationalVelocity
        {
            get { return rotationalVelocity % MathHelper.TwoPi; } //mod by 2pi in case of direction overflow
            set { rotationalVelocity = value % MathHelper.TwoPi; }
        }
        float rotationalVelocity;

        public float Scale = 1f; //draw scale
        public float Depth = 0f; //draw depth
        public bool Active = true; //if active; draw and update. if not, don't do anything
        public SpriteEffects Effects = SpriteEffects.None; //any sprite effects

        public Vector2 WorldLocation; //the world position
        public Vector2 Velocity; //velocity of the object, measured with a vector

        public Circle BoundingCircle;
        public int BoundingXPadding = 0;
        public int BoundingYPadding = 0; //Padding for bounding box collision

        //if true, omit looping if it goes off the edge, don't do minor functions, general optimization...
        public bool LiteMode = false;

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

        public int StartFrame //frame to reset the loop to
        {
            get
            { return startFrame; }
            set
            {
                if (value <= (Rows * Columns))
                    startFrame = value;
                else
                    throw new ArgumentOutOfRangeException();
            }
        }
        int startFrame;

        public int CurrentFrame //current frame in the animation loop
        {
            get { return currentFrame; }
            set
            {
                currentFrame = (int)MathHelper.Clamp(value, 0, totalFrames);
            }
        }
        private int currentFrame;

        public float FrameDelay = 0.16f;
        float delayCounter = 0f;

        public int Rows = 1; //rows in the sprite sheet, 1 is default
        public int Columns = 1; //columns in the sprite sheet, 1 is also default
        public bool Animating = false;

        public float RotationDegrees
        {
            get { return MathHelper.ToDegrees(Rotation); }
            set { Rotation = MathHelper.ToRadians(value); }
        }
        public float RotationVelocityDegrees
        {
            get { return MathHelper.ToDegrees(RotationalVelocity); }
            set { RotationalVelocity = MathHelper.ToRadians(value); }
        }

        public const float VELOCITYSCALAR = 1.0f / 60.0f; //Default to 60fps standard movement

        #endregion

        #region Properties

        public int GetWidth; //Width of a frame
        public int GetHeight; //Height of a frame
        public int GetRow { get { return (int)((float)CurrentFrame / (float)Columns); } } //Current row
        public int GetColumn { get { return CurrentFrame % Columns; } } //Current column
        public Vector2 SpriteCenter { get; set; } //Get this Sprite's center
        
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

        public Rectangle BoundingBox //get the non axis-aligned bounding box
        {
            get
            {
                int max = Math.Max(GetWidth, GetHeight); //get the dominant length

                return new Rectangle( //Get bounding box with respect to padding values
                    ((int)WorldCenter.X - (max >> 1)) + BoundingXPadding, //bitwise divide by 2
                    ((int)WorldCenter.Y - (max >> 1)) + BoundingYPadding,
                    max - (BoundingXPadding * 2), //essentially a worst case scenario bb, just in case of any rotation
                    max - (BoundingYPadding * 2));
            }
        }

        public Vector2 ScreenLocation
        { get { return Camera.GetLocalCoords(WorldLocation); } } //get screen coordinates

        public Rectangle ScreenRectangle
        { get { return Camera.GetLocalCoords(WorldRectangle); } } //get screen rectangle

        public Vector2 WorldCenter
        { 
            get { return WorldLocation + SpriteCenter; }
            set { WorldLocation = value - SpriteCenter; }
        } //gets/sets the center of the sprite in world coords

        public Vector2 DrawCoordinates
        { get { return ScreenCenter; } }

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
            bool liteMode = false, //handle world-looping and other
            int collisionRadius = 0, //collision radius used in bounding circle collision, default to 0 or no bounding circle
            int xPadding = 0, //amount of x padding, used in bounding box collision, default to 0, or no bounding box
            int yPadding = 0, //amount of y padding, used in bounding box collision, default to 0, or no bounding box
            SpriteEffects effects = SpriteEffects.None,
            int totalFrames = 0,
            int rows = 1,
            int columns = 1,
            int startingFrame = 0,
            float frameDelay = 0)

        {
            if (texture == null) { throw new ArgumentNullException("Null texture reference."); }
            //assign parameters
            TintColor = tintColor; 
            Rotation = rotation;
            RotationalVelocity = rotationalVelocity;
            Scale = scale;
            Depth = depth;
            Effects = effects;
            Velocity = velocity;
            Animating = animating;
            Active = true;

            BoundingCircle = new Circle(WorldCenter, collisionRadius);
            BoundingXPadding = xPadding; BoundingYPadding = yPadding; //assign collision data
            Rows = rows; Columns = columns; this.TotalFrames = totalFrames; StartFrame = startFrame; FrameDelay = frameDelay; //assign animation data

            Texture = texture; //texture assignment needs to be below row/column

            WorldCenter = worldLocation; //NEEDS TO BE BELOW ROW & COLUMN ASSIGNMENTS

            Origin = SpriteCenter; //assign origin to the center of a frame
        }

        #region Methods

        public virtual void Update(GameTime gameTime) //main update method, virtual for future inheritance
        {
            if (Active) //if object is active
            {
                if (Velocity != Vector2.Zero)
                {
                    WorldLocation += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    BoundingCircle.Position = WorldCenter;
                }
                if(RotationalVelocity != 0)
                    Rotation += RotationalVelocity;

                if (TotalFrames > 1 && Animating) //if the gameobject is animated
                {
                    delayCounter += (float)gameTime.ElapsedGameTime.TotalMilliseconds; //add the elapsed time to the delay counter
                    if (delayCounter >= FrameDelay) //if frame delay met
                    {
                        delayCounter = 0; //reset
                        CurrentFrame++; //increment the current frame
                    }

                    if (CurrentFrame >= TotalFrames)
                        CurrentFrame = StartFrame; //Loop animation
                }

                if (LiteMode == false && Camera.IsObjectInWorld(this.WorldRectangle) == false)
                {
                    if (Camera.LOOPWORLD) //if world is looping and the object is out of bounds
                    {
                        Vector2 temp = WorldCenter; //temporary Vector2 used for updated position

                        //X-Axis Component
                        if (WorldCenter.X > Camera.WorldRectangle.Width)
                            temp.X = Camera.WorldRectangle.X - (GetWidth >> 1); //If X is out of bounds to the right, move X to the left side
                        if (WorldCenter.X < Camera.WorldRectangle.X)
                            temp.X = Camera.WorldRectangle.Width + (GetWidth >> 1); //If X is out of bound to the left, move X to the right side

                        //Y-Axis Component
                        if (WorldCenter.Y > Camera.WorldRectangle.Height)
                            temp.Y = Camera.WorldRectangle.Y - (GetHeight >> 1); //If Y is out of bounds to the bottom, move Y to the top
                        if (WorldCenter.Y < Camera.WorldRectangle.Y)
                            temp.Y = Camera.WorldRectangle.Height + (GetHeight >> 1); //If Y is out of bounds to the top, move Y to the bottom

                        WorldCenter = temp; //Assign updated position
                    }
                    if (LiteMode || Camera.LOOPWORLD == false)
                    {
                        Active = false; //if the object is outside the world but the LOOPWORLD constant is false, set inactive
                    }
                }
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch) //main draw method, virtual also
        {
            if (Active) //if the sprite is active...
            {
                if (Camera.IsObjectVisible(WorldRectangle))
	            {
                    Rectangle? sourceRectangle;

                    if (TotalFrames > 1) //if multi-frame animation & object is visible
                        sourceRectangle = new Rectangle
                            (GetWidth * GetColumn,
                            GetHeight * GetRow,
                            GetWidth, GetHeight); //get source rectangle to use
                    else
                        sourceRectangle = null; //assign null if not animated

                    spriteBatch.Draw( //draw it
                        Texture,
                        DrawCoordinates,
                        sourceRectangle, //use generated source rectangle
                        TintColor,
                        Rotation,
                        Origin,
                        Scale,
                        Effects,
                        Depth);
	            }
            }

            //spriteBatch.Draw(
            //    ContentHandler.Textures["particle"],
            //    Camera.GetLocalCoords(BoundingBox),
            //    Color.White); //debug
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

        public bool IsCircleColliding(GameObject obj) //simple bounding circle collision detection check
        {
            if (BoundingCircle.IsColliding(obj.BoundingCircle))
                return true;
            else
                return false;
        }

        public bool IsCircleAndBoxColliding(GameObject obj) //tests for both bb and bc collisions with another game object
        {
            if (IsBoxColliding(obj) && IsCircleColliding(obj))
                return true;
            return false;
        }

        public void RotateTo(Vector2 point) //rotates the GameObject to a point
        {
            Rotation = point.RotateTo();
        }

        public void VectorTrack(Vector2 targetVector, float velTrackSpeed = 0.8f, float stabilization = 0.3f) //tracks the rotation to a vector target
        {
            float target = Vector2.Normalize(targetVector).RotateTo(); //get the target angle from this projectile's velocity
            target = MathHelper.ToDegrees(target);

            Util.VerifyAngle(ref target);

            if (target < RotationDegrees - 10)
                RotationVelocityDegrees -= velTrackSpeed;
            if (target > RotationDegrees + 10)
                RotationVelocityDegrees += velTrackSpeed;

            //stabilize rotation; ripped from Player class
            float newRotVel = RotationVelocityDegrees;
            if (newRotVel > 0) //if rotating right
                newRotVel -= stabilization * newRotVel;
            if (newRotVel < 0) //if rotating left
                newRotVel += stabilization * -newRotVel;
            RotationVelocityDegrees = newRotVel; //and assign
        }

        public void RecalcTextureData() //recalculate the width, height, row, column, and sprite center; done on new texture assignment
        {
            GetWidth = Texture.Width / Columns;
            GetHeight = Texture.Height / Rows;
            SpriteCenter = new Vector2(GetWidth >> 1, GetHeight >> 1); //binary shift == divide by two
        }

        protected Vector2 rotationToVector()
        {
            return Rotation.RotationToVector();
        } //local version of extension method

        #endregion

        #region Static Methods

        public static GameObjectPair Bounce(GameObject obj1, GameObject obj2) //bounce two objects and return a pair
        {
            if (obj1.Equals(obj2)) //can't bounce the same object!
                throw new InvalidOperationException("Identical objects");

            Vector2 centerOfMass = (obj1.Velocity + obj2.Velocity) / 2; //calculate the center of mass
            Vector2 normal1 = GetNormal(new GameObjectPair(obj1, obj2));
            Vector2 normal2 = GetNormal(new GameObjectPair(obj2, obj1));

            //Bounce Obj1
            obj1.Velocity -= centerOfMass;
            obj1.Velocity = Vector2.Reflect(obj1.Velocity, normal1);
            obj1.Velocity += centerOfMass;

            //Bounce Obj2
            obj2.Velocity -= centerOfMass;
            obj2.Velocity = Vector2.Reflect(obj2.Velocity, normal2);
            obj2.Velocity += centerOfMass;

            return new GameObjectPair(obj1, obj2);
        }

        public static Vector2 GetNormal(GameObjectPair objects) //get the normal of a pair
        {
            Vector2 normal = objects.Object2.WorldCenter - objects.Object1.WorldCenter;
            normal.Normalize();
            return normal;
        }

        public static Vector2 GetOffset(GameObject root, float radius) //find the specified offset of a rotated root object
        {
            Vector2 temp = root.rotationToVector();
            temp.X *= radius;
            temp.Y *= radius;
            return root.WorldCenter + temp;
        }

        public static Vector2 GetOffset(GameObject root, float radius, float rotation) //overload with x/rotation offset
        {
            Vector2 temp = (root.Rotation + MathHelper.ToRadians(rotation)).RotationToVector();
            temp *= radius;
            return root.WorldCenter + temp;
        }

        #endregion
    }

    public struct GameObjectPair //workaround as a return type
    {
        public GameObject Object1;
        public GameObject Object2;

        public GameObjectPair(GameObject obj1, GameObject obj2)
        {
            Object1 = obj1;
            Object2 = obj2;
        }

        public Vector2 CenterPoint() //get the vector between the two object's centers
        {
            Vector2 temp = Object1.WorldCenter + Object2.WorldCenter;
            temp /= 2;

            return temp;
        }
    }
}
