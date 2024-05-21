using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSC
{
    public class Circle
    {
        public Vector2 Center { get; set; }
        public float Radius { get; set; }

        public Circle(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        // Method to check if a point is inside the circle
        public bool Contains(Vector2 point)
        {
            float distanceSquared = Vector2.DistanceSquared(Center, point);
            return distanceSquared <= (Radius * Radius);
        }

        public bool Intersects(Rectangle rectangle, Circle playerBounds)
        {
            // Find the closest point to the circle within the rectangle
            float closestX = MathHelper.Clamp(playerBounds.Center.X, rectangle.Left, rectangle.Right);
            float closestY = MathHelper.Clamp(playerBounds.Center.Y, rectangle.Top, rectangle.Bottom);

            // Calculate the distance between the circle's center and this closest point
            float distanceX = playerBounds.Center.X - closestX;
            float distanceY = playerBounds.Center.Y - closestY;

            // If the distance is less than the circle's radius, there's an intersection
            float distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);
            return distanceSquared < (Radius * Radius);
        }

        public static void DrawCircle(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, int radius, Color color)
        {
            position.X += radius;
            position.Y += radius;
            spriteBatch.Draw(texture, position - new Vector2(radius), null, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
    }

}
