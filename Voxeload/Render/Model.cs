using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.Render
{
    public class Model
    {
        public Vector3[] Vertices { get; }
        public uint[] Indices { get; }

        public Model(Vector3[] vertices, uint[] indices)
        {
            Vertices = vertices;
            Indices = indices;
        }
    }
}
