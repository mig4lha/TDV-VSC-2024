using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using VSC;
using static System.Formats.Asn1.AsnWriter;

public class Player
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public Circle Bounds { get; private set; }

    public float ProjectileSpeed { get; set; }
    public float ProjectileFireRate { get; set; }
    public int DamagePerShot { get; set; }

    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }
    public int Score { get; private set; }

    // Player texture
    private Texture2D texture;

    // Player Speed
    private float Speed = 300f;

    // Health bar dimensions
    private const int HealthBarWidth = 50;
    private const int HealthBarHeight = 5;

    private bool isInvulnerable;
    private float invulnerabilityDuration = Globals.invulnerability_time; // Duration in seconds
    private float invulnerabilityTimer;

    // Constructor
    public Player(Texture2D texture, Vector2 position)
    {
        this.texture = texture;
        Position = position;
        MaxHealth = Globals.default_player_hp;
        CurrentHealth = MaxHealth;

        ProjectileSpeed = 600f;
        ProjectileFireRate = 0.6f;
        DamagePerShot = 50;

        Velocity = Vector2.Zero;
        isInvulnerable = false;
        invulnerabilityTimer = 0;
        IsDead = false;

        Score = 0;

        UpdateBounds();
    }

    // Update method
    public void Update(GameTime gameTime)
    {
        // Get the elapsed time since the last frame
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (isInvulnerable)
        {
            invulnerabilityTimer -= deltaTime;
            if (invulnerabilityTimer <= 0)
            {
                isInvulnerable = false;
            }
        }

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

    public void TakeDamage(int damage)
    {
        if (!isInvulnerable)
        {
            CurrentHealth -= damage;
            if (CurrentHealth < 0)
            {
                IsDead = true;
                return;
            }
            isInvulnerable = true;
            invulnerabilityTimer = invulnerabilityDuration;
        }
    }

    public void IncrementScore(int amount)
    {
        Score += amount;
    }

    // Draw method
    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(texture, Position, null, Color.White, 0f, Vector2.Zero, Globals.texture_scale_factor, SpriteEffects.None, 0f);

        // Draw health bar above the player
        Vector2 healthBarPosition = new Vector2(Position.X, Position.Y - 20);

        // Calculate health bar widths
        float healthPercentage = (float)CurrentHealth / MaxHealth;
        int healthBarGreenWidth = (int)(HealthBarWidth * healthPercentage);
        int healthBarRedWidth = HealthBarWidth - healthBarGreenWidth;

        // Draw green part of health bar
        Rectangle healthBarGreenRect = new Rectangle((int)healthBarPosition.X, (int)healthBarPosition.Y, healthBarGreenWidth, HealthBarHeight);
        spriteBatch.Draw(Utils.CreateRectangleTexture(spriteBatch.GraphicsDevice, healthBarGreenRect.Width, healthBarGreenRect.Height, Color.Green), healthBarGreenRect, Color.Green);

        // Draw red part of health bar (if any)
        if (healthBarRedWidth > 0)
        {
            Rectangle healthBarRedRect = new Rectangle((int)healthBarPosition.X + healthBarGreenWidth, (int)healthBarPosition.Y, healthBarRedWidth, HealthBarHeight);
            spriteBatch.Draw(Utils.CreateRectangleTexture(spriteBatch.GraphicsDevice, healthBarRedRect.Width, healthBarRedRect.Height, Color.Red), healthBarRedRect, Color.Red);
        }
    }
}
