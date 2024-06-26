# Skull Attack

Francisco Pinheiro Ribeiro a27942  
José Miguel Cunha a22550

---

## Introdução

Iremos apresentar o código do nosso jogo ***Skull Attack***, uma recriação do jogo designado por ***Vampire Survivors***. Nesta apresentação vamos mostrar não só jogo em si como também o código usado para refazer este jogo.

## O que é *Skull Attack*

*Skull Attack* é um *roguelike shoot'em up* onde o jogador luta contra rondas, num perídodo de tempo, de inimigos para tentar ganhar o máximo número de pontos. O jogo em si tem estilo muito minimalista num setting gótico.

---

## Código

Para este jogo, usamos as seguintes classes:  
- ***Globals.cs***;  
- ***MapLoader.cs***;  
- ***Utils.cs***;
- **SpriteAnimation**;   
- ***Camera.cs***;  
- ***Circle.cs***;  
- ***Collision.cs***;  
- ***Player.cs***;  
- ***Projectile.cs***;  
- ***Enemy.cs***;  

---

### Globals.cs

Esta classe é responsável por todas as variáveis globais que serão utilizadas no código para executar o jogo. Coisas como a velocidade dos projéteis, a vida do jogador e dos inimigos assim como a velocidade, etc.

```C#

internal class Globals
{
  // Tile map dimensions
  public static int MapWidth = 0;
  public static int MapHeight = 0;

  // Tile size in pixels
  public const int TileWidth = 16;
  public const int TileHeight = 16;

  public static float texture_scale_factor = 3.0f;

  public static float cursor_texture_scale_factor = 3.0f;

  public static bool debugMenuVisible = false;

  public static float player_momentum_projectile_factor = 0.6f;

  public static int default_enemy_hp = 100;

  public static float default_enemy_speed = 100;

  public static int default_player_hp = 100;

  public static int default_enemy_damage = 25;

  public static float invulnerability_time = 1.0f;

  public static int timer_in_seconds = 2*60;

  public static int MaxEnemies = 500;
}
```

---

### MapLoader.cs

Esta classe trata da leitura de ficheiros e criação de mapas com base nesses ficheiro. 

```C#
  public int[,] LoadMap(string filePath)
  {
    // Default tile map with dimensions 1x1
    int[,] defaultTileMap = new int[1, 1] { { 0 } };

    try
    {
      // Read the text file
      string[] lines = File.ReadAllLines(filePath);

      // Get the dimensions of the map
      int width = lines[0].Length;
      int height = lines.Length;

      Globals.MapWidth = width;
      Globals.MapHeight = height;

      // Initialize the tile map
      int[,] tileMap = new int[width, height];

      // Parse the characters in the text file to populate the tile map
      for (int y = 0; y < height; y++)
      {
        for (int x = 0; x < width; x++)
        {
          char tileChar = lines[y][x];
          int tileType = 0; // Default to empty tile

          switch (tileChar)
          {
            case 'w': // Wall
              tileType = 2;
              break;
            case 'p': // Wall
              tileType = 3;
              break;
            case 'f': // Floor
              tileType = 1;
              break;
            default:  // Empty
              tileType = 0;
              break;
          }

          tileMap[x, y] = tileType;
        }
      }
                
      return tileMap;
    }
    catch (FileNotFoundException ex)
    {
      Console.WriteLine("The file could not be found: " + ex.Message);
    }
    catch (IOException ex)
    {
      Console.WriteLine("An error occurred while reading the file: " + ex.Message);
    }

    // Return the default tile map if an error occurred
    return defaultTileMap;
  }
```

A secção em cima trata da leitura dos ficheiros de forma a poder criar um mapa com base em letras usadas para representar cada tile do mapa.

```C#
  public static int[,] LoadMapFromFile(string fileName)
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

    return tileMap;
  }
```

Esta secção trata da criação da instância *MapLoader* de forma a poder criar o mapa.

---

### Utils.cs

Esta classe trata de todos os menus assim como as diferentes texturas usadas no jogo assim como atualiza.

```C#
private static int frameCount;
private static float elapsedTime;
private static float fps;

public static Texture2D redTexture;

public static Texture2D[,] selectedFloorTextures;

public static float FPS => fps;
```

Todas variáveis globais a ser usadas nesta classe.

```C#
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
```

- **UpdateFPS** - está em carregue de atualizar o contador de *frames per second*;

```C#
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
```

- **DrawDebugMenu** - é uma função maioritariamente usada para nós vermos se as colisões coincidem com os objetos em si.

```C#
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
```

- **CreateCircleTexture** - trata da criação de círculos usados no debug menu.  
- **DrawCircle** - dá-nos uma representação visual dos círculos criados.

```C#
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
```

- **CreateRectangleTexture** - trata da criação de retângulos usados no debug menu.

```C#
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
```

- **ScaleCursorTexture** - trata da criação do cursor usado pelo jogador para poder apontar os ataques.

```C#
public static Texture2D CreateColoredTexture(GraphicsDevice graphicsDevice, Color color)
{
  Texture2D texture = new Texture2D(graphicsDevice, 1, 1);

  texture.SetData(new[] { color });
  return texture;
}
```

- **CreateColoredTexture** - função extra usada para a criação de texturas

```C#
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
```

- **GetTextureForTileType** - trata da creação do chão do mapa.

```C#
public static void UpdateMainMenu(GameTime gameTime, GraphicsDevice graphicsDevice)
{
  // Handle menu input and transitions
  KeyboardState keyboardState = Keyboard.GetState();

  if (keyboardState.IsKeyDown(Keys.Enter))
  {
    currentState = GameState.Playing;
  }
}      
```

- **UpdateMainMenu** - atualiza o menu quando o jogador preciona no *Enter* para começar a jogar.

```C#
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
```

- **UpdatePlaying** - trata de verificar colisões e atualizar o jogo

```C#
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
```

- **UpdatePaused** - trata de verificar se o jogo é pausado e se o jogador continua.

```C#
public static void UpdateGameOver(GameTime gameTime)
{
  // Handle game over input and transitions
  KeyboardState keyboardState = Keyboard.GetState();

  if (keyboardState.IsKeyDown(Keys.Enter) && !wasEnterKeyPressed)
  {
    currentState = GameState.MainMenu;
  }
  wasEnterKeyPressed = keyboardState.IsKeyDown(Keys.Enter);
}
```

- **UpdateGameOver** - atualiza o jogo para voltar ao *Main Menu*.

```C#
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
```

- **UpdateWin** - verifica se o jogador ganha.

```C#
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
```

- **DrawMainMenu** - dá ao jogador uma representação visual do *Main Menu*.

```C#
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
```

- **DrawPlaying** - gera o conteúdo do jogo para o jogador podr ver.

```C#
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
```

- **DrawPaused** - apresenta ao jogador o menu de pausa.

```C#
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
```

- **DrawGameOver** - apresenta ao jogador o ecra *Game Over*.

```C#
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
```

- **DrawWin** - Apresenta ao jogador quando ele ganha

```C#
public static void StartGame(GraphicsDevice graphicsDevice)
{
  tileMap = MapLoader.LoadMapFromFile("level1_Big.txt");

  // Placeholder position of the player
  playerStartPosition = new Vector2(100, 100);

  player = new Player(player_sprite, playerStartPosition);

  collisionObjects = Collision.CreateCollisionObjects(graphicsDevice, tileMap);

  // Initialize camera with starting position
  camera = new Camera(graphicsDevice, Vector2.Zero);

  remainingTime = initialTime;
  timerRunning = true;

  SpawnEnemies(20);
}
```

- **StartGame** - inicializa o jogo quando o jogador começa.

```C#
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
```

- **ResetGame** - Dá reset do jogo de forma a poder começar de novo.

```C#
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
      Enemy enemy = new Enemy(skeleton_texture, position);
      enemies.Add(enemy);

      spawnedCount++;
    }
  }
}
```

- **SpawnEnemies** - trata da adição d inimigos no jogo.

```C#
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
```

- **IsValidSpawn** - verifica se o espaço está vazio para adicionar inimigos.

---

### SpriteAnimation.cs

Esta classe está encarregue de desenhar e rodar as animações do jogo

```C#
private Texture2D spriteSheet;
        private int frameWidth;
        private int frameHeight;
        private int frameStart;
        private int frameEnd;
        private float frameTime;
        private int currentFrameIndex;
        private float timer;

        public SpriteAnimation(Texture2D spriteSheet, int frameWidth, int frameHeight, int frameStart, int frameEnd, float frameTime)
        {
            this.spriteSheet = spriteSheet;
            this.frameWidth = frameWidth;
            this.frameHeight = frameHeight;
            this.frameStart = frameStart;
            this.frameEnd = frameEnd;
            this.frameTime = frameTime;
            this.currentFrameIndex = frameStart;
            this.timer = 0f;
        }
```

Variáveis e construtores usados para a classe.

```C#
public void Update(float deltaTime)
        {
            timer += deltaTime;

            if (timer > frameTime)
            {
                currentFrameIndex = (currentFrameIndex + 1) % (frameEnd - frameStart + 1) + frameStart;
                timer = 0f;
            }
        }
```

- **Update** - vai atualizando o jogo enquanto está a ser jogado.

```C#
public void Draw(SpriteBatch spriteBatch, Vector2 position, bool isPlayer, float Rotation, Vector2 Origin, bool isEnemy)
        {
            int frameX = currentFrameIndex % (spriteSheet.Width / frameWidth);
            int frameY = currentFrameIndex / (spriteSheet.Width / frameWidth);
            Rectangle sourceRectangle = new Rectangle(frameX * frameWidth, frameY * frameHeight, frameWidth, frameHeight);
            if (isPlayer)
            {
                position.X -= 48;
                position.Y -= 48;
                if (Player.IsFacingLeft)
                {
                    spriteBatch.Draw(spriteSheet, position, sourceRectangle, Color.White, 0f, Vector2.Zero, Globals.texture_scale_factor, SpriteEffects.FlipHorizontally, 0f);
                } else
                {
                    spriteBatch.Draw(spriteSheet, position, sourceRectangle, Color.White, 0f, Vector2.Zero, Globals.texture_scale_factor, SpriteEffects.None, 0f);
                }
                
            } else if (isEnemy)
            {
                spriteBatch.Draw(spriteSheet, position, sourceRectangle, Color.White, 0f, Vector2.Zero, Globals.texture_scale_factor, SpriteEffects.None, 0f);
            } 
            else
            {
                position.X -= 16;
                position.Y -= 16;
                spriteBatch.Draw(spriteSheet, position, sourceRectangle, Color.White, Rotation, Origin, 1.5f, SpriteEffects.None, 0f);
            }
        }
```

- **Draw** - desenha os sprites.

---

### Camera.cs

Esta classe trata de tudo em relação à camara que segue o jogador.

```C#
 private GraphicsDevice graphicsDevice;

public Vector2 Position { get; private set; }
public Matrix TransformMatrix => Matrix.CreateTranslation(-Position.X, -Position.Y, 0);
```

Globais necessárias para a classe.

```C#
public Camera(GraphicsDevice graphicsDevice, Vector2 initialPosition)
{
  this.graphicsDevice = graphicsDevice;
  Position = initialPosition;
}
```

- **Camera** - prepara a cámara, centralizando-a ao jogador.

```C#
public void Follow(Vector2 targetPosition)
{
  // Adjust the camera's position to keep the target centered on the screen
  Position = targetPosition - new Vector2(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);
}
```

- **Follow** - garante que a cámara siga o jogador no decorrer do jogo.

---

### Circle.cs

Gera uma classe que é utilizada maioritariamente nas colisões.

```C#
public Vector2 Center { get; set; }
public float Radius { get; set; }

public Circle(Vector2 center, float radius)
{
  Center = center;
  Radius = radius;
}
```

Criação da variável para ser usada em outras funções.

```C#
public bool Contains(Vector2 point)
{
  float distanceSquared = Vector2.DistanceSquared(Center, point);
  return distanceSquared <= (Radius * Radius);
}
```

- **Contains** - verifica se algo está contido dentro do círculo.

```C#
public bool Intersects(Rectangle rectangle, Circle playerBounds)
{
  // Find the closest point to the circle within the rectangle
  float closestX = MathHelper.Clamp(playerBounds.Center.X, rectangle.Left, rectangle.Right);
  float closestY = MathHelper.Clamp(playerBounds.Center.Y, rectangle.Top, rectangle.Bottom);

  // Calculate the distance between the circle's center and this closest point
  float distanceX = playerBounds.Center.X - closestX;
  float distanceY = playerBounds.Center.Y - closestY;

  // If the distance is less than the circle's radius, there's an intersection
  float distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);
  return distanceSquared < (Radius * Radius);
}
```

- **Intersects** - verifica se algo interseta o círculo.

```C#
public static void DrawCircle(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, int radius, Color color)
{
  position.X += radius;
  position.Y += radius;
  spriteBatch.Draw(texture, position - new Vector2(radius), null, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
}
```

- **DrawCircle** - cria o círculo para ser usado nas colisiões.

---

### Collision.cs

Trata de tudo a envolver com as colisões do jogador com o resto do jogo.

```C#
public Rectangle Bounds { get; private set; }

public Collision(Rectangle bounds)
{
  Bounds = bounds;
}
```

Criação da variável para ser usada nas colisões.

```C#
public static bool Collides(Circle circle, Rectangle rect)
{
  // Find the closest point on the rectangle to the circle's center
  float closestX = MathHelper.Clamp(circle.Center.X, rect.Left, rect.Right);
  float closestY = MathHelper.Clamp(circle.Center.Y, rect.Top, rect.Bottom);

  // Calculate the distance between the circle's center and this closest point
  float distanceX = circle.Center.X - closestX;
  float distanceY = circle.Center.Y - closestY;

  // If the distance is less than the circle's radius, an intersection occurs
  float distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);
  return distanceSquared < (circle.Radius * circle.Radius);
}
```

- **Collides** - trata das colisões entre retângulos e círculos.

```C#
public bool CollidesWith(Rectangle otherBounds)
{
  return Bounds.Intersects(otherBounds);
}
```

- **CollidesWith** - trata das colisões entre retângulos.

```C#
public static bool CircleCircleCollision(Circle circle1, Circle circle2)
{
  float distanceSquared = Vector2.DistanceSquared(circle1.Center, circle2.Center);
  float radiusSumSquared = (circle1.Radius + circle2.Radius) * (circle1.Radius + circle2.Radius);
  return distanceSquared <= radiusSumSquared;
}
```

- **CircleCircleCollision** - trata das colisões entre círculos.

```C#
public static List<Collision> CreateCollisionObjects(GraphicsDevice GraphicsDevice, int[,] tileMap)
{
  List<Collision> collisionObjects = new List<Collision>();

  // Calculate offset to center the map on the screen
  int offsetX = (GraphicsDevice.Viewport.Width - (Globals.MapWidth * Globals.TileWidth * (int)Globals.texture_scale_factor)) / 2;
  int offsetY = (GraphicsDevice.Viewport.Height - (Globals.MapHeight * Globals.TileHeight * (int)Globals.texture_scale_factor)) / 2;
  offsetX = 0;
  offsetY = 0;

  // Iterate over tileMap to find collision objects and create corresponding objects
  for (int x = 0; x < Globals.MapWidth; x++)
  {
    for (int y = 0; y < Globals.MapHeight; y++)
    {
      // Identify the type of collision object based on the tile value
      int tileType = tileMap[x, y];

      // Check if the tile represents a collision object
      if (IsCollisionObject(tileType))
      {
        // Calculate position for the collision object without applying scale factor
        int xPos = offsetX + x * Globals.TileWidth * (int)Globals.texture_scale_factor;
        int yPos = offsetY + y * Globals.TileHeight * (int)Globals.texture_scale_factor;
        Rectangle boundsFullTile = new Rectangle(xPos, yPos, Globals.TileWidth * (int)Globals.texture_scale_factor, Globals.TileHeight * (int)Globals.texture_scale_factor);

        // Create a collision object based on its type
        Collision collisionObject;
        switch (tileType)
        {
          case 2: // Wall
            Rectangle bounds = new Rectangle(xPos, yPos, Globals.TileWidth * (int)Globals.texture_scale_factor, Globals.TileHeight * (int)Globals.texture_scale_factor);
            collisionObject = new Collision(bounds);
            break;
          // Add more cases for other types of collision objects
          default:
            // Handle other types of collision objects
            collisionObject = new Collision(boundsFullTile);
            break;
        }

        // Add the collision object to the list
        collisionObjects.Add(collisionObject);
      }
    }
  }

  return collisionObjects;
}
```

- **CreateCollisionObjects** - trata das colisões entre o jogador e as paredes do mapa.

---

### Player.cs

Trata de tudo que precisamos para o jogador, atribuir valores à velocidade, vida, etc. assim como interações do jogador com o resto do jogo.

```C#
public Vector2 Position { get; set; }
public Vector2 Velocity { get; set; }
public Circle Bounds { get; private set; }

public float ProjectileSpeed { get; set; }
public float ProjectileFireRate { get; set; }
public int DamagePerShot { get; set; }

public int MaxHealth { get; private set; }
public int CurrentHealth { get; private set; }
public bool IsDead { get; private set; }
public static bool IsFacingLeft { get; private set; }
public int Score { get; private set; }

// Player texture
private Texture2D texture;

// Player Speed
private float Speed = 300f;

// Health bar dimensions
private const int HealthBarWidth = 50;
private const int HealthBarHeight = 5;

private Texture2D healthBarGreenTexture;
private Texture2D healthBarRedTexture;

private bool isInvulnerable;
private float invulnerabilityDuration = Globals.invulnerability_time; // Duration in seconds
private float invulnerabilityTimer;

private SpriteAnimation idleAnimation;
private SpriteAnimation walkAnimation;
private SpriteAnimation deathAnimation;
private SpriteAnimation currentAnimation;

// Constructor
public Player(Vector2 position, GraphicsDevice graphicsDevice)
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

  // Create health bar textures
  healthBarGreenTexture = Utils.CreateRectangleTexture(graphicsDevice, HealthBarWidth, HealthBarHeight, Color.Green);
  healthBarRedTexture = Utils.CreateRectangleTexture(graphicsDevice, HealthBarWidth, HealthBarHeight, Color.Red);

  idleAnimation = new SpriteAnimation(Game1.playerSpriteSheet, 48, 48, 0, 5, 0.1f);
  walkAnimation = new SpriteAnimation(Game1.playerSpriteSheet, 48, 48, 16, 21, 0.1f);
  deathAnimation = new SpriteAnimation(Game1.playerSpriteSheet, 48, 48, 88, 90, 0.1f);

  currentAnimation = idleAnimation;
}
```

Variáveis e construtores da classe.
 
```C#
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

        if (IsDead)
        {
            return;
        }

        // Get the keyboard state
        KeyboardState keyboardState = Keyboard.GetState();

        bool IsMoving = false;

        // Calculate player velocity based on keyboard input
        Vector2 newVelocity = Vector2.Zero;
        if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
        {
            newVelocity.X -= Speed; // Move left
            IsMoving = true;
            IsFacingLeft = true;
        }
        if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
        {
            newVelocity.X += Speed; // Move right
            IsMoving = true;
            IsFacingLeft = false;
        }
        if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W))
        {
            newVelocity.Y -= Speed; // Move up
            IsMoving = true;
        }
        if (keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S))
        {
            newVelocity.Y += Speed; // Move down
            IsMoving = true;
        }

        if (IsMoving)
        {
            // Set animation to walk
            currentAnimation = walkAnimation;
        }
        else
        {
            // Set animation to idle
            currentAnimation = idleAnimation;
        }

        // Update current animation
        currentAnimation.Update(deltaTime);

        // Update the velocity
        Velocity = newVelocity;

        // Update the position based on velocity
        Position += Velocity * deltaTime;

        UpdateBounds();
    }
```

- **Update** - vai atualizando o estado do jogador (se está a mexer, atacar, etc).

```C#
public void UpdateBounds()
{
  // Assuming the player's texture size is 16x16
  int radius = (int)(8 * Globals.texture_scale_factor);
  Vector2 center = Position + new Vector2(texture.Width / 2, texture.Height / 2) * Globals.texture_scale_factor;
  Bounds = new Circle(center, radius);
}
```

- **UpdateBounds** - atualiza os limites do jogador.

```C#
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
```

- **TakeDamage** - atualiza a vida do jogador e avisa quando o jogador morre assim como quando está invulnerável.

```C#
public void IncrementScore(int amount)
{
  Score += amount;
}
```

- **IncrementScore** - atualiza o score do jogador.

```C#
public void Draw(SpriteBatch spriteBatch)
    {
        if (IsDead == true) return; // Don't draw if the player is dead

        //spriteBatch.Draw(texture, Position, null, Color.White, 0f, Vector2.Zero, Globals.texture_scale_factor, SpriteEffects.None, 0f);
        currentAnimation.Draw(spriteBatch, Position, true, 0f, Vector2.Zero, false);

        // Draw health bar above the player
        Vector2 healthBarPosition = new Vector2(Position.X, Position.Y - 20);

        // Calculate health bar widths
        float healthPercentage = (float)CurrentHealth / MaxHealth;
        int healthBarGreenWidth = (int)(HealthBarWidth * healthPercentage);
        int healthBarRedWidth = HealthBarWidth - healthBarGreenWidth;

        // Draw green part of health bar
        Rectangle healthBarGreenRect = new Rectangle((int)healthBarPosition.X, (int)healthBarPosition.Y, healthBarGreenWidth, HealthBarHeight);
        spriteBatch.Draw(healthBarGreenTexture, healthBarGreenRect, new Rectangle(0, 0, healthBarGreenWidth, HealthBarHeight), Color.White);

        // Draw red part of health bar (if any)
        if (healthBarRedWidth > 0)
        {
            Rectangle healthBarRedRect = new Rectangle((int)healthBarPosition.X + healthBarGreenWidth, (int)healthBarPosition.Y, healthBarRedWidth, HealthBarHeight);
            spriteBatch.Draw(healthBarRedTexture, healthBarRedRect, new Rectangle(0, 0, healthBarRedWidth, HealthBarHeight), Color.White);
        }
    }
```

- **Draw** - cria o personagem do jogador, com uma barra de hp.

---

### Projectile.cs

Esta classe está em carregue da criação e interação dos projéteis do jogador com o resto do jogo.

```C#
public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public static int DespawnDistance { get; set; }
        public Texture2D Texture { get; set; }
        public Circle Bounds { get; private set; } // Hitbox for the projectile
        public SpriteAnimation Animation { get; set; }
        public float Rotation { get; private set; } // Rotation angle in radians
        public Vector2 Origin { get; private set; } // Origin vector to keep projectile centered in initial position after rotation

        public Projectile(Texture2D texture, Vector2 position, Vector2 velocity, Vector2 directionToCursor)
        {
            Texture = texture;
            Position = position;
            Velocity = velocity;

            // Calculate rotation angle
            Rotation = (float)Math.Atan2(-directionToCursor.Y, -directionToCursor.X);

            DespawnDistance = 3000;

            // Define bounds with a radius matching half of the projectile texture's width
            Bounds = new Circle(position, texture.Width);

            Animation = new SpriteAnimation(Game1.attackSpritesheet, 32, 32, 0, 3, 0.1f);
        }
```

Criação da variável que representará o projétil do jogador.

```C#
public void Update(float deltaTime)
        {
            // Move the projectile based on its velocity and the elapsed time
            Position += Velocity * deltaTime;

            // Update the position of the bounds
            Bounds = new Circle(Position, Bounds.Radius);

            Animation.Update(deltaTime); // Update animation frame
        }
```

- **Update** - atualiza a posição dos projéteis.

```C#
public int GetBoundsRadius()
{
  return (int)Bounds.Radius;
}
```

- **GetBoundsRadius** - atualiza as colisões dos projéteis.

```C#
public void Draw(SpriteBatch spriteBatch)
        {
            Origin = new Vector2(Texture.Width, Texture.Height);
            // Draw the projectile's texture centered at the projectile's position
            Animation.Draw(spriteBatch, Position, false, Rotation, Origin, false);
        }
```

- **Draw** - cria o projétil de forma a ser vísivel ao jogador.

---

### Enemy.cs

Está encarregue de criar os inímigos assim como outras funções.

```C#
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
```

Criação das propriedades do inimigo.

```C#
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
```

- **Update** - atualiza a posição e colisão do inimigo.

```C#
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
```

- ***TakeDamage*** - atualiza a vida do inimigo assim como o estado deles (vivos ou mortos).

```C#
public void Draw(SpriteBatch spriteBatch)
    {
        Vector2 newPosition = Position;
        Animation.Draw(spriteBatch, newPosition, false, 0f, Vector2.Zero, true);
        //spriteBatch.Draw(Texture, Position, null, Color.White, 0f, Vector2.Zero, Globals.texture_scale_factor, SpriteEffects.None, 0f);
    }
```

- **Draw** - apresenta o inimigo de forma visível ao jogador.

--- 

### Game1.cs

```C#
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
```

Tudo qué preciso para iniciar o jogo assim como formas de identificar o estado jogo.

```C#
protected override void Initialize()
{
  Utils.selectedFloorTextures = new Texture2D[Globals.MapWidth, Globals.MapHeight];

  MouseCursor customCursor = Utils.ScaleCursorTexture(GraphicsDevice, customCursorTexture, Globals.cursor_texture_scale_factor);

  // Set the custom cursor
  Mouse.SetCursor(customCursor);

  base.Initialize();
}
```

- **Initialize** - inicia o jogo.

```C#
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
```

- **LoadContent** - carrega todo o ceonteúdo necessário para criar o jogo.

```C#
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
```

- **Update** - vai atualizando o estado do jogo.

```C#
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
```

- **Draw** - apresenta o jogo dependendo do seu estado
