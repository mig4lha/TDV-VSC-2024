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

        public static void DrawDebugMenu(SpriteBatch _spriteBatch, SpriteFont spriteFont, List<Collision> collisionObjects, GraphicsDevice _graphics, Player player)
        {
            string fpsText = $"FPS: {FPS:F2}";
            redTexture = CreateColoredTexture(_graphics, Color.Red);

            Vector2 player_screen_Pos = player.Position;

            string player_screen_Pos_string = $"X:{player_screen_Pos.X:F2} Y: {player_screen_Pos.Y:F2} ";

            _spriteBatch.Begin();

            _spriteBatch.DrawString(spriteFont, fpsText, new Vector2(10, 10), Color.White);
            _spriteBatch.DrawString(spriteFont, player_screen_Pos_string, new Vector2(10, 30), Color.White);

            // Draw collision objects
            foreach (Collision collisionObject in collisionObjects)
            {
                Rectangle bounds = collisionObject.Bounds;
                // Draw a rectangle representing the collision object's bounds
                _spriteBatch.Draw(redTexture, bounds, Color.White);
            }

            //Rectangle playerBounds = new Rectangle((int)player.Position.X, (int)player.Position.Y, 16 * (int)Globals.texture_scale_factor, 16 * (int)Globals.texture_scale_factor);
            Circle.DrawCircle(_spriteBatch, CreateCircleTexture(_graphics, player.GetBoundsRadius(), Color.White), player.Position, player.GetBoundsRadius(), Color.White);
            //_spriteBatch.Draw(CreateColoredTexture(_graphics, Color.White), playerBounds, Color.White);

            _spriteBatch.End();
        }

        public static Texture2D CreateCircleTexture(GraphicsDevice graphicsDevice, int radius, Color color)
        {
            int diameter = radius * 2;
            Texture2D texture = new Texture2D(graphicsDevice, diameter, diameter);

            Color[] data = new Color[diameter * diameter];
            float radiusSquared = radius * radius;

            for (int x = 0; x < diameter; x++)
            {
                for (int y = 0; y < diameter; y++)
                {
                    int index = x * diameter + y;
                    Vector2 position = new Vector2(x - radius, y - radius);
                    if (position.LengthSquared() <= radiusSquared)
                    {
                        data[index] = color;
                    }
                    else
                    {
                        data[index] = Color.Transparent;
                    }
                }
            }

            texture.SetData(data);
            return texture;
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
