using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxeload.World;

namespace Voxeload.Render
{
    public class ChunkRenderer
    {
        private int vbo;
        private int vao;
        private int ebo;
        private int cbo;

        private int indicesLength = 0;
        private Voxeload voxeload;

        public bool IsChunkLoaded => model != null;

        private ChunkModel model = null;

        public ChunkRenderer(Voxeload voxeload)
        {
            vbo = GL.GenBuffer();
            vao = GL.GenVertexArray();
            ebo = GL.GenBuffer();
            cbo = GL.GenBuffer();

            this.voxeload = voxeload;
        }

        ~ChunkRenderer()
        {
            GL.DeleteBuffer(vbo);
            GL.DeleteVertexArray(vao);
            GL.DeleteBuffer(ebo);
            GL.DeleteBuffer(cbo);
        }

        public void LoadChunkModel(ChunkModel model)
        {
            // Bind VAO
            GL.BindVertexArray(vao);

            // Load vertices
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 3 * model.Vertices.Length, model.Vertices, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
            GL.EnableVertexAttribArray(0);

            // Load colours
            GL.BindBuffer(BufferTarget.ArrayBuffer, cbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(byte) * model.Colours.Length, model.Colours, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, true, sizeof(byte) * 4, 0);
            GL.EnableVertexAttribArray(1);

            // Load indices
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(uint) * model.Indices.Length, model.Indices, BufferUsageHint.DynamicDraw);

            // Unbind VAO
            GL.BindVertexArray(0);

            // Set indices length
            indicesLength = model.Indices.Length;

            this.model = model;
        }

        public void Render(int x, int y, int z)
        {
            if (!IsChunkLoaded) return;

            // Set uniform
            GL.Uniform4(GL.GetUniformLocation(voxeload.ActiveShader.Handle, "offset"), x * Chunk.X_LENGTH, y * Chunk.Y_LENGTH, z * Chunk.Z_LENGTH, 0.0f);

            // Render
            GL.BindVertexArray(vao);
            GL.DrawElements(PrimitiveType.Triangles, indicesLength, DrawElementsType.UnsignedInt, 0);
        }
    }
}
