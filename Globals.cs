using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSC
{
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

        public static bool debugMenuVisible = true;

        public static float player_momentum_projectile_factor = 0.2f;
    }
}
