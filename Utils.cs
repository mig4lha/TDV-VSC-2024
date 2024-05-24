using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static VSC.Game1;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace VSC
{
    public static class Utils
    {
        private static int frameCount;
        private static float elapsedTime;
        private static float fps;

        public static Texture2D redTexture;

        public static Texture2D[,] selectedFloorTextures;

        public static float FPS => fps;

        public static void UpdateFPS(GameTime gameTime)
        {
            // Update FPS variables
            frameCount++;
            elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update FPS every second
            if (elapsedTime >= 1.0f)
            {
                // Calculate FPS
                fps = frameCount / elapsedTime;

                // Reset frame count and elapsed time
                frameCount = 0;
                elapsedTime = 0f;
            }
        }

        public static void DrawDebugMenu(SpriteBatch spriteBatch, SpriteFont spriteFont, List<Collision> collisionObjects, GraphicsDevice graphics, Player player, List<Projectile> projectiles, Camera camera, List<Enemy> enemies)
        {
            string fpsText = $"FPS: {FPS:F2}";
            redTexture = CreateColoredTexture(graphics, Color.Red);

            spriteBatch.Begin();

            // Draw collision objects
            foreach (Collision collisionObject in collisionObjects)
            {
                Rectangle bounds = collisionObject.Bounds;
                bounds.X -= (int)camera.Position.X;
                bounds.Y -= (int)camera.Position.Y;
                spriteBatch.Draw(redTexture, bounds, Color.White);
            }

            // Draw player collision circle
            Circle playerBounds = player.Bounds;
            Vector2 playerBoundsPosition = playerBounds.Center - new Vector2(playerBounds.Radius) - camera.Position;
            Circle.DrawCircle(spriteBatch, CreateCircleTexture(graphics, (int)playerBounds.Radius, Color.White), playerBoundsPosition, (int)playerBounds.Radius, Color.White);

            // Draw projectile collision circles
            foreach (Projectile projectile in projectiles)
            {
                Vector2 texturePosition = projectile.Position - new Vector2(projectile.GetBoundsRadius());
                texturePosition -= camera.Position;
                Circle.DrawCircle(spriteBatch, CreateCircleTexture(graphics, projectile.GetBoundsRadius(), Color.Blue), texturePosition, projectile.GetBoundsRadius(), Color.Blue);
            }

            // Draw enemy collision circles
            foreach (Enemy enemy in enemies)
            {
                Vector2 texturePosition = enemy.Bounds.Center - new Vector2(enemy.Bounds.Radius) - camera.Position;
                Circle.DrawCircle(spriteBatch, CreateCircleTexture(graphics, (int)enemy.Bounds.Radius, Color.Green), texturePosition, (int)enemy.Bounds.Radius, Color.Green);
            }

            spriteBatch.DrawString(spriteFont, fpsText, new Vector2(10, 10), Color.White);
            spriteBatch.DrawString(spriteFont, $"Player Position: X:{player.Position.X:F2} Y:{player.Position.Y:F2}", new Vector2(10, 30), Color.White);

            spriteBatch.End();
        }






        public static Texture2D CreateCircleTexture(GraphicsDevice graphicsDevice, int radius, Color color)
        {
            int diameter = radius * 2;
            Texture2D texture = new Texture2D(graphicsDevice, diameter, diameter);
            Color[] colorData = new Color[diameter * diameter];

            float radiussquared = radius * radius;

            for (int x = 0; x < diameter; x++)
            {
                for (int y = 0; y < diameter; y++)
                {
                    int index = x * diameter + y;
                    Vector2 pos = new Vector2(x - radius, y - radius);
                    if (pos.LengthSquared() <= radiussquared)
                    {
                        colorData[index] = color;
                    }
                    else
                    {
                        colorData[index] = Color.Transparent;
                    }
                }
            }

            texture.SetData(colorData);
            return texture;
        }

        public static void DrawCircle(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, int radius, Color color)
        {
            spriteBatch.Draw(texture, position, color);
        }


        public static MouseCursor ScaleCursorTexture(GraphicsDevice _graphics, Texture2D originalCursorTexture, float scaleFactor)
        {
            // Calculate the scaled width and height
            int scaledWidth = (int)(originalCursorTexture.Width * scaleFactor);
            int scaledHeight = (int)(originalCursorTexture.Height * scaleFactor);

            // Create a scaled version of the cursor texture
            Color[] originalData = new Color[originalCursorTexture.Width * originalCursorTexture.Height];
            originalCursorTexture.GetData(originalData);
            Color[] scaledData = new Color[scaledWidth * scaledHeight];
            for (int y = 0; y < scaledHeight; y++)
            {
                for (int x = 0; x < scaledWidth; x++)
                {
                    int index = (int)(x / scaleFactor) + (int)(y / scaleFactor) * originalCursorTexture.Width;
                    scaledData[x + y * scaledWidth] = originalData[index];
                }
            }

            Texture2D scaledCursorTexture = new Texture2D(_graphics, scaledWidth, scaledHeight);
            scaledCursorTexture.SetData(scaledData);

            // Create a MouseCursor object from the scaled cursor texture
            MouseCursor scaledCursor = MouseCursor.FromTexture2D(scaledCursorTexture, 0, 0);

            return scaledCursor;
        }

        public static Texture2D CreateColoredTexture(GraphicsDevice graphicsDevice, Color color)
        {
            Texture2D texture = new Texture2D(graphicsDevice, 1, 1);

            texture.SetData(new[] { color });
            return texture;
        }

        public static Texture2D GetTextureForTileType(int tileType, int x, int y)
        {
            switch (tileType)
            {
                case 1: // Floor tile
                        // If the selected floor texture for this tile has not been chosen yet
                    if (selectedFloorTextures[x, y] == null)
                    {
                        // Seed the random number generator with a value based on the map's size and position
                        int seed = x * Globals.MapWidth + y; // Formula for the seed using the map width and height
                        Random localRandom = new Random(seed);

                        // Randomly select one of the floor tile textures
                        int randomFloorTile = localRandom.Next(1, 5); // Random number between 1 and 4
                        switch (randomFloorTile)
                        {
                            case 1:
                                selectedFloorTextures[x, y] = Game1.floor_tile;
                                break;
                            case 2:
                                selectedFloorTextures[x, y] = Game1.floor_tile2;
                                break;
                            case 3:
                                selectedFloorTextures[x, y] = Game1.floor_tile3;
                                break;
                            case 4:
                                selectedFloorTextures[x, y] = Game1.floor_tile4;
                                break;
                            default:
                                selectedFloorTextures[x, y] = Game1.floor_tile;
                                break;
                        }
                    }
                    return selectedFloorTextures[x, y];
                case 2: // Wall
                    return Game1.wall_top_tile;
                case 3: // Player spawn
                    return Game1.square_player_spawn;
                default:
                    return Game1.empty_tile;
            }
        }

        public static Texture2D CreateRectangleTexture(GraphicsDevice graphicsDevice, int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(graphicsDevice, width, height);
            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = color;
            }
            texture.SetData(data);
            return texture;
        }


        public static void UpdateMainMenu(GameTime gameTime, GraphicsDevice graphicsDevice)
        {
            // Handle menu input and transitions
            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Enter) && !wasEnterKeyPressed)
            {
                wasEnterKeyPressed = true;
                currentState = GameState.Playing;
            }
        }


        public static void UpdatePlaying(GameTime gameTime, float deltaTime, Player player, List<Projectile> projectiles)
        {
            float deathAnimationDuration = 3.0f;
            float deathAnimationTimer = 0.0f;

            if (player.IsDead)
            {
                // If the player is dead, start a timer for the death animation
                deathAnimationTimer += deltaTime;

                // Check if the death animation has finished playing
                if (deathAnimationTimer >= deathAnimationDuration)
                {
                    // Transition to the game over state
                    currentState = GameState.GameOver;
                    return;
                }
            }

            // Pausing game
            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.P) && !wasPKeyPressed)
            {
                isPaused = !isPaused; // Toggle the pause state
                currentState = isPaused ? GameState.Paused : GameState.Playing; // Update game state
            }
            wasPKeyPressed = keyboardState.IsKeyDown(Keys.P);

            if (!isPaused) // Only update the timer if the game is not paused
            {
                if (timerRunning)
                {
                    remainingTime -= deltaTime;
                    if (remainingTime <= 0)
                    {
                        remainingTime = 0;
                        timerRunning = false;

                        currentState = GameState.Win;
                    }
                }
            }

            keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.F3) && !wasF3Pressed)
            {
                Globals.debugMenuVisible = !Globals.debugMenuVisible;
            }
            wasF3Pressed = keyboardState.IsKeyDown(Keys.F3);

            // Update spawn timer
            spawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Check if it's time to spawn more enemies
            if (spawnTimer >= spawnInterval && timerRunning == true)
            {
                int num_enemies = (int)(Globals.timer_in_seconds - remainingTime);
                SpawnEnemies(num_enemies);

                // Reset the spawn timer
                spawnTimer = 0f;
            }

            timeSinceLastShot += (float)gameTime.ElapsedGameTime.TotalSeconds;

            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed && timeSinceLastShot >= player.ProjectileFireRate)
            {
                Vector2 projectilePosition = player.Position + new Vector2(player.Bounds.Radius);

                Vector2 directionToCursor = new Vector2(mouseState.X, mouseState.Y) - (projectilePosition - camera.Position);
                directionToCursor.Normalize();

                if (player.Velocity != Vector2.Zero)
                {
                    float angleBetween = (float)Math.Atan2(player.Velocity.Y, player.Velocity.X) - (float)Math.Atan2(directionToCursor.Y, directionToCursor.X);
                    float momentumFactor = MathHelper.ToDegrees(angleBetween) * Globals.player_momentum_projectile_factor;
                    float maxChangeAngle = 10f;
                    momentumFactor = MathHelper.Clamp(momentumFactor, -maxChangeAngle, maxChangeAngle);
                    float adjustedAngle = MathHelper.ToRadians(MathHelper.ToDegrees((float)Math.Atan2(directionToCursor.Y, directionToCursor.X)) + momentumFactor);
                    directionToCursor = new Vector2((float)Math.Cos(adjustedAngle), (float)Math.Sin(adjustedAngle));
                }

                directionToCursor.Normalize();
                Vector2 projectileVelocity = directionToCursor * player.ProjectileSpeed;
                projectiles.Add(new Projectile(projectileTexture, projectilePosition, projectileVelocity, directionToCursor));
                timeSinceLastShot = 0f;
            }

            Vector2 previousPosition = player.Position;
            player.Update(gameTime);

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
                player.Position = previousPosition;
                player.UpdateBounds();

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
                    player.Position = new Vector2(previousPosition.X, player.Position.Y);
                    player.UpdateBounds();
                }

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
                    player.Position = new Vector2(player.Position.X, previousPosition.Y);
                    player.UpdateBounds();
                }
            }

            foreach (Projectile projectile in projectiles.ToList())
            {
                projectile.Update(deltaTime);
                foreach (Collision collisionObject in collisionObjects)
                {
                    if (Collision.Collides(projectile.Bounds, collisionObject.Bounds))
                    {
                        projectiles.Remove(projectile);
                        break;
                    }
                }
            }

            foreach (Enemy enemy in enemies.ToList())
            {
                enemy.Update(gameTime, player.Position, collisionObjects, enemies);

                foreach (Projectile projectile in projectiles.ToList())
                {
                    if (Collision.CircleCircleCollision(enemy.Bounds, projectile.Bounds))
                    {
                        enemy.TakeDamage(player.DamagePerShot, player);
                        projectiles.Remove(projectile);
                        break;
                    }
                }

                if (Collision.CircleCircleCollision(player.Bounds, enemy.Bounds))
                {
                    player.TakeDamage(enemy.Damage);
                    if(player.IsDead == true)
                    {
                        currentState = GameState.GameOver;
                        return;
                    }
                }
            }

            float maxDistanceSquared = Projectile.DespawnDistance * Projectile.DespawnDistance;
            projectiles.RemoveAll(p => Vector2.DistanceSquared(p.Position, player.Position) > maxDistanceSquared);

            camera.Follow(player.Position);
        }

        public static void UpdatePaused(GameTime gameTime)
        {
            // Handle paused state input
            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.P) && !wasPKeyPressed)
            {
                isPaused = !isPaused; // Toggle the pause state
                currentState = isPaused ? GameState.Paused : GameState.Playing; // Update game state
            }
            wasPKeyPressed = keyboardState.IsKeyDown(Keys.P);
        }

        public static void UpdateGameOver(GameTime gameTime)
        {
            // Handle game over input and transitions
            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Enter) && !wasEnterKeyPressed)
            {
                wasEnterKeyPressed = true;
                currentState = GameState.MainMenu;
            }
        }

        public static void UpdateWin(GameTime gameTime)
        {
            // Handle game over input and transitions
            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Enter) && !wasEnterKeyPressed)
            {
                wasEnterKeyPressed = true;
                currentState = GameState.MainMenu;
            }
        }


        public static void DrawMainMenu(SpriteBatch spriteBatch, Texture2D main_menu_background, Texture2D logo, Texture2D enterKeyTexture, SpriteFont menuFont, GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            // Calculate the center position of the screen
            Vector2 screenCenter = new Vector2(graphicsDevice.Viewport.Width / 2f, graphicsDevice.Viewport.Height / 2f);

            // Calculate the scaled dimensions of the Enter key texture
            float scaledEnterKeyWidth = enterKeyTexture.Width * Globals.texture_scale_factor;
            float scaledEnterKeyHeight = enterKeyTexture.Height * Globals.texture_scale_factor;

            // Define the vertical spacing between the logo and the text
            float verticalSpacing = 80f;

            // Calculate the total height of the logo, text, and Enter key texture
            float totalHeight = logo.Height + verticalSpacing + Math.Max(menuFont.LineSpacing, scaledEnterKeyHeight);

            // Calculate the starting position to center the group vertically
            float startY = screenCenter.Y - totalHeight / 2f;

            // Calculate the position of the logo (centered horizontally)
            Vector2 logoPosition = new Vector2(screenCenter.X - (logo.Width / 2), startY);

            // Calculate the width of the "Press" text and Enter key texture combined
            float pressTextWidth = menuFont.MeasureString("Press ").X;
            float totalTextWidth = pressTextWidth + scaledEnterKeyWidth + (menuFont.MeasureString(" to Start").X);

            // Calculate the starting position to center the group horizontally
            float startX = screenCenter.X - totalTextWidth / 2f;

            // Calculate the position of the "Press" text (centered horizontally)
            Vector2 pressTextPosition = new Vector2(startX, logoPosition.Y + logo.Height + verticalSpacing);

            // Calculate the position of the Enter key texture (centered horizontally)
            Vector2 enterKeyPosition = new Vector2(pressTextPosition.X + pressTextWidth, pressTextPosition.Y);

            // Draw the background image stretched to fit the screen
            spriteBatch.Draw(main_menu_background, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), Color.White);

            // Draw the logo
            spriteBatch.Draw(logo, logoPosition, Color.White);

            // Calculate the alpha value for the fade effect
            Color fadeColor = new Color(1f, 1f, 1f, fadeTimer);

            // Draw the "Press" text with fade effect
            spriteBatch.DrawString(menuFont, "Press ", pressTextPosition, fadeColor);

            // Draw the Enter key texture with fade effect
            spriteBatch.Draw(enterKeyTexture, enterKeyPosition, null, fadeColor, 0f, Vector2.Zero, Globals.texture_scale_factor, SpriteEffects.None, 0f);

            // Draw the " to Start" text with fade effect
            spriteBatch.DrawString(menuFont, " to Start", new Vector2(enterKeyPosition.X + scaledEnterKeyWidth, pressTextPosition.Y), fadeColor);
        }


        public static void DrawPlaying(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, List<Projectile> projectiles)
        {
            if (!player.IsDead)
            {
                Color backgroundColor = new Color(0x25, 0x13, 0x1A); // Hex: #25131A
                graphicsDevice.Clear(backgroundColor);

                // Begin drawing with camera's transform matrix
                spriteBatch.Begin(transformMatrix: camera.TransformMatrix, samplerState: SamplerState.PointClamp);

                // Draw the tile map
                for (int x = 0; x < Globals.MapWidth; x++)
                {
                    for (int y = 0; y < Globals.MapHeight; y++)
                    {
                        Vector2 position = new Vector2(x * Globals.TileWidth * Globals.texture_scale_factor, y * Globals.TileHeight * Globals.texture_scale_factor);

                        int tileType = tileMap[x, y];

                        Texture2D tileTexture = GetTextureForTileType(tileType, x, y);

                        // Draw the tile with scaling applied
                        spriteBatch.Draw(tileTexture, position, null, Color.White, 0f, Vector2.Zero, Globals.texture_scale_factor, SpriteEffects.None, 0f);

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
                player.Draw(spriteBatch);

                foreach (Projectile projectile in projectiles)
                {
                    projectile.Draw(spriteBatch);
                }

                // Draw enemies
                foreach (Enemy enemy in enemies)
                {
                    enemy.Draw(spriteBatch);
                }

                spriteBatch.End();

                // Draw debug menu if visible
                if (Globals.debugMenuVisible)
                {
                    DrawDebugMenu(spriteBatch, defaultFont, collisionObjects, graphicsDevice, player, projectiles, camera, enemies);
                }

                // Begin a new sprite batch for UI elements
                spriteBatch.Begin(samplerState: SamplerState.PointClamp);

                // Draw the timer
                int minutes = (int)(remainingTime / 60);
                int seconds = (int)(remainingTime % 60);
                string timerText = $"{minutes:D2}:{seconds:D2}";

                // Measure the width of the text
                Vector2 textSize = Game1.defaultFont.MeasureString(timerText);

                // Calculate the position to center the text on the X axis
                float xPosition = (graphicsDevice.Viewport.Width - textSize.X) / 2;
                Vector2 position_timer = new Vector2(xPosition, 50);

                // Draw the centered text
                spriteBatch.DrawString(timerFont, timerText, position_timer, Color.White);

                string scoreText = $"{player.Score}";
                Vector2 position_score = new Vector2(50, 50);

                Globals.LastRecordedScore = player.Score;

                // Draw the centered text
                spriteBatch.DrawString(timerFont, scoreText, position_score, Color.White);

                spriteBatch.End();
            }
        }


        public static void DrawPaused(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            // Draw the paused screen
            string text = "Game Paused. Press P to Resume";
            Vector2 textSize = timerFont.MeasureString(text);
            float xPosition = (graphicsDevice.Viewport.Width - textSize.X) / 2;
            float yPosition = (graphicsDevice.Viewport.Height - textSize.Y) / 2;
            spriteBatch.DrawString(timerFont, text, new Vector2(xPosition, yPosition), Color.White);
            spriteBatch.End();
        }

        public static void DrawGameOver(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            spriteBatch.Draw(game_over_background, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), Color.White);

            string text1 = "GAME OVER";
            string text2 = "Press Enter to Return to Main Menu";
            string scoreText = $"Score: {Globals.LastRecordedScore}";

            Vector2 scoreTextSize = timerFont.MeasureString(scoreText);
            Vector2 textTextSize = headerFont.MeasureString(text1);
            Vector2 text2TextSize = timerFont.MeasureString(text2);

            float totalHeight = scoreTextSize.Y + 200 + textTextSize.Y + text2TextSize.Y + 200; // Total height of both strings plus the spacing

            // Calculate the starting Y position to center the group
            float startY = (graphicsDevice.Viewport.Height - totalHeight) / 2;

            // Draw the score text
            float xPosition = (graphicsDevice.Viewport.Width - scoreTextSize.X) / 2;
            spriteBatch.DrawString(timerFont, scoreText, new Vector2(xPosition, startY), Color.White);

            // Draw the game over text
            xPosition = (graphicsDevice.Viewport.Width - textTextSize.X) / 2;
            spriteBatch.DrawString(headerFont, text1, new Vector2(xPosition, startY + scoreTextSize.Y + 200), Color.White);

            xPosition = (graphicsDevice.Viewport.Width - text2TextSize.X) / 2;
            spriteBatch.DrawString(timerFont, text2, new Vector2(xPosition, startY + scoreTextSize.Y + 400), Color.White);

            spriteBatch.End();
        }

        public static void DrawWin(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            spriteBatch.Draw(win_background, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), Color.White);

            string text1 = "YOU SURVIVED!";
            string text2 = "Press Enter to Return to Main Menu";
            string scoreText = $"Score: {Globals.LastRecordedScore}";

            Vector2 scoreTextSize = timerFont.MeasureString(scoreText);
            Vector2 textTextSize = headerFont.MeasureString(text1);
            Vector2 text2TextSize = timerFont.MeasureString(text2);

            float totalHeight = scoreTextSize.Y + 200 + textTextSize.Y + text2TextSize.Y + 200; // Total height of both strings plus the spacing

            // Calculate the starting Y position to center the group
            float startY = (graphicsDevice.Viewport.Height - totalHeight) / 2;

            // Draw the score text
            float xPosition = (graphicsDevice.Viewport.Width - scoreTextSize.X) / 2;
            spriteBatch.DrawString(timerFont, scoreText, new Vector2(xPosition, startY), Color.White);

            // Draw the rest of text
            xPosition = (graphicsDevice.Viewport.Width - textTextSize.X) / 2;
            spriteBatch.DrawString(headerFont, text1, new Vector2(xPosition, startY + scoreTextSize.Y + 200), Color.White);

            xPosition = (graphicsDevice.Viewport.Width - text2TextSize.X) / 2;
            spriteBatch.DrawString(timerFont, text2, new Vector2(xPosition, startY + scoreTextSize.Y + 400), Color.White);

            spriteBatch.End();
        }

        public static void StartGame(GraphicsDevice graphicsDevice)
        {
            tileMap = MapLoader.LoadMapFromFile("level1_Big.txt");

            // Placeholder position of the player
            playerStartPosition = new Vector2(100, 100);

            player = new Player(playerStartPosition, graphicsDevice);

            collisionObjects = Collision.CreateCollisionObjects(graphicsDevice, tileMap);

            // Initialize camera with starting position
            camera = new Camera(graphicsDevice, Vector2.Zero);

            remainingTime = initialTime;
            timerRunning = true;

            SpawnEnemies(20);
        }

        public static void ResetGame(GraphicsDevice graphicsDevice, List<Projectile> projectiles, List<Enemy> enemies)
        {
            player = new Player(playerStartPosition, graphicsDevice);
            projectiles.Clear();
            enemies.Clear();
            remainingTime = initialTime;
            timerRunning = true;
            playerSpawn = false;
            isPaused = false;

            SpawnEnemies(20);
        }

        // Function to spawn a specified number of enemies
        public static void SpawnEnemies(int count)
        {
            Random random = new Random();
            int spawnedCount = 0;

            while (spawnedCount < count && enemies.Count < Globals.MaxEnemies)
            {
                // Generate random position within map bounds
                int x = random.Next(1, Globals.MapWidth - 1); // Ensure x is not on the boundary
                int y = random.Next(1, Globals.MapHeight - 1); // Ensure y is not on the boundary

                // Check if the tile and its neighbors are valid for spawning
                if (IsValidSpawnTile(x, y))
                {
                    // Calculate position in world coordinates
                    Vector2 position = new Vector2(x * Globals.TileWidth, y * Globals.TileHeight);

                    // Create enemy object and add to list
                    Enemy enemy = new Enemy(skeletonSpritesheet, position);
                    enemies.Add(enemy);

                    spawnedCount++;
                }
            }
        }

        // Check if a tile is valid for spawning
        private static bool IsValidSpawnTile(int x, int y)
        {
            // Check if the current tile is not a wall or empty
            if (tileMap[x, y] == 0 || tileMap[x, y] == 2 || tileMap[x, y] == 3)
                return false;

            // Check surrounding tiles to ensure they are not walls or other invalid tiles
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue; // Skip the current tile

                    int nx = x + dx;
                    int ny = y + dy;

                    // Ensure nx and ny are within bounds
                    if (nx < 0 || nx >= Globals.MapWidth || ny < 0 || ny >= Globals.MapHeight)
                        return false;

                    // Check neighboring tile
                    if (tileMap[nx, ny] == 0 || tileMap[nx, ny] == 2 || tileMap[nx, ny] == 3)
                        return false;
                }
            }

            return true;
        }


    }
}
