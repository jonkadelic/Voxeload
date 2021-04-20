using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.World
{
    public class TileTransparent : Tile
    {
        public TileTransparent(byte id, ITileAppearance appearance) : base(id, appearance)
        {
        }
    }
}
