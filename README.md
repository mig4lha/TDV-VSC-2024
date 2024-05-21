# Vampire Survivors

Francisco Pinheiro Ribeiro a27942  
José Miguel Cunha a22550

---

## Introdução

Iremos apresentar o código do nosso jogo ***Vampire Survivors***, uma recriação do jogo designado pelo mesmo título. Nesta apresentação vamos mostrar o código usado para recriar este roguelike.

Para este jogo, usamos as seguintes classes:
-***Globals.cs***;
-***MapLoader.cs***;
-***Utils.cs***;
-***Camera.cs***;
-***Circle.cs***;
-***Collision.cs***;
-***Player.cs***;
-***Projectile.cs***;
-***Enemy.cs***;

---

###Globals.cs

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

###MapLoader.cs

Esta classe trata da leitura de ficheiros e criação de mapas com base nesses ficheiro. 

```
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

```
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

###Utils.cs

Esta classe trata de todos os menus assim como as diferentes texturas usadas no jogo assim como atualiza.

```
private static int frameCount;
private static float elapsedTime;
private static float fps;

public static Texture2D redTexture;

public static Texture2D[,] selectedFloorTextures;

public static float FPS => fps;
```

Todas variáveis globais a ser usadas nesta classe.

```
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

-**UpdateFPS** - está em carregue de atualizar o contador de *frames per second*;

```
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

-**DrawDebugMenu** - é uma função maioritariamente usada para nós vermos se as colisões coincidem com os objetos em si.

```
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

-**CreateCircleTexture** - trata da criação de círculos usados no debug menu.  
-**DrawCircle** - dá-nos uma representação visual dos círculos criados.

```
public static Texture2D CreateRectangleTexture(GraphicsDevice graphicsDevice, int width, int height, Color color)
{
  Texture2D rectangleTexture = new Texture2D(graphicsDevice, width, height);
  Color[] data = new Color[width * height];
  for (int i = 0; i < data.Length; ++i) data[i] = color;
  rectangleTexture.SetData(data);
  return rectangleTexture;
}
```

-**CreateRectangleTexture** - trata da criação de retângulos usados no debug menu.

```
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

-**ScaleCursorTexture** - trata da criação do cursor usado pelo jogador para poder apontar os ataques.

```
public static Texture2D CreateColoredTexture(GraphicsDevice graphicsDevice, Color color)
{
  Texture2D texture = new Texture2D(graphicsDevice, 1, 1);

  texture.SetData(new[] { color });
  return texture;
}
```

-**CreateColoredTexture** - função extra usada para a criação de texturas

```
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

-**GetTextureForTileType** - trata da creação do chão do mapa.

```
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

-**UpdateMainMenu** - atualiza o menu quando o jogador preciona no *Enter*

