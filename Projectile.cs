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

        public Projectile(Texture2D texture, Vector2 position, Vector2 velocity)
        {
            Texture = texture;
            Position = position;
            Velocity = velocity;

            DespawnDistance = 3000;

            // Define bounds with a radius matching half of the projectile texture's width
            Bounds = new Circle(position, texture.Width / 2);
        }

        public void Update(float elapsedSeconds)
        {
            // Move the projectile based on its velocity and the elapsed time
            Position += Velocity * elapsedSeconds;

            // Update the position of the bounds
            Bounds = new Circle(Position, Bounds.Radius);   
        }

        public int GetBoundsRadius()
        {
            return (int)Bounds.Radius;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw the projectile's texture centered at the projectile's position
            spriteBatch.Draw(Texture, Position - new Vector2(Texture.Width / 2, Texture.Height / 2), Color.White);
        }

    }
}
