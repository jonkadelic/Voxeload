using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.World
{
    public interface ILevelGenerator
    {
        public byte[,,] GenerateChunk(int x, int y, int z);
    }
}
