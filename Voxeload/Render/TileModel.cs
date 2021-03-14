using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.Render
{
    public class TileModel : TexturedModel
    {

        public byte[] UVFaces { get; }

        public TileModel(Vector3[] vertices, Vector2[] uvs, byte[] uvFaces) : base(vertices, uvs)
        {
            UVFaces = uvFaces;
        }

    }
}
