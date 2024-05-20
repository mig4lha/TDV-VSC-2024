using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

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
    }
}
