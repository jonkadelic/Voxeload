using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;

using Voxeload.World;

namespace Voxeload.Render
{
    class ChunkToRenderComparator : IComparer<(int x, int y, int z)>
    {
        Vector3 origin;

        public ChunkToRenderComparator(Vector3 origin)
        {
            this.origin = origin;
        }

        public int Compare((int x, int y, int z) a, (int x, int y, int z) b)
        {
            Vector3 aWorld = new(a.x * Chunk.X_LENGTH + Chunk.X_LENGTH / 2, a.y * Chunk.Y_LENGTH + Chunk.Y_LENGTH / 2, a.z * Chunk.Z_LENGTH + Chunk.Z_LENGTH / 2);
            Vector3 bWorld = new(b.x * Chunk.X_LENGTH + Chunk.X_LENGTH / 2, b.y * Chunk.Y_LENGTH + Chunk.Y_LENGTH / 2, b.z * Chunk.Z_LENGTH + Chunk.Z_LENGTH / 2);

            float aDistSquared = Vector3.Distance(aWorld, origin);
            float bDistSquared = Vector3.Distance(bWorld, origin);

            return (aDistSquared.CompareTo(bDistSquared));
        }
    }
}
