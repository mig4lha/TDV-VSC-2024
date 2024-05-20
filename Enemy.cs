using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using VSC;

public class Enemy
{
    // Enemy properties
    public Vector2 Position { get; set; }
    public Texture2D Texture { get; set; }
    public Circle Bounds { get; private set; } // Hitbox for the enemy

    // Constructor
    public Enemy(Texture2D texture, Vector2 position)
    {
        Texture = texture;
        Position = position;

        // Calculate the hitbox radius based on the texture scale factor
        float scaleFactor = Globals.texture_scale_factor;
        float scaledRadius = texture.Width * scaleFactor * 0.5f;

        float increasedRadius = scaledRadius * 1.2f;

        // Define bounds with the scaled radius centered on the enemy
        Bounds = new Circle(Position + new Vector2(texture.Width * 0.5f * scaleFactor, texture.Height * 0.5f * scaleFactor), increasedRadius);
    }

    // Update method
    public void Update(GameTime gameTime)
    {
        // Update the position of the bounds to ensure it stays centered on the enemy
        Bounds.Center = Position + new Vector2(Texture.Width * 0.5f * Globals.texture_scale_factor, Texture.Height * 0.5f * Globals.texture_scale_factor);

        // You can add additional logic for enemy behavior here
    }

    // Draw method
    public void Draw(SpriteBatch spriteBatch)
    {
        // Draw the enemy texture centered on its position
        spriteBatch.Draw(Texture, Position, null, Color.White, 0f, Vector2.Zero, Globals.texture_scale_factor, SpriteEffects.None, 0f);
    }
}
