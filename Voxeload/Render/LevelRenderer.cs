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
        ChunkRenderer[,,] renderers;

        LinkedList<(int x, int y, int z)> chunksToReload = new();

        ChunkRenderExchangeQueue modeller;

        public LevelRenderer(Voxeload voxeload, Level level)
        {
            this.voxeload = voxeload;

            renderers = new ChunkRenderer[Level.Z_LENGTH, Level.Y_LENGTH, Level.X_LENGTH];

            for (int z = 0; z < Level.Z_LENGTH; z++)
            {
                for (int y = 0; y < Level.Y_LENGTH; y++)
                {
                    for (int x = 0; x < Level.X_LENGTH; x++)
                    {
                        renderers[z, y, x] = new ChunkRenderer(voxeload);
                        chunksToReload.AddLast((x, y, z));
                    }
                }
            }

            this.level = level;

            modeller = new(level);
        }

        public void Render()
        {
            int counter = 0;
            while (chunksToReload.Count > 0 && counter < 16) 
            {
                (int x, int y, int z) = chunksToReload.First.Value;
                Chunk chunk = level.GetChunk(x, y, z);
                if (chunk != null)
                {
                    modeller.Request(chunk);
                    chunksToReload.RemoveFirst();
                }
                counter++;
            }

            ChunkModel model;
            counter = 0;
            while (counter < 16 && (model = modeller.Receive()) != null)
            {
                renderers[model.Chunk.Z, model.Chunk.Y, model.Chunk.X].LoadChunkModel(model);
                counter++;
            }


            for (int z = 0; z < Level.Z_LENGTH; z++)
            {
                for (int y = 0; y < Level.Y_LENGTH; y++)
                {
                    for (int x = 0; x < Level.X_LENGTH; x++)
                    {
                        ChunkRenderer renderer = renderers[z, y, x];
                        Chunk chunk = level.GetChunk(x, y, z);

                        if (chunk == null) continue;

                        if (chunk.IsDirty)
                        {
                            chunksToReload.AddFirst((x, y, z));
                            chunk.IsDirty = false;
                        }

                        //if (renderer.IsChunkLoaded == false && !chunksToReload.Contains((x, y, z)))
                        //{
                        //    chunksToReload.AddFirst((x, y, z));
                        //}

                        renderer.Render(x, y, z);
                    }
                }
            }
        }


    }
}
