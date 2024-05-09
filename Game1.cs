using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
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
        private Texture2D player_sprite;
        public static Texture2D empty_tile;
        public static Texture2D projectileTexture;
        private Texture2D customCursorTexture;
        private float timeSinceLastShot = 0f;

        private SpriteFont testFont;

        private List<Collision> collisionObjects;

        List<Projectile> projectiles = new List<Projectile>();

        private Player player;

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

            tileMap = MapLoader.LoadMapFromFile("level1.txt");

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

            testFont = Content.Load<SpriteFont>("TestFont");

            customCursorTexture = Content.Load<Texture2D>("cursor");
        }

        protected override void Update(GameTime gameTime)
        {
            Utils.UpdateFPS(gameTime);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Check if the "R" key is pressed
            bool isRKeyPressed = Keyboard.GetState().IsKeyDown(Keys.R);

            // Restart player position if "R" key is released and was pressed in the previous frame
            if (wasRKeyPressed && !isRKeyPressed)
            {
                // Reset player position to the spawn location
                player.Position = playerStartPosition;
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
            Vector2 currentPlayerPosition = player.Position;

            // Update the player
            player.Update(gameTime);

            // Check for collisions between the player and walls
            foreach (Collision collisionObject in collisionObjects)
            {
                if (Collision.CollidesWithCircle(player.GetBounds(), collisionObject.GetBounds()))
                {
                    // Reset player position to the last valid position
                    player.Position = currentPlayerPosition;
                }
            }

            // Calculate time elapsed since last shot
            timeSinceLastShot += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Detect mouse left click and check if enough time has passed since last shot
            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed && timeSinceLastShot >= player.ProjectileFireRate)
            {
                Vector2 projectilePosition = player.Position;
                projectilePosition.X += 16;
                projectilePosition.Y += 16;

                // Calculate direction towards cursor
                Vector2 direction = new Vector2(mouseState.X, mouseState.Y) - projectilePosition;
                direction.Normalize();

                // Create projectile at player position with velocity in the calculated direction
                Vector2 projectileVelocity = direction * player.ProjectileSpeed;
                projectiles.Add(new Projectile(projectileTexture, projectilePosition, projectileVelocity));

                // Reset time since last shot
                timeSinceLastShot = 0f;
            }

            // Update projectiles
            foreach (Projectile projectile in projectiles)
            {
                projectile.Update();
            }

            // Remove projectiles if they move beyond a certain radius from the player
            float maxDistanceSquared =  Projectile.DespawnDistance * Projectile.DespawnDistance; // 500px radius
            projectiles.RemoveAll(p => Vector2.DistanceSquared(p.Position, player.Position) > maxDistanceSquared);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Color backgroundColor = new Color(0x25, 0x13, 0x1A); // Hex: #25131A
            GraphicsDevice.Clear(backgroundColor);

            // Calculate offset to center the map on the screen
            int offsetX = (GraphicsDevice.Viewport.Width - (Globals.MapWidth * Globals.TileWidth * (int)Globals.texture_scale_factor)) / 2;
            int offsetY = (GraphicsDevice.Viewport.Height - (Globals.MapHeight * Globals.TileHeight * (int)Globals.texture_scale_factor)) / 2;

            _spriteBatch.Begin();

            //_spriteBatch.DrawString(testFont, "FPS: " + Utils.FPS.ToString("0.00"), new Vector2(10, 10), Color.White);

            // Draw the tile map
            for (int x = 0; x < Globals.MapWidth; x++)
            {
                for (int y = 0; y < Globals.MapHeight; y++)
                {
                    Vector2 position = new Vector2(offsetX + x * Globals.TileWidth * Globals.texture_scale_factor, offsetY + y * Globals.TileHeight * Globals.texture_scale_factor);

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

            _spriteBatch.End();

            // Draw debug menu if visible
            if (Globals.debugMenuVisible)
            {
                Utils.DrawDebugMenu(_spriteBatch, testFont, collisionObjects, GraphicsDevice, player);
            }

            base.Draw(gameTime);
        }

    }
}
