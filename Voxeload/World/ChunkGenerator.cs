using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Voxeload.World
{
    public class ChundskGenerator
    {
        protected Queue<Vector3i> chunksToGenerate = new();
        protected Queue<Chunk> completedChunks = new();
        protected bool stopping = false;
        protected Level level;
        protected object toGenerateLock = new();
        protected object completedLock = new();
        protected ILevelGenerator generator;

        public ChunkGenerator(Level level, ILevelGenerator generator)
        {
            this.level = level;
            this.generator = generator;
            Thread t = new(() => Run());
            t.IsBackground = true;
            t.Priority = ThreadPriority.AboveNormal;
            t.Start();
        }

        protected void Run()
        {
            while (!stopping)
            {
                if (chunksToGenerate.Count > 0)
                {
                    Vector3i pos;
                    lock (toGenerateLock)
                    {
                        pos = chunksToGenerate.Dequeue();
                    }
                    GenerateChunk(pos);
                }
            }
        }

        public bool Request(Vector3i pos)
        {
            if (!chunksToGenerate.Contains(pos))
            {
                lock (toGenerateLock)
                {
                    chunksToGenerate.Enqueue(pos);
                }
                return true;
            }
            else return false;
        }

        public Chunk Receive()
        {
            if (completedChunks.Count > 0)
            {
                lock (completedLock)
                {
                    return completedChunks.Dequeue();
                }
            }

            return null;
        }

        protected void GenerateChunk(Vector3i pos)
        {
            byte[,,] chunkData = generator.GenerateChunk(pos.X, pos.Y, pos.Z);

            Chunk chunk = new(level, chunkData, pos.X, pos.Y, pos.Z);

            lock (completedLock)
            {
                completedChunks.Enqueue(chunk);
            }
        }
    }
}
