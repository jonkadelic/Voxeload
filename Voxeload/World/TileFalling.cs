using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.World
{
    public class TileFalling : Tile
    {
        public TileFalling(byte id, ITileAppearance appearance) : base(id, appearance)
        {
        }

        public TileFalling(byte id, uint texture) : base(id, texture)
        {
        }

        public override void OnUpdate(Level level, int x, int y, int z)
        {
            if (level.GetTileID(0, x, y - 1, z) == 0)
            {
                level.SetTileIDNotify(0, new(x, y, z), new(x, y, z), 0);
                level.SetTileIDNotify(0, new(x, y - 1, z), new(x, y, z), ID);
            }
        }
    }
}
