using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace VSC
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private int[,] tileMap; // Variable to store the loaded tile map

        public static Texture2D floor_tile;
        public static Texture2D floor_tile2;
        public static Texture2D floor_tile3;
        public static Texture2D floor_tile4;
        public static Texture2D wall_top_tile;
        public static Texture2D square_player_spawn;
        public static Texture2D skeleton_texture;
        private Texture2D player_sprite;
        public static Texture2D empty_tile;
        public static Texture2D projectileTexture;
        private Texture2D customCursorTexture;
        private float timeSinceLastShot = 0f;

        private Camera camera;

        private SpriteFont defaultFont;
        private SpriteFont timerFont;

        private List<Collision> collisionObjects;

        public List<Projectile> projectiles = new List<Projectile>();
        public static List<Enemy> enemies = new List<Enemy>();

        private Player player;

        private double initialTime = 60.0; // Initial time in seconds
        private double remainingTime;
        private bool timerRunning;

        private Vector2 playerStartPosition;

        private bool playerSpawn = false;

        private bool wasRKeyPressed = false;
        private bool wasF3Pressed = false;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.IsFullScreen = true;
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;

            IsFixedTimeStep = false; // Remove FPS cap
            _graphics.SynchronizeWithVerticalRetrace = false; // Disable vsync

            _graphics.ApplyChanges();

            tileMap = MapLoader.LoadMapFromFile("level1_Big.txt");

            // Placeholder position of the player
            playerStartPosition = new Vector2(100, 100);

            // LoadContent before initializing player object so the texture is loaded
            LoadContent();

            player = new Player(player_sprite, playerStartPosition);

            collisionObjects = Collision.CreateCollisionObjects(GraphicsDevice,tileMap);
        }

        protected override void Initialize()
        {
            Utils.selectedFloorTextures = new Texture2D[Globals.MapWidth, Globals.MapHeight];

            MouseCursor customCursor = Utils.ScaleCursorTexture(GraphicsDevice, customCursorTexture, Globals.cursor_texture_scale_factor);

            // Set the custom cursor
            Mouse.SetCursor(customCursor);

            // Initialize camera with starting position
            camera = new Camera(GraphicsDevice, Vector2.Zero);

            remainingTime = initialTime;
            timerRunning = true;

            SpawnEnemies(10); // Spawn 10 enemies

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            floor_tile = Content.Load<Texture2D>("floor_tile");
            floor_tile2 = Content.Load<Texture2D>("floor_tile2");
            floor_tile3 = Content.Load<Texture2D>("floor_tile3");
            floor_tile4 = Content.Load<Texture2D>("floor_tile4");
            wall_top_tile = Content.Load<Texture2D>("wall_top_tile");
            square_player_spawn = Content.Load<Texture2D>("square_player_spawn");
            empty_tile = Content.Load<Texture2D>("empty_tile");
            player_sprite = Content.Load<Texture2D>("priest1_v1_1");
            projectileTexture = Content.Load<Texture2D>("projectile");
            skeleton_texture = Content.Load<Texture2D>("skeleton2_v2_1");

            defaultFont = Content.Load<SpriteFont>("TestFont");
            timerFont = Content.Load<SpriteFont>("Timer");

            customCursorTexture = Content.Load<Texture2D>("cursor");
        }

        protected override void Update(GameTime gameTime)
        {
            // Get the elapsed time since the last frame
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Utils.UpdateFPS(gameTime);

            if (timerRunning)
            {
                remainingTime -= deltaTime;
                if (remainingTime <= 0)
                {
                    remainingTime = 0;
                    timerRunning = false;
                    // Handle timer reaching 0 (e.g., end game, display message)
                }
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Check if the "R" key is pressed
            bool isRKeyPressed = Keyboard.GetState().IsKeyDown(Keys.R);

            // Restart player position if "R" key is released and was pressed in the previous frame
            if (wasRKeyPressed && !isRKeyPressed)
            {
                // Reset player position to the spawn location
                player.Position = playerStartPosition;
                SpawnEnemies(10);
            }

            // Update the variable for the next frame
            wasRKeyPressed = isRKeyPressed;

            KeyboardState keyboardState = Keyboard.GetState();

            // Check if F3 key is pressed and was not pressed in the previous frame
            if (keyboardState.IsKeyDown(Keys.F3) && !wasF3Pressed)
            {
                // Toggle debug menu visibility
                Globals.debugMenuVisible = !Globals.debugMenuVisible;
            }

            // Update the flag to track F3 key press
            wasF3Pressed = keyboardState.IsKeyDown(Keys.F3);

            // Store the player's current position
            Vector2 previousPosition = player.Position;

            player.Update(gameTime);

            // Handle collisions
            bool collisionDetected = false;

            foreach (Collision collisionObject in collisionObjects)
            {
                if (Collision.Collides(player.Bounds, collisionObject.Bounds))
                {
                    collisionDetected = true;
                    break;
                }
            }

            if (collisionDetected)
            {
                // Handle collision response by adjusting position incrementally
                player.Position = previousPosition;
                player.UpdateBounds();

                // Attempt to move player along x-axis only
                player.Position = new Vector2(player.Position.X + player.Velocity.X * deltaTime, player.Position.Y);
                player.UpdateBounds();
                collisionDetected = false;
                foreach (Collision collisionObject in collisionObjects)
                {
                    if (Collision.Collides(player.Bounds, collisionObject.Bounds))
                    {
                        collisionDetected = true;
                        break;
                    }
                }

                if (collisionDetected)
                {
                    // Revert x-axis movement
                    player.Position = new Vector2(previousPosition.X, player.Position.Y);
                    player.UpdateBounds();
                }

                // Attempt to move player along y-axis only
                player.Position = new Vector2(player.Position.X, player.Position.Y + player.Velocity.Y * deltaTime);
                player.UpdateBounds();
                collisionDetected = false;
                foreach (Collision collisionObject in collisionObjects)
                {
                    if (Collision.Collides(player.Bounds, collisionObject.Bounds))
                    {
                        collisionDetected = true;
                        break;
                    }
                }

                if (collisionDetected)
                {
                    // Revert y-axis movement
                    player.Position = new Vector2(player.Position.X, previousPosition.Y);
                    player.UpdateBounds();
                }
            }

            // Calculate time elapsed since last shot
            timeSinceLastShot += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Detect mouse left click and check if enough time has passed since last shot
            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed && timeSinceLastShot >= player.ProjectileFireRate)
            {
                // Start projectile from player center
                Vector2 projectilePosition = player.Position + new Vector2(player.Bounds.Radius);

                // Calculate direction towards cursor
                Vector2 directionToCursor = new Vector2(mouseState.X, mouseState.Y) - (projectilePosition - camera.Position);
                directionToCursor.Normalize();

                // Check if the player is moving
                if (player.Velocity != Vector2.Zero)
                {
                    // Calculate the angle between the player's velocity direction and the cursor direction
                    float angleBetween = (float)Math.Atan2(player.Velocity.Y, player.Velocity.X) - (float)Math.Atan2(directionToCursor.Y, directionToCursor.X);

                    // Adjust the projectile direction based on the angle and a predetermined factor
                    float momentumFactor = MathHelper.ToDegrees(angleBetween) * Globals.player_momentum_projectile_factor; // Convert to degrees and apply the factor
                    float maxChangeAngle = 10f; // Maximum change angle in degrees
                    momentumFactor = MathHelper.Clamp(momentumFactor, -maxChangeAngle, maxChangeAngle); // Clamp the factor within the maximum change angle
                    float adjustedAngle = MathHelper.ToRadians(MathHelper.ToDegrees((float)Math.Atan2(directionToCursor.Y, directionToCursor.X)) + momentumFactor); // Calculate the adjusted angle
                    directionToCursor = new Vector2((float)Math.Cos(adjustedAngle), (float)Math.Sin(adjustedAngle)); // Convert the adjusted angle back to a direction vector
                }

                // Normalize the direction vector again
                directionToCursor.Normalize();

                // Create projectile at player position with velocity in the calculated direction
                Vector2 projectileVelocity = directionToCursor * player.ProjectileSpeed;
                projectiles.Add(new Projectile(projectileTexture, projectilePosition, projectileVelocity));

                // Reset time since last shot
                timeSinceLastShot = 0f;
            }


            // Update projectiles
            foreach (Projectile projectile in projectiles.ToList())
            {
                projectile.Update(deltaTime);

                // Check for collision with other objects or bounds
                foreach (Collision collisionObject in collisionObjects)
                {
                    if (Collision.Collides(projectile.Bounds, collisionObject.Bounds))
                    {
                        // Handle collision
                        // For example: remove the projectile from the list
                        projectiles.Remove(projectile);
                        break; // No need to check for collision with other objects
                    }
                }
            }

            // Update enemies
            foreach (Enemy enemy in enemies.ToList())
            {
                enemy.Update(gameTime, player.Position, collisionObjects, enemies);

                // Check for collision with projectiles
                foreach (Projectile projectile in projectiles.ToList())
                {
                    if (Collision.CircleCircleCollision(enemy.Bounds, projectile.Bounds))
                    {
                        enemy.TakeDamage(player.DamagePerShot);
                        projectiles.Remove(projectile);
                        break; // No need to check for collision with other projectiles
                    }
                }
            }

            // Remove projectiles if they move beyond a certain radius from the player
            float maxDistanceSquared = Projectile.DespawnDistance * Projectile.DespawnDistance; // 500px radius
            projectiles.RemoveAll(p => Vector2.DistanceSquared(p.Position, player.Position) > maxDistanceSquared);

            // Update camera to follow player
            camera.Follow(player.Position);

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            Color backgroundColor = new Color(0x25, 0x13, 0x1A); // Hex: #25131A
            GraphicsDevice.Clear(backgroundColor);

            // Begin drawing with camera's transform matrix
            _spriteBatch.Begin(transformMatrix: camera.TransformMatrix);

            // Draw the tile map
            for (int x = 0; x < Globals.MapWidth; x++)
            {
                for (int y = 0; y < Globals.MapHeight; y++)
                {
                    Vector2 position = new Vector2(x * Globals.TileWidth * Globals.texture_scale_factor,y * Globals.TileHeight * Globals.texture_scale_factor);

                    int tileType = tileMap[x, y];

                    Texture2D tileTexture = Utils.GetTextureForTileType(tileType, x, y);

                    // Draw the tile with scaling applied
                    _spriteBatch.Draw(tileTexture, position, null, Color.White, 0f, Vector2.Zero, Globals.texture_scale_factor, SpriteEffects.None, 0f);

                    if (tileType == 3 && playerSpawn == false) // Assuming tileType 3 represents the tile where the player should be drawn
                    {
                        playerStartPosition = new Vector2(position.X + 1, position.Y);
                        // Calculate player position on top of this tile
                        player.Position = new Vector2(position.X + 1, position.Y);
                        playerSpawn = true;
                    }
                }
            }

            // Draw the player
            player.Draw(_spriteBatch);

            foreach (Projectile projectile in projectiles)
            {
                projectile.Draw(_spriteBatch);
            }

            // Draw enemies
            foreach (Enemy enemy in enemies)
            {
                enemy.Draw(_spriteBatch);
            }

            _spriteBatch.End();

            // Draw debug menu if visible
            if (Globals.debugMenuVisible)
            {
                Utils.DrawDebugMenu(_spriteBatch, defaultFont, collisionObjects, GraphicsDevice, player, projectiles, camera, enemies);
            }

            // Begin a new sprite batch for UI elements
            _spriteBatch.Begin();

            // Draw the timer
            int minutes = (int)(remainingTime / 60);
            int seconds = (int)(remainingTime % 60);
            string timerText = $"{minutes:D2}:{seconds:D2}";

            // Measure the width of the text
            Vector2 textSize = defaultFont.MeasureString(timerText);

            // Calculate the position to center the text on the X axis
            float xPosition = (GraphicsDevice.Viewport.Width - textSize.X) / 2;
            Vector2 position_timer = new Vector2(xPosition, 50);

            // Draw the centered text
            _spriteBatch.DrawString(timerFont, timerText, position_timer, Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void SpawnEnemies(int count)
        {
            Random random = new Random();

            while (enemies.Count < count)
            {
                // Generate random position within map bounds
                int x = random.Next(0, Globals.MapWidth);
                int y = random.Next(0, Globals.MapHeight);

                // Check if the tile is not empty or a wall
                if (tileMap[x, y] != 0 && tileMap[x, y] != 2 && tileMap[x, y] != 3)
                {
                    // Calculate position in world coordinates
                    Vector2 position = new Vector2(x * Globals.TileWidth, y * Globals.TileHeight);

                    // Create enemy object and add to list
                    Enemy enemy = new Enemy(skeleton_texture, position);
                    enemies.Add(enemy);
                }
            }
        }
    }
}
