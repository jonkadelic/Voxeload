using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxeload.World;

namespace Voxeload.Render
{
    public class LevelRenderer
    {
        protected Voxeload voxeload;

        Level level;
        ChunkRenderer[,] renderers;

        LinkedList<(int x, int z)> chunksToReload = new();

        public LevelRenderer(Voxeload voxeload, Level level)
        {
            this.voxeload = voxeload;

            renderers = new ChunkRenderer[Level.Z_LENGTH, Level.X_LENGTH];

            for (int z = 0; z < Level.Z_LENGTH; z++)
            {
                for (int x = 0; x < Level.X_LENGTH; x++)
                {
                    renderers[z, x] = new ChunkRenderer(voxeload);
                    chunksToReload.AddLast((x, z));
                }
            }

            this.level = level;
        }

        public void Render()
        {
            int counter = 0;
            while (chunksToReload.Count != 0 && counter < 4)
            {
                (int x, int z) = chunksToReload.First.Value;
                chunksToReload.RemoveFirst();

                ChunkRenderer renderer = renderers[z, x];
                renderer.X = x;
                renderer.Z = z;
                renderer.LoadChunk(level, level.GetChunk(x, z));

                counter++;
            }

            for (int z = 0; z < Level.Z_LENGTH; z++)
            {
                for (int x = 0; x < Level.X_LENGTH; x++)
                {
                    ChunkRenderer renderer = renderers[z, x];
                    Chunk chunk = level.GetChunk(x, z);

                    if (chunk == null) continue;

                    if (chunk.IsDirty)
                    {
                        chunksToReload.AddFirst((x, z));
                        chunk.IsDirty = false;
                    }

                    if (renderer.IsChunkLoaded == false && !chunksToReload.Contains((x, z)))
                    {
                        chunksToReload.AddFirst((x, z));
                    }

                    renderer.Render(x, z);
                }
            }
        }


    }
}
