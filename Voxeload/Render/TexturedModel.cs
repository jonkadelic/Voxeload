using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxeload.World;

namespace Voxeload.Render
{
    public class TexturedModel : Model
    {
        public Vector2[] UVs { get; }

        public TexturedModel(Vector3[] vertices, Vector2[] uvs) : base(vertices)
        {
            UVs = uvs;
        }
    }
}
