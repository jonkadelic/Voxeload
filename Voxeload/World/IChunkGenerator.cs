using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;

namespace Voxeload.World
{
    public interface IChunkGenerator
    {
        public byte[,,,] GenerateChunk(ref List<(Vector3i pos, byte tile)> structureBuffer, int chunkX, int chunkY, int chunkZ);
    }
}
