using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Voxeload.World
{
    public class ChunkGenExchangeQueue : ExchangeQueue<Vector3i, Chunk>
    {
        protected Level level;
        protected IChunkGenerator generator;

        public ChunkGenExchangeQueue(Level level, IChunkGenerator generator) : base(ThreadPriority.AboveNormal)
        {
            this.level = level;
            this.generator = generator;
        }

        protected override Chunk Process(Vector3i pos)
        {
            byte[,,,] chunkData = generator.GenerateChunk(pos.X, pos.Y, pos.Z);

            return new(level, chunkData, pos.X, pos.Y, pos.Z);
        }
    }
}
