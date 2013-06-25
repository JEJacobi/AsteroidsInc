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
    public struct Circle //simple bounding circle struct
    {
        public Vector2 Position;
        public int Radius;

        public Circle(Vector2 position, int radius)
        {
            Position = position;
            Radius = radius;
        }

        public bool Intersects(Circle obj)
        {
            return Intersects(this, obj);
        }

        public override string ToString()
        {
            return Position.ToString() + ", " + Radius.ToString();
        } 

        public static bool Intersects(Circle circle1, Circle circle2)
        {
            //Distance squared to avoid costly sqrt
            float distance = Vector2.DistanceSquared(circle1.Position, circle2.Position);
            int totalradii = circle1.Radius + circle2.Radius; //get the total radii
            totalradii *= totalradii; //and raise it by the power of two (multiplied by itself instead of Math.Pow)

            if (distance < totalradii) //if the distance is smaller than the sum of the radii
                return true; //collision!
            return false;
        }
    }
}
