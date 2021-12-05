using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Voxeload.Entities;
using Voxeload.World;

namespace Voxeload.Render
{
    public class LevelRenderer
    {
        protected Voxeload voxeload;

        Level level;
        List<ChunkRenderer> renderers;

        HashSet<ChunkCoord> chunksToReload = new();

        ChunkRenderExchangeQueue modeller;

        public LevelRenderer(Voxeload voxeload, Level level)
        {
            this.voxeload = voxeload;

            renderers = new();

            for (int z = 0; z < EntityPlayer.viewRadius * 2 + 1; z++)
            {
                for (int y = 0; y < EntityPlayer.viewRadius * 2 + 1; y++)
                {
                    for (int x = 0; x < EntityPlayer.viewRadius * 2 + 1; x++)
                    {
                        renderers.Add(new ChunkRenderer(voxeload));
                        chunksToReload.Add(new(x, y, z));
                    }
                }
            }

            this.level = level;

            modeller = new(level);
        }

        public void Render()
        {
            //chunksToDraw.Sort(new ChunkToRenderComparator(voxeload.player.Pos));

            HashSet<ChunkCoord> visibleSet = voxeload.player.GetVisibleChunkSet();

            foreach (ChunkCoord chunkCoord in chunksToReload)
            {
                Chunk chunk = level.GetChunk(chunkCoord.X, chunkCoord.Y, chunkCoord.Z);
                if (chunk != null)
                {
                    chunk.IsDirty[0] = false;
                    chunk.IsDirty[1] = false;
                    modeller.Request(chunk);
                    chunksToReload.Remove(chunkCoord);
                }
            }

            (Chunk, ChunkModel[]) models;
            int counter = 0;
            while (counter < 24 && (models = modeller.Receive()).Item2 != null)
            {
                ChunkCoord coord = new(models.Item1.X, models.Item1.Y, models.Item1.Z);
                bool foundRenderer = false;
                // Find renderer to update
                foreach (ChunkRenderer renderer in renderers)
                {
                    if (renderer.ChunkCoords != null && renderer.ChunkCoords.Equals(coord))
                    {
                        renderer.LoadChunkModel(coord, models.Item2);
                        foundRenderer = true;
                        break;
                    }
                }

                if (!foundRenderer)
                {
                    // Find free renderer
                    foreach (ChunkRenderer renderer in renderers)
                    {
                        if (renderer.ChunkCoords == null || !visibleSet.Contains(renderer.ChunkCoords))
                        {
                            renderer.LoadChunkModel(coord, models.Item2);
                            foundRenderer = true;
                            break;
                        }
                    }
                }

                if (foundRenderer) counter++;
            }

            voxeload.FramebufferManager.GetFramebuffer("tiles").Use();

            foreach (ChunkCoord coord in visibleSet)
            {
                if (coord.X < 0 || coord.X >= Level.X_LENGTH) continue;
                if (coord.Y < 0 || coord.Y >= Level.Y_LENGTH) continue;
                if (coord.Z < 0 || coord.Z >= Level.Z_LENGTH) continue;

                Chunk chunk = level.GetChunk(coord.X, coord.Y, coord.Z);
                if (chunk == null) continue;

                ChunkRenderer renderer = renderers.FirstOrDefault(r => r.ChunkCoords != null && r.ChunkCoords.Equals(coord));
                if (renderer == null)
                {
                    chunksToReload.Add(coord);
                    continue;
                }

                if (!voxeload.frustum.VolumeVsFrustum(new(chunk.X * Chunk.X_LENGTH, chunk.Y * Chunk.Y_LENGTH, chunk.Z * Chunk.Z_LENGTH), Chunk.X_LENGTH, Chunk.Y_LENGTH, Chunk.Z_LENGTH)) continue;

                for (int l = 0; l < Chunk.LAYER_COUNT; l++)
                {
                    if (chunk.IsDirty[l])
                    {
                        chunksToReload.Add(coord);
                    }
                }

                renderer.Render(coord.X, coord.Y, coord.Z);
            }

            Framebuffer.Disuse();
        }


    }
}
