using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.World
{
    public interface ITileAccess
    {
        public byte GetTileID(int layer, int x, int y, int z);
        byte GetVisibleSides(int layer, int x, int y, int z);
        public void SetTileID(int layer, int x, int y, int z, byte id);
    }
}
