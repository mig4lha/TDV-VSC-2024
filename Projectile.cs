using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSC
{
    public class Projectile
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public static int DespawnDistance { get; set; }
        public Texture2D Texture { get; set; }
        public Circle Bounds { get; private set; } // Hitbox for the projectile
        public SpriteAnimation Animation { get; set; }
        public float Rotation { get; private set; } // Rotation angle in radians
        public Vector2 Origin { get; private set; } // Origin vector to keep projectile centered in initial position after rotation

        public Projectile(Texture2D texture, Vector2 position, Vector2 velocity, Vector2 directionToCursor)
        {
            Texture = texture;
            Position = position;
            Velocity = velocity;

            // Calculate rotation angle
            Rotation = (float)Math.Atan2(-directionToCursor.Y, -directionToCursor.X);

            DespawnDistance = 3000;

            // Define bounds with a radius matching half of the projectile texture's width
            Bounds = new Circle(position, texture.Width);

            Animation = new SpriteAnimation(Game1.attackSpritesheet, 32, 32, 0, 3, 0.1f);
        }

        public void Update(float deltaTime)
        {
            // Move the projectile based on its velocity and the elapsed time
            Position += Velocity * deltaTime;

            // Update the position of the bounds
            Bounds = new Circle(Position, Bounds.Radius);

            Animation.Update(deltaTime); // Update animation frame
        }

        public int GetBoundsRadius()
        {
            return (int)Bounds.Radius;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Origin = new Vector2(Texture.Width, Texture.Height);
            // Draw the projectile's texture centered at the projectile's position
            Animation.Draw(spriteBatch, Position, false, Rotation, Origin, false);
        }

    }
}
