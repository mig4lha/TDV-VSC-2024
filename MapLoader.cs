using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSC
{
    public class MapLoader
    {
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
    }

}
