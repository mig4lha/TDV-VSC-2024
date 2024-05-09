using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace VSC
{
    public class Player
    {
        // Player position
        public Vector2 Position { get; set; }

        // Player texture
        private Texture2D texture;

        // Player speed
        private float speed = 300f;

        // Constructor
        public Player(Texture2D texture, Vector2 position)
        {
            this.texture = texture;
            Position = position;
        }

        // Update method
        public void Update(GameTime gameTime)
        {
            // Get the elapsed time since the last frame
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Get the keyboard state
            KeyboardState keyboardState = Keyboard.GetState();

            // Create a copy of the current position
            Vector2 newPosition = Position;

            // Move the player based on keyboard input
            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
            {
                newPosition.X -= speed * deltaTime; 
            }
            if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
            {
                newPosition.X += speed * deltaTime; 
            }
            if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W))
            {
                newPosition.Y -= speed * deltaTime; 
            }
            if (keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S))
            {
                newPosition.Y += speed * deltaTime;
            }

            // Update the position
            Position = newPosition;
        }


        //public Rectangle GetBounds()
        //{
        //    return new Rectangle((int)Position.X, (int)Position.Y, 16 * (int)Globals.texture_scale_factor, 16 * (int)Globals.texture_scale_factor);
        //}

        public Circle GetBounds()
        {
            // Assuming the player's texture size is 16x16
            // Calculate the radius of the circle
            int radius = (int)(8 * Globals.texture_scale_factor); // Half of the width or height

            // Calculate the center of the circle
            Vector2 center = new Vector2(Position.X + radius, Position.Y + radius);

            // Return a circle object with the center and radius
            return new Circle(center, radius);
        }

        public int GetBoundsRadius()
        {
            // Assuming the player's texture size is 16x16
            // Calculate the radius of the circle
            int radius = (int)(8 * Globals.texture_scale_factor); // Half of the width or height

            return radius;
        }

        // Draw method
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, null, Color.White, 0f, Vector2.Zero, Globals.texture_scale_factor, SpriteEffects.None, 0f);
        }
    }
}
