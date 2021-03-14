using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxeload.World;

namespace Voxeload.Render
{
    public class ChunkModel : Model
    {
        public byte[] Colours { get; }

        public Chunk Chunk { get; }

        public ChunkModel(Chunk chunk, Vector3[] vertices, uint[] indices, byte[] colours) : base(vertices, indices)
        {
            Chunk = chunk;
            Colours = colours;
        }
    }
}
