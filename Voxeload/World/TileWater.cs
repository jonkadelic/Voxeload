using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Voxeload.Render;

namespace Voxeload.World
{
    public class TileWater : Tile
    {
        public override BaseTileModels TileModel { get; } = new WaterTileModels();

        public override int TickInterval { get; } = 1;

        public TileWater(byte id, ITileAppearance appearance) : base(id, appearance)
        {
        }

        public override void OnTick(Level level, int x, int y, int z)
        {
            if (level.GetTileID(1, x, y, z) != ID)
            {
                level.SetTileIDNotify(1, new(x, y, z), new(x, y, z), ID);
            }
            if (x - 1 > 0 && level.GetTileID(0, x - 1, y, z) == 0 && level.GetTileID(1, x - 1, y, z) == 0)
            {
                level.SetTileIDNotify(1, new(x - 1, y, z), new(x, y, z), ID);
            }
            if (x + 1 < Level.X_LENGTH * Chunk.X_LENGTH && level.GetTileID(0, x + 1, y, z) == 0 && level.GetTileID(1, x + 1, y, z) == 0)
            {
                level.SetTileIDNotify(1, new(x + 1, y, z), new(x, y, z), ID);
            }
            if (z - 1 > 0 && level.GetTileID(0, x, y, z - 1) == 0 && level.GetTileID(1, x, y, z - 1) == 0)
            {
                level.SetTileIDNotify(1, new(x, y, z - 1), new(x, y, z), ID);
            }
            if (z + 1 < Level.Z_LENGTH * Chunk.Z_LENGTH && level.GetTileID(0, x, y, z + 1) == 0 && level.GetTileID(1, x, y, z + 1) == 0)
            {
                level.SetTileIDNotify(1, new(x, y, z + 1), new(x, y, z), ID);
            }
            if (y - 1 < Level.Y_LENGTH * Chunk.Y_LENGTH && level.GetTileID(0, x, y - 1, z) == 0 && level.GetTileID(1, x, y - 1, z) == 0)
            {
                level.SetTileIDNotify(1, new(x, y - 1, z), new(x, y, z), ID);
            }
        }
    }
}
