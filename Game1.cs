using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace VSC
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private bool _isHandlingResize = false;

        private int[,] tileMap; // Variable to store the loaded tile map

        private Texture2D floor_tile;
        private Texture2D floor_tile2;
        private Texture2D floor_tile3;
        private Texture2D floor_tile4;
        private Texture2D wall_top_tile;
        private Texture2D wall_bottom_tile;
        private Texture2D wall_left_tile;
        private Texture2D wall_right_tile;
        private Texture2D wall_right_corner_tile;
        private Texture2D wall_left_corner_tile;
        private Texture2D empty_tile;

        private float tile_scale_factor = 2.0f;

        //private int[,] tileMap = new int[Globals.MapWidth, Globals.MapHeight]
        //{
        //    // Example tile map data (IDs)
        //    { 7, 2, 2, 2, 2, 2, 2, 2, 2, 3 }, 
        //    { 7, 1, 1, 1, 1, 1, 1, 1, 1, 3 }, 
        //    { 7, 1, 1, 1, 1, 1, 1, 1, 1, 3 }, 
        //    { 7, 1, 1, 1, 1, 1, 1, 1, 1, 3 }, 
        //    { 7, 1, 1, 1, 1, 1, 1, 1, 1, 3 }, 
        //    { 7, 1, 1, 1, 1, 1, 1, 1, 1, 3 }, 
        //    { 7, 1, 1, 1, 1, 1, 1, 1, 1, 3 }, 
        //    { 7, 1, 1, 1, 1, 1, 1, 1, 1, 3 }, 
        //    { 7, 1, 1, 1, 1, 1, 1, 1, 1, 3 }, 
        //    { 6, 5, 5, 5, 5, 5, 5, 5, 5, 4 }  
        //};

        private Texture2D[,] selectedFloorTextures;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.IsFullScreen = true;
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.ApplyChanges();
            Window.AllowUserResizing = true;

            LoadMapFromFile("level1.txt");
        }

        protected override void Initialize()
        {
            selectedFloorTextures = new Texture2D[Globals.MapWidth, Globals.MapHeight];

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
            wall_bottom_tile = Content.Load<Texture2D>("wall_bottom_tile");
            wall_left_tile = Content.Load<Texture2D>("wall_left_tile");
            wall_right_tile = Content.Load<Texture2D>("wall_right_tile");
            wall_right_corner_tile = Content.Load<Texture2D>("wall_right_corner_tile");
            wall_left_corner_tile = Content.Load<Texture2D>("wall_left_corner_tile");
            empty_tile = Content.Load<Texture2D>("empty_tile");
        }

        private void LoadMapFromFile(string fileName)
        {
            string workingDirectory = Environment.CurrentDirectory;
            // Get the current directory
            string currentDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;

            // Combine the current directory with the relative path to the maps directory and the specific level file name
            string filePath = Path.Combine(currentDirectory, "maps", fileName);

            // Create an instance of MapLoader
            MapLoader mapLoader = new MapLoader();

            // Load the map from the file
            tileMap = mapLoader.LoadMap(filePath);
            Console.WriteLine(tileMap);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Color backgroundColor = new Color(0x25, 0x13, 0x1A); // Hex: #25131A
            GraphicsDevice.Clear(backgroundColor);

            // Calculate offset to center the map on the screen
            int offsetX = (GraphicsDevice.Viewport.Width - (Globals.MapWidth * Globals.TileWidth * (int)Globals.tile_scale_factor)) / 2;
            int offsetY = (GraphicsDevice.Viewport.Height - (Globals.MapHeight * Globals.TileHeight * (int)Globals.tile_scale_factor)) / 2;

            _spriteBatch.Begin();

            // Draw the tile map
            for (int x = 0; x < Globals.MapWidth; x++)
            {
                for (int y = 0; y < Globals.MapHeight; y++)
                {
                    Vector2 position = new Vector2(offsetX + x * Globals.TileWidth * Globals.tile_scale_factor, offsetY + y * Globals.TileHeight * Globals.tile_scale_factor);

                    int tileType = tileMap[x, y];

                    Texture2D tileTexture = GetTextureForTileType(tileType, x, y);

                    // Draw the tile with scaling applied
                    _spriteBatch.Draw(tileTexture, position, null, Color.White, 0f, Vector2.Zero, Globals.tile_scale_factor, SpriteEffects.None, 0f);
                }
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }


        private Texture2D GetTextureForTileType(int tileType, int x, int y)
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
                                selectedFloorTextures[x, y] = floor_tile;
                                break;
                            case 2:
                                selectedFloorTextures[x, y] = floor_tile2;
                                break;
                            case 3:
                                selectedFloorTextures[x, y] = floor_tile3;
                                break;
                            case 4:
                                selectedFloorTextures[x, y] = floor_tile4;
                                break;
                            default:
                                selectedFloorTextures[x, y] = floor_tile;
                                break;
                        }
                    }
                    return selectedFloorTextures[x, y];
                case 2: // Top wall
                    return wall_top_tile;
                //case 3: // Bottom wall
                //    return wall_bottom_tile;
                //case 4: // Left wall
                //    return wall_left_tile;
                //case 5: // Right wall
                //    return wall_right_tile;
                //case 6: // Top Left Corner wall
                //    return wall_left_tile;
                //case 7: // Top Right wall
                //    return wall_right_tile;
                //case 8: // Bottom Left wall
                //    return wall_left_corner_tile;
                //case 9: // Bottom Right wall
                //    return wall_right_corner_tile;
                default:
                    return empty_tile;
            }
        }


    }
}
