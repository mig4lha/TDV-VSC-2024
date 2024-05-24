using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;

namespace VSC
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public static int[,] tileMap; // Variable to store the loaded tile map

        public static Texture2D main_menu_background;
        public static Texture2D game_over_background;
        public static Texture2D win_background;
        public static Texture2D logo;
        public static Texture2D enterKeyTexture;
        public static Texture2D floor_tile;
        public static Texture2D floor_tile2;
        public static Texture2D floor_tile3;
        public static Texture2D floor_tile4;
        public static Texture2D wall_top_tile;
        public static Texture2D square_player_spawn;
        public static Texture2D player_sprite;
        public static Texture2D empty_tile;
        public static Texture2D projectileTexture;
        private Texture2D customCursorTexture;
        public static float timeSinceLastShot = 0f;

        public static Camera camera;

        public static SpriteFont defaultFont;
        public static SpriteFont timerFont;
        public static SpriteFont headerFont;

        public static List<Collision> collisionObjects;

        public List<Projectile> projectiles = new List<Projectile>();
        public static List<Enemy> enemies = new List<Enemy>();

        public static Player player;

        public static Texture2D playerSpriteSheet;
        public static Texture2D skeletonSpritesheet;
        public static Texture2D attackSpritesheet;

        public static double initialTime = Globals.timer_in_seconds; // Initial time in seconds
        public static double remainingTime;
        public static bool timerRunning;

        public static Vector2 playerStartPosition;

        public static bool playerSpawn = false;

        public static bool wasRKeyPressed = false;
        public static bool wasF3Pressed = false;
        public static bool wasPKeyPressed = false;
        public static bool wasEnterKeyPressed = false;

        public static float fadeTimer = 0f;
        public static bool fadingIn = true;
        public static float fadeSpeed = 0.8f; // Speed of the fade effect

        public static bool isPaused = false;

        public static GameState currentState;

        public static float elapsedTimeTotal = 0f;

        public static float spawnTimer = 0f; // Timer for enemy spawning
        public static float spawnInterval = 5f; // Interval in seconds between enemy spawns

        public enum GameState
        {
            MainMenu,
            Playing,
            Paused,
            GameOver,
            Win
        }

        Song MainMenu;
        Song Playing;
        Song GameOver;
        Song Win;

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

            // LoadContent before initializing player object so the texture is loaded
            LoadContent();

            // Set initial game state
            currentState = GameState.MainMenu;

            Utils.StartGame(GraphicsDevice);
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
            main_menu_background = Content.Load<Texture2D>("main_menu_background_blur");
            game_over_background = Content.Load<Texture2D>("game_over_background_blur");
            win_background = Content.Load<Texture2D>("win_background_blur");
            logo = Content.Load<Texture2D>("logo");
            enterKeyTexture = Content.Load<Texture2D>("pxkw_enter");
            floor_tile = Content.Load<Texture2D>("floor_tile");
            floor_tile2 = Content.Load<Texture2D>("floor_tile2");
            floor_tile3 = Content.Load<Texture2D>("floor_tile3");
            floor_tile4 = Content.Load<Texture2D>("floor_tile4");
            wall_top_tile = Content.Load<Texture2D>("wall_top_tile");
            square_player_spawn = Content.Load<Texture2D>("square_player_spawn");
            empty_tile = Content.Load<Texture2D>("empty_tile");
            projectileTexture = Content.Load<Texture2D>("projectile");

            defaultFont = Content.Load<SpriteFont>("TestFont");
            timerFont = Content.Load<SpriteFont>("Timer");
            headerFont = Content.Load<SpriteFont>("Header");

            customCursorTexture = Content.Load<Texture2D>("cursor");
            
            MainMenu = Content.Load<Song>("Main Menu Theme");
            Playing = Content.Load<Song>("Playing");
            GameOver = Content.Load<Song>("Game Over");
            Win = Content.Load<Song>("Win");

            playerSpriteSheet = Content.Load<Texture2D>("player_spritesheet");
            attackSpritesheet = Content.Load<Texture2D>("attack_spritesheet");
            skeletonSpritesheet = Content.Load<Texture2D>("skeleton_spritesheet");
        }

        protected override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState keyboardState = Keyboard.GetState();

            switch (currentState)
            {
                case GameState.MainMenu:
                    if (fadingIn)
                    {
                        fadeTimer += deltaTime * fadeSpeed;
                        if (fadeTimer >= 1f)
                        {
                            fadeTimer = 1f;
                            fadingIn = false;
                        }
                    }
                    else
                    {
                        fadeTimer -= deltaTime * fadeSpeed;
                        if (fadeTimer <= 0f)
                        {
                            fadeTimer = 0f;
                            fadingIn = true;
                        }
                    }
                    if (MediaPlayer.Queue.ActiveSong != MainMenu)
                    {
                        MediaPlayer.Stop();
                        MediaPlayer.IsRepeating = true;
                        MediaPlayer.Volume = 0.4f;
                        MediaPlayer.Play(MainMenu);
                    }
                    Utils.UpdateMainMenu(gameTime, GraphicsDevice);
                    break;                
                case GameState.Playing:
                    if (MediaPlayer.Queue.ActiveSong != Playing)
                    {
                        MediaPlayer.Stop();
                        MediaPlayer.IsRepeating = true;
                        MediaPlayer.Volume = 0.4f;
                        MediaPlayer.Play(Playing);
                    }
                    Utils.UpdatePlaying(gameTime, deltaTime, player, projectiles);
                    break;
                case GameState.Paused:
                    Utils.UpdatePaused(gameTime);
                    break;
                case GameState.GameOver:
                    if (MediaPlayer.Queue.ActiveSong != GameOver)
                    {
                        MediaPlayer.Stop();
                        MediaPlayer.IsRepeating = true;
                        MediaPlayer.Volume = 0.4f;
                        MediaPlayer.Play(GameOver);
                    }          
                    Utils.UpdateGameOver(gameTime);

                    if (keyboardState.IsKeyDown(Keys.Enter) && !wasEnterKeyPressed)
                    {
                        wasEnterKeyPressed = true;
                        Utils.ResetGame(GraphicsDevice, projectiles, enemies);
                        currentState = GameState.MainMenu;
                    }
                    break;
                case GameState.Win:
                    if (MediaPlayer.Queue.ActiveSong != Win)
                    {
                        MediaPlayer.Stop();
                        MediaPlayer.IsRepeating = true;
                        MediaPlayer.Volume = 0.4f;
                        MediaPlayer.Play(Win);
                    }
                    Utils.UpdateWin(gameTime);

                    if (keyboardState.IsKeyDown(Keys.Enter) && !wasEnterKeyPressed)
                    {
                        wasEnterKeyPressed = true;
                        Utils.ResetGame(GraphicsDevice, projectiles, enemies);
                        currentState = GameState.MainMenu;
                    }
                    break;
            }

            if (keyboardState.IsKeyUp(Keys.Enter))
            {
                wasEnterKeyPressed = false;
            }

            Utils.UpdateFPS(gameTime);

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            Color backgroundColor = new Color(0x25, 0x13, 0x1A); // Hex: #25131A
            GraphicsDevice.Clear(backgroundColor);

            switch (currentState)
            {
                case GameState.MainMenu:
                    _spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.NonPremultiplied);
                    Utils.DrawMainMenu(_spriteBatch, main_menu_background, logo, enterKeyTexture, timerFont, GraphicsDevice, gameTime);
                    _spriteBatch.End();
                    break;
                case GameState.Playing:
                    Utils.DrawPlaying(_spriteBatch, GraphicsDevice, projectiles);
                    break;
                case GameState.Paused:
                    Utils.DrawPaused(_spriteBatch, GraphicsDevice);
                    break;
                case GameState.GameOver:
                    Utils.DrawGameOver(_spriteBatch, GraphicsDevice);
                    Utils.ResetGame(GraphicsDevice, projectiles, enemies);
                    break;
                case GameState.Win:
                    Utils.DrawWin(_spriteBatch, GraphicsDevice);
                    Utils.ResetGame(GraphicsDevice, projectiles, enemies);
                    break;
            }

            base.Draw(gameTime);
        }
    }
}