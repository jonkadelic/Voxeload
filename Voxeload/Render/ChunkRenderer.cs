using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxeload.Shaders;
using Voxeload.World;

namespace Voxeload.Render
{
    public class ChunkRenderer
    {
        private int[] vbos = new int[Chunk.LAYER_COUNT];
        private int[] vaos = new int[Chunk.LAYER_COUNT];
        private int[] uvbos = new int[Chunk.LAYER_COUNT];
        private int[] facebos = new int[Chunk.LAYER_COUNT];

        private Voxeload voxeload;

        public bool IsChunkLoaded { get; protected set; }

        private int[] vertexLengths = new int[Chunk.LAYER_COUNT];

        public ChunkRenderer(Voxeload voxeload)
        {
            for (int i = 0; i < Chunk.LAYER_COUNT; i++)
            {
                vbos[i] = GL.GenBuffer();
                vaos[i] = GL.GenVertexArray();
                uvbos[i] = GL.GenBuffer();
                facebos[i] = GL.GenBuffer();
            }

            this.voxeload = voxeload;
        }

        ~ChunkRenderer()
        {
            GL.DeleteBuffers(Chunk.LAYER_COUNT, vbos);
            GL.DeleteVertexArrays(Chunk.LAYER_COUNT, vaos);
            GL.DeleteBuffers(Chunk.LAYER_COUNT, uvbos);
            GL.DeleteBuffers(Chunk.LAYER_COUNT, facebos);
        }

        public void LoadChunkModel(ChunkModel[] models)
        {
            for (int i = 0; i < Chunk.LAYER_COUNT; i++)
            {
                // Bind VAO
                GL.BindVertexArray(vaos[i]);

                // Load vertices
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbos[i]);
                GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 3 * models[i].Vertices.Length, models[i].Vertices, BufferUsageHint.DynamicDraw);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
                GL.EnableVertexAttribArray(0);

                // Load UVs
                GL.BindBuffer(BufferTarget.ArrayBuffer, uvbos[i]);
                GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 2 * models[i].UVs.Length, models[i].UVs, BufferUsageHint.DynamicDraw);
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2, 0);
                GL.EnableVertexAttribArray(1);

                // Load faces
                GL.BindBuffer(BufferTarget.ArrayBuffer, facebos[i]);
                GL.BufferData(BufferTarget.ArrayBuffer, sizeof(byte) * models[i].Faces.Length, models[i].Faces, BufferUsageHint.DynamicDraw);
                GL.VertexAttribIPointer(2, 1, VertexAttribIntegerType.UnsignedByte, sizeof(byte), IntPtr.Zero);
                GL.EnableVertexAttribArray(2);

                // Unbind VAO
                GL.BindVertexArray(0);

                vertexLengths[i] = models[i].Vertices.Length;
            }

            IsChunkLoaded = true;
        }

        public void Render(int x, int y, int z)
        {
            if (!IsChunkLoaded) return;

            // Set uniform
            GL.Uniform4(GL.GetUniformLocation(voxeload.ActiveShader.Handle, "offset"), x * Chunk.X_LENGTH, y * Chunk.Y_LENGTH, z * Chunk.Z_LENGTH, 0.0f);

            // Render blocks
            GL.BindVertexArray(vaos[0]);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertexLengths[0]);

            Framebuffer activeBuffer = Framebuffer.ActiveFramebuffer;

            // Render water
            GL.BindVertexArray(vaos[1]);
            voxeload.ShaderProgramManager.GetProgram("water_tile").Use();
            GL.Uniform4(GL.GetUniformLocation(voxeload.ActiveShader.Handle, "offset"), x * Chunk.X_LENGTH, y * Chunk.Y_LENGTH, z * Chunk.Z_LENGTH, 0.0f);
            voxeload.FramebufferManager.GetFramebuffer("water").Use();
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertexLengths[1]);
            voxeload.ActiveShader.Use();
            activeBuffer.Use();
        }
    }
}
