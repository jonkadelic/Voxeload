using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Voxeload.Render;

namespace Voxeload.World
{
    public class TileFlower : TileTransparent
    {
        public override BaseTileModels TileModel { get; } = new FlatTileModels();

        public override bool Collides { get; } = false;

        public TileFlower(byte id, ITileAppearance appearance) : base(id, appearance)
        {
        }

        public override void OnUpdate(Level level, int x, int y, int z)
        {
            if (level.GetTileID(0, x, y - 1, z) == 0)
            {
                level.SetTileID(0, x, y, z, 0);
            }
        }
    }
}
