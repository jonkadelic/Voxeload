using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

        List<(int x, int y, int z)> chunksToReload = new();
        List<(int x, int y, int z)> chunksToDraw = new();

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
                        chunksToReload.Add((x, y, z));
                        chunksToDraw.Add((x, y, z));
                    }
                }
            }

            this.level = level;

            modeller = new(level);
        }

        public void Render()
        {
            //chunksToDraw.Sort(new ChunkToRenderComparator(voxeload.player.Pos));

            int counter = 0;
            while (chunksToReload.Count > 0 && counter < 16) 
            {
                (int x, int y, int z) = chunksToReload[0];
                Chunk chunk = level.GetChunk(x, y, z);
                if (chunk != null)
                {
                    modeller.Request(chunk);
                    chunksToReload.RemoveAt(0);
                }
                counter++;
            }

            ChunkModel[] models;
            counter = 0;
            while (counter < 16 && (models = modeller.Receive()) != null)
            {
                renderers[models[0].Chunk.Z, models[0].Chunk.Y, models[0].Chunk.X].LoadChunkModel(models);
                counter++;
            }

            voxeload.FramebufferManager.GetFramebuffer("tiles").Use();

            foreach ((int x, int y, int z) in chunksToDraw)
            {
                ChunkRenderer renderer = renderers[z, y, x];
                Chunk chunk = level.GetChunk(x, y, z);

                if (chunk == null) continue;

                for (int l = 0; l < Chunk.LAYER_COUNT; l++)
                {
                    if (chunk.IsDirty[l])
                    {
                        chunksToReload.Insert(0, (x, y, z));
                        chunk.IsDirty[l] = false;
                    }
                }

                renderer.Render(x, y, z);
            }

            Framebuffer.Disuse();
        }


    }
}
