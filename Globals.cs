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

        public static bool debugMenuVisible = false;

        public static float player_momentum_projectile_factor = 0.6f;

        public static int default_enemy_hp = 100;

        public static float default_enemy_speed = 100;

        public static int default_player_hp = 100;

        public static int default_enemy_damage = 25;

        public static float invulnerability_time = 1.0f;

        public static int timer_in_seconds = 2*60;

        public static int MaxEnemies = 500;

        public static int LastRecordedScore = 0;
    }
}
