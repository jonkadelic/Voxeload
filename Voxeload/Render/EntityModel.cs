using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.Render
{
    public class EntityModel : Model
    {
        private int vbo;
        private int vao;
        private int ebo;
        private int cbo;

        public EntityModel(Model model) : this(model.Vertices, model.Indices)
        {

        }

        public EntityModel(Vector3[] vertices, uint[] indices) : base(vertices, indices)
        {
            List<byte> colours = new();

            for (int i = 0; i < vertices.Length * 4; i++)
            {
                colours.Add(0);
            }

            vbo = GL.GenBuffer();
            vao = GL.GenVertexArray();
            ebo = GL.GenBuffer();
            cbo = GL.GenBuffer();

            GL.BindVertexArray(vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float) * 3, vertices, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, cbo);
            GL.BufferData(BufferTarget.ArrayBuffer, colours.Count * sizeof(byte), colours.ToArray(), BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, true, 4 * sizeof(byte), 0);
            GL.EnableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(0);
        }

        public virtual void SetColours(uint[] colours)
        {
            if (colours.Length != Vertices.Length) throw new ArgumentException("Not enough colours!");

            byte[] data = new byte[colours.Length * 4];

            for (int i = 0; i < colours.Length; i++)
            {
                byte[] temp = BitConverter.GetBytes(colours[i]);
                temp.CopyTo(data, i * 4);
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, cbo);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, colours.Length * 4, data);
        }

        public virtual void Render()
        {
            GL.BindVertexArray(vao);
            GL.DrawElements(PrimitiveType.Triangles, Indices.Length, DrawElementsType.UnsignedInt, 0);
        }
    }
}
