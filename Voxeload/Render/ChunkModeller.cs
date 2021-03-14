using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Voxeload.World;

namespace Voxeload.Render
{
    public class ChunkModeller
    {
        protected Queue<Chunk> chunksToModel = new();
        protected Queue<ChunkModel> completedModels = new();
        protected bool stopping = false;
        protected Level level;
        protected object lockObject = new();

        public ChunkModeller(Level level)
        {
            this.level = level;
            Thread t = new(() => Run());
            t.IsBackground = true;
            t.Priority = ThreadPriority.AboveNormal;
            t.Start();
        }

        protected void Run()
        {
            while (!stopping)
            {
                if (chunksToModel.Count > 0)
                {
                    Chunk chunk;
                    lock (lockObject)
                    {
                        chunk = chunksToModel.Dequeue();
                    }
                    LoadChunk(chunk);
                }
            }
        }

        public bool Request(Chunk chunk)
        {
            if (!chunksToModel.Contains(chunk))
            {
                lock (lockObject)
                {
                    chunksToModel.Enqueue(chunk);
                }
                return true;
            }
            else return false;
        }

        public ChunkModel Receive()
        {
            if (completedModels.Count > 0)
            {
                return completedModels.Dequeue();
            }

            return null;
        }

        protected void LoadChunk(Chunk chunk)
        {
            if (chunk == null) return;

            List<Vector3> vertices = new();
            List<uint> indices = new();
            List<byte> colours = new();

            lock (chunk.chunkDataLock)
            {
                for (int z = 0; z < Chunk.Z_LENGTH; z++)
                {
                    for (int y = 0; y < Chunk.Y_LENGTH; y++)
                    {
                        for (int x = 0; x < Chunk.X_LENGTH; x++)
                        {
                            byte sides = level.GetVisibleSides((chunk.X * Chunk.X_LENGTH) + x, (chunk.Y * Chunk.Y_LENGTH) + y, (chunk.Z * Chunk.Z_LENGTH) + z);
                            byte id = chunk.GetTileID(x, y, z);

                            if (sides == 0 || id == 0) continue;

                            Tile tile = Tile.tiles[id];

                            if (tile == null) continue;

                            Vector3 offset = new(x, y, z);

                            Model model = tile.TileModel.GetModel(sides);

                            uint count = (uint)vertices.Count;
                            foreach (uint index in model.Indices)
                            {
                                indices.Add(index + count);
                            }

                            foreach (Vector3 vert in model.Vertices)
                            {
                                vertices.Add(vert + offset);
                            }

                            colours.AddRange(tile.TileModel.GetColours(tile.TileAppearance, sides));
                        }
                    }
                }
            }

            completedModels.Enqueue(new(chunk, vertices.ToArray(), indices.ToArray(), colours.ToArray()));
        }
    }
}
