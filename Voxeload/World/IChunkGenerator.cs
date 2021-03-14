using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.World
{
    public interface IChunkGenerator
    {
        public byte[,,,] GenerateChunk(int chunkX, int chunkY, int chunkZ);
    }
}
