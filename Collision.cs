using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSC
{
    public class Collision
    {
        // Rectangle representing the bounds of the collision object
        public Rectangle Bounds { get; set; }

        public Collision(Rectangle bounds)
        {
            Bounds = bounds;
        }

        public Rectangle GetBounds()
        {
            return Bounds;
        }

        // Method to check for collision with another rectangle
        public bool CollidesWith(Rectangle otherBounds)
        {
            return Bounds.Intersects(otherBounds);
        }

        //public static bool CollidesWithCircle(Circle playerBounds, Rectangle otherBounds)
        //{
        //    Circle circle = new Circle(playerBounds.Center,playerBounds.Radius);
        //    return circle.Intersects(otherBounds, playerBounds);
        //}

        // Method to check for collision between a circle and a rectangle
        public static bool CollidesWithCircle(Circle circle, Rectangle rectangle)
        {
            // Find the closest point to the circle within the rectangle
            float closestX = MathHelper.Clamp(circle.Center.X, rectangle.Left, rectangle.Right);
            float closestY = MathHelper.Clamp(circle.Center.Y, rectangle.Top, rectangle.Bottom);

            // Calculate the distance between the circle's center and this closest point
            float distanceX = circle.Center.X - closestX;
            float distanceY = circle.Center.Y - closestY;

            // If the distance is less than the circle's radius, there's an intersection
            float distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);
            return distanceSquared < (circle.Radius * circle.Radius);
        }

        public static List<Collision> CreateCollisionObjects(GraphicsDevice GraphicsDevice, int[,] tileMap)
        {
            List<Collision> collisionObjects = new List<Collision>();

            // Calculate offset to center the map on the screen
            int offsetX = (GraphicsDevice.Viewport.Width - (Globals.MapWidth * Globals.TileWidth * (int)Globals.texture_scale_factor)) / 2;
            int offsetY = (GraphicsDevice.Viewport.Height - (Globals.MapHeight * Globals.TileHeight * (int)Globals.texture_scale_factor)) / 2;
            offsetX = 0;
            offsetY = 0;

            // Iterate over tileMap to find collision objects and create corresponding objects
            for (int x = 0; x < Globals.MapWidth; x++)
            {
                for (int y = 0; y < Globals.MapHeight; y++)
                {
                    // Identify the type of collision object based on the tile value
                    int tileType = tileMap[x, y];

                    // Check if the tile represents a collision object
                    if (IsCollisionObject(tileType))
                    {
                        // Calculate position for the collision object without applying scale factor
                        int xPos = offsetX + x * Globals.TileWidth * (int)Globals.texture_scale_factor;
                        int yPos = offsetY + y * Globals.TileHeight * (int)Globals.texture_scale_factor;
                        Rectangle boundsFullTile = new Rectangle(xPos, yPos, Globals.TileWidth * (int)Globals.texture_scale_factor, Globals.TileHeight * (int)Globals.texture_scale_factor);

                        // Create a collision object based on its type
                        Collision collisionObject;
                        switch (tileType)
                        {
                            case 2: // Wall
                                Rectangle bounds = new Rectangle(xPos, yPos, Globals.TileWidth * (int)Globals.texture_scale_factor, Globals.TileHeight * (int)Globals.texture_scale_factor - 30);
                                collisionObject = new Collision(bounds);
                                break;
                            // Add more cases for other types of collision objects
                            default:
                                // Handle other types of collision objects
                                collisionObject = new Collision(boundsFullTile);
                                break;
                        }

                        // Add the collision object to the list
                        collisionObjects.Add(collisionObject);
                    }
                }
            }

            return collisionObjects;
        }


        public static bool IsCollisionObject(int tileType)
        {
            return tileType == 2;
        }
    }
}
