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
        private int uvbo;
        private int facebo;

        private int water_vbo;
        private int water_vao;
        private int water_uvbo;
        private int water_facebo;

        private Voxeload voxeload;

        public bool IsChunkLoaded => model != null && waterModel != null;

        private ChunkModel model = null;
        private ChunkModel waterModel = null;

        public ChunkRenderer(Voxeload voxeload)
        {
            vbo = GL.GenBuffer();
            vao = GL.GenVertexArray();
            uvbo = GL.GenBuffer();
            facebo = GL.GenBuffer();

            water_vbo = GL.GenBuffer();
            water_vao = GL.GenVertexArray();
            water_uvbo = GL.GenBuffer();
            water_facebo = GL.GenBuffer();

            this.voxeload = voxeload;
        }

        ~ChunkRenderer()
        {
            GL.DeleteBuffer(vbo);
            GL.DeleteVertexArray(vao);
            GL.DeleteBuffer(uvbo);
            GL.DeleteBuffer(facebo);

            GL.DeleteBuffer(water_vbo);
            GL.DeleteVertexArray(water_vao);
            GL.DeleteBuffer(water_uvbo);
            GL.DeleteBuffer(water_facebo);
        }

        public void LoadChunkModel(ChunkModel model, ChunkModel water_model)
        {
            // Bind VAO
            GL.BindVertexArray(vao);

            // Load vertices
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 3 * model.Vertices.Length, model.Vertices, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
            GL.EnableVertexAttribArray(0);

            // Load UVs
            GL.BindBuffer(BufferTarget.ArrayBuffer, uvbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 2 * model.UVs.Length, model.UVs, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2, 0);
            GL.EnableVertexAttribArray(1);

            // Load faces
            GL.BindBuffer(BufferTarget.ArrayBuffer, facebo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(byte) * model.Faces.Length, model.Faces, BufferUsageHint.DynamicDraw);
            GL.VertexAttribIPointer(2, 1, VertexAttribIntegerType.UnsignedByte, sizeof(byte), IntPtr.Zero);
            GL.EnableVertexAttribArray(2);

            // Unbind VAO
            GL.BindVertexArray(0);

            // Bind VAO
            GL.BindVertexArray(water_vao);

            // Load vertices
            GL.BindBuffer(BufferTarget.ArrayBuffer, water_vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 3 * water_model.Vertices.Length, water_model.Vertices, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
            GL.EnableVertexAttribArray(0);

            // Load UVs
            GL.BindBuffer(BufferTarget.ArrayBuffer, water_uvbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 2 * water_model.UVs.Length, water_model.UVs, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2, 0);
            GL.EnableVertexAttribArray(1);

            // Load faces
            GL.BindBuffer(BufferTarget.ArrayBuffer, water_facebo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(byte) * water_model.Faces.Length, water_model.Faces, BufferUsageHint.DynamicDraw);
            GL.VertexAttribIPointer(2, 1, VertexAttribIntegerType.UnsignedByte, sizeof(byte), IntPtr.Zero);
            GL.EnableVertexAttribArray(2);

            // Unbind VAO
            GL.BindVertexArray(0);

            this.model = model;
            this.waterModel = water_model;
        }

        public void Render(int x, int y, int z)
        {
            if (!IsChunkLoaded) return;

            // Set uniform
            GL.Uniform4(GL.GetUniformLocation(voxeload.ActiveShader.Handle, "offset"), x * Chunk.X_LENGTH, y * Chunk.Y_LENGTH, z * Chunk.Z_LENGTH, 0.0f);

            // Render blocks
            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, model.Vertices.Length);

            // Render water
            GL.BindVertexArray(water_vao);
            GL.Enable(EnableCap.AlphaTest);
            GL.DrawArrays(PrimitiveType.Triangles, 0, waterModel.Vertices.Length);
            GL.Disable(EnableCap.AlphaTest);
        }
    }
}
