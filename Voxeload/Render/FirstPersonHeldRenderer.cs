using System;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using Voxeload.World;

namespace Voxeload.Render
{
    public class FirstPersonHeldRenderer
    {
        protected Voxeload voxeload;
        protected int vbo;
        protected int vao;
        protected int uvbo;
        protected int facebo;

        protected int vertexLength = 0;

        public Tile Tile { get; protected set; }
        
        public FirstPersonHeldRenderer(Voxeload voxeload)
        {
            this.voxeload = voxeload;

            // Generate buffers
            vbo = GL.GenBuffer();
            vao = GL.GenVertexArray();
            uvbo = GL.GenBuffer();
            facebo = GL.GenBuffer();

            SetTile(voxeload.player.HeldTile);
        }

        private byte faces = 0b00000000;
        public void SetTile(Tile tile)
        {
            if (tile is null)
            {
                throw new ArgumentNullException(nameof(tile));
            }

            // Get model
            TileModel model = tile.TileModel.GetModel(0b00111111);

            List<Vector2> uvs = new();

            for (int i = 0; i < model.UVs.Length; i++)
            {
                int index = (int)tile.TileAppearance[(Tile.Face)model.UVFaces[i]];
                int texX = index % 16;
                int texY = 15 - (index / 16);

                uvs.Add(new((model.UVs[i].X + texX) / 16, (model.UVs[i].Y + texY) / 16));
            }

            // Bind VAO
            GL.BindVertexArray(vao);

            // Load vertices
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 3 * model.Vertices.Length, model.Vertices, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
            GL.EnableVertexAttribArray(0);

            // Load UVs
            GL.BindBuffer(BufferTarget.ArrayBuffer, uvbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 2 * uvs.Count, uvs.ToArray(), BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2, 0);
            GL.EnableVertexAttribArray(1);

            // Load faces
            GL.BindBuffer(BufferTarget.ArrayBuffer, facebo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(byte) * model.UVFaces.Length, model.UVFaces, BufferUsageHint.DynamicDraw);
            GL.VertexAttribIPointer(2, 1, VertexAttribIntegerType.UnsignedByte, sizeof(byte), IntPtr.Zero);
            GL.EnableVertexAttribArray(2);

            // Unbind VAO
            GL.BindVertexArray(0);

            vertexLength = model.Vertices.Length;
            Tile = tile;
        }

        public void Render()
        {
            Matrix4 model = Matrix4.CreateTranslation(-0.5f, -0.5f, -0.5f) * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(45)) * Matrix4.CreateTranslation(1.5f, -1.2f, -1.5f);
            Matrix4 view = Matrix4.Identity;
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90), (float)voxeload.ClientSize.X / voxeload.ClientSize.Y, 0.1f, 100.0f);
            GL.UniformMatrix4(GL.GetUniformLocation(voxeload.ActiveShader.Handle, "model"), false, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(voxeload.ActiveShader.Handle, "view"), false, ref view);
            GL.UniformMatrix4(GL.GetUniformLocation(voxeload.ActiveShader.Handle, "projection"), false, ref projection);
            GL.Uniform4(GL.GetUniformLocation(voxeload.ActiveShader.Handle, "offset"), 0, 0, 0, 0.0f);

            GL.BindVertexArray(vao);
            GL.Disable(EnableCap.DepthTest);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertexLength);
            GL.Enable(EnableCap.DepthTest);

        }
    }
}
