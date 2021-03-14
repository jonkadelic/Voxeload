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

        public Model(Vector3[] vertices)
        {
            Vertices = vertices;
        }
    }
}
