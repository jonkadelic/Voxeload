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

            ChunkModel[] models;
            counter = 0;
            while (counter < 16 && (models = modeller.Receive()) != null)
            {
                renderers[models[0].Chunk.Z, models[0].Chunk.Y, models[0].Chunk.X].LoadChunkModel(models);
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

                        if (chunk.IsDirty[0])
                        {
                            chunksToReload.AddFirst((x, y, z));
                            chunk.IsDirty[0] = false;
                        }

                        renderer.Render(x, y, z);
                    }
                }
            }

            // Draw framebuffer to screen
            voxeload.ShaderProgramManager.GetProgram("frame").Use();
            GL.BindVertexArray(ChunkRenderer.screenvao);
            //GL.Disable(EnableCap.DepthTest);
            GL.BindTexture(TextureTarget.Texture2D, ChunkRenderer.watertexc);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.Disable(EnableCap.Blend);
            //GL.Enable(EnableCap.DepthTest);
            voxeload.ActiveShader.Use();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, ChunkRenderer.waterfbo);
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }


    }
}
