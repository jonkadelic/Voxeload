using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.World
{
    public interface ITileAppearance
    {
        public uint this[Tile.Face face] { get; }
    }
}
