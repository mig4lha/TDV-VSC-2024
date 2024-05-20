using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using VSC;

public class Player
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public Circle Bounds { get; private set; }

    public float ProjectileSpeed { get; set; }
    public float ProjectileFireRate { get; set; }

    // Player texture
    private Texture2D texture;

    // Player Speed
    private float Speed = 300f;

    // Constructor
    public Player(Texture2D texture, Vector2 position)
    {
        this.texture = texture;
        Position = position;

        ProjectileSpeed = 600f;
        ProjectileFireRate = 0.6f;

        Velocity = Vector2.Zero;

        UpdateBounds();
    }

    // Update method
    public void Update(GameTime gameTime)
    {
        // Get the elapsed time since the last frame
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Get the keyboard state
        KeyboardState keyboardState = Keyboard.GetState();

        // Calculate player velocity based on keyboard input
        Vector2 newVelocity = Vector2.Zero;
        if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
        {
            newVelocity.X -= Speed; // Move left
        }
        if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
        {
            newVelocity.X += Speed; // Move right
        }
        if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W))
        {
            newVelocity.Y -= Speed; // Move up
        }
        if (keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S))
        {
            newVelocity.Y += Speed; // Move down
        }

        // Update the velocity
        Velocity = newVelocity;

        // Update the position based on velocity
        Position += Velocity * deltaTime;

        UpdateBounds();
    }

    public void UpdateBounds()
    {
        // Assuming the player's texture size is 16x16
        int radius = (int)(8 * Globals.texture_scale_factor);
        Vector2 center = Position + new Vector2(texture.Width / 2, texture.Height / 2) * Globals.texture_scale_factor;
        Bounds = new Circle(center, radius);
    }

    // Draw method
    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(texture, Position, null, Color.White, 0f, Vector2.Zero, Globals.texture_scale_factor, SpriteEffects.None, 0f);
    }
}
