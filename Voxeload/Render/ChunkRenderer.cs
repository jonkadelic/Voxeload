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

        public bool IsChunkLoaded => Chunk != null;

        public Chunk Chunk { get; private set; } = null;

        public int X { get; set; } = 0;
        public int Z { get; set; } = 0;

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

        public void LoadChunk(Level level, Chunk chunk)
        {
            Chunk = chunk;
            if (chunk == null) return;

            List<Vector3> vertices = new();
            List<uint> indices = new();
            List<byte> colours = new();

            for (int z = 0; z < Chunk.Z_LENGTH; z++)
            {
                for (int y = 0; y < Chunk.Y_LENGTH; y++)
                {
                    for (int x = 0; x < Chunk.X_LENGTH; x++)
                    {
                        byte sides = level.GetVisibleSides((X * Chunk.X_LENGTH) + x, y, (Z * Chunk.Z_LENGTH) + z);
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

            // Bind VAO
            GL.BindVertexArray(vao);

            // Load vertices
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 3 * vertices.Count, vertices.ToArray(), BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
            GL.EnableVertexAttribArray(0);

            // Load colours
            GL.BindBuffer(BufferTarget.ArrayBuffer, cbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(byte) * colours.Count, colours.ToArray(), BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, true, sizeof(byte) * 4, 0);
            GL.EnableVertexAttribArray(1);

            // Load indices
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(uint) * indices.Count, indices.ToArray(), BufferUsageHint.DynamicDraw);

            // Unbind VAO
            GL.BindVertexArray(0);

            // Set indices length
            indicesLength = indices.Count;
        }

        public void Render(int x, int z)
        {
            if (!IsChunkLoaded) return;

            // Set uniform
            GL.Uniform4(GL.GetUniformLocation(voxeload.ActiveShader.Handle, "offset"), x * Chunk.X_LENGTH, 0, z * Chunk.Z_LENGTH, 0.0f);

            // Render
            GL.BindVertexArray(vao);
            GL.DrawElements(PrimitiveType.Triangles, indicesLength, DrawElementsType.UnsignedInt, 0);
        }
    }
}
