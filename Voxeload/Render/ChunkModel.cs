using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxeload.World;

namespace Voxeload.Render
{
    public class ChunkModel : TexturedModel
    {
        public Chunk Chunk { get; }

        public byte[] Faces { get; }

        public float[] Brightnesses { get; }

        public ChunkModel(Chunk chunk, Vector3[] vertices, Vector2[] uvs, byte[] faces, float[] brightnesses) : base(vertices, uvs)
        {
            Chunk = chunk;
            Faces = faces;
            Brightnesses = brightnesses;
        }

        public override string ToString()
        {
            return $"Chunk: {Chunk.X} {Chunk.Y} {Chunk.Z}";
        }
    }
}
