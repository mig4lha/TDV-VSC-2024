using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using VSC;
using System.Collections.Generic;

public class Enemy
{
    // Enemy properties
    public Vector2 Position { get; set; }
    public Texture2D Texture { get; set; }
    public Circle Bounds { get; private set; } // Hitbox for the enemy
    public int Health { get; private set; }
    public int Damage { get; private set; }
    public SpriteAnimation Animation { get; set; }

    private float speed;

    // Constructor
    public Enemy(Texture2D texture, Vector2 position)
    {
        Texture = texture;
        Position = position;

        // Calculate the hitbox radius based on the texture scale factor
        float scaleFactor = Globals.texture_scale_factor;
        float scaledRadius = texture.Width/4 * scaleFactor * 0.5f;

        // Define bounds with the scaled radius
        Bounds = new Circle(position + new Vector2(scaledRadius), scaledRadius);
        Health = Globals.default_enemy_hp; // Set initial health
        speed = Globals.default_enemy_speed; // Speed of the enemy
        Damage = Globals.default_enemy_damage;

        Animation = new SpriteAnimation(Game1.skeletonSpritesheet, 16, 16, 0, 3, 0.1f);
    }

    // Update method
    public void Update(GameTime gameTime, Vector2 playerPosition, List<Collision> collisionObjects, List<Enemy> otherEnemies)
    {
        if(Game1.currentState == Game1.GameState.Playing)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Calculate direction towards player
            Vector2 direction = playerPosition - Position;
            direction.Normalize();

            // Calculate new position
            Vector2 newPosition = Position + direction * speed * deltaTime;

            // Check for potential collisions with walls
            Circle newBounds = new Circle(newPosition + new Vector2(Bounds.Radius), Bounds.Radius);
            foreach (var collisionObject in collisionObjects)
            {
                if (Collision.Collides(newBounds, collisionObject.Bounds))
                {
                    return; // If collision detected, do not move
                }
            }

            // Check for potential collisions with other enemies
            foreach (var enemy in otherEnemies)
            {
                if (enemy == this) continue;

                if (Collision.CircleCircleCollision(newBounds, enemy.Bounds))
                {
                    // Calculate repulsion vector
                    Vector2 repulsion = newPosition - enemy.Position;
                    float distance = repulsion.Length();
                    if (distance == 0)
                    {
                        // If the enemies are exactly at the same position, create a small random vector
                        repulsion = new Vector2(0.1f, 0.1f);
                        distance = repulsion.Length();
                    }

                    // Normalize repulsion vector and calculate overlap
                    repulsion.Normalize();
                    float overlap = Bounds.Radius + enemy.Bounds.Radius - distance;

                    // Adjust the newPosition based on the overlap
                    newPosition += repulsion * overlap * 0.5f;
                }
            }

            // Update position and bounds
            Position = newPosition;
            Bounds = new Circle(Position + new Vector2(Bounds.Radius), Bounds.Radius);

            Animation.Update(deltaTime); // Update animation frame
        }
    }

    public void TakeDamage(int damage, Player player)
    {
        Health -= damage;
        if (Health <= 0)
        {
            // Handle enemy death (e.g., remove from the game)
            Health = 0; // Ensure health doesn't go negative
            // Remove the enemy from the game
            // For example, you can remove it from a list of active enemies
            // Assuming enemiesList is a list containing all active enemies
            if (Game1.enemies.Contains(this))
            {
                Game1.enemies.Remove(this);
                player.IncrementScore(100);
            }
        }
    }

    // Draw method
    public void Draw(SpriteBatch spriteBatch)
    {
        Vector2 newPosition = Position;
        Animation.Draw(spriteBatch, newPosition, false, 0f, Vector2.Zero, true);
        //spriteBatch.Draw(Texture, Position, null, Color.White, 0f, Vector2.Zero, Globals.texture_scale_factor, SpriteEffects.None, 0f);
    }
}
