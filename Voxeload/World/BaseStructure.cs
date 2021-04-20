using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;

namespace Voxeload.World
{
    public abstract class BaseStructure
    {
        public JavaImports.Random Rand { get; }
        public Vector3i Origin { get; }
        public Vector3i Offset { get; protected set; }
        public int LengthX { get; protected set; }
        public int LengthY { get; protected set; }
        public int LengthZ { get; protected set; }

        public BaseStructure(JavaImports.Random rand, Vector3i origin)
        {
            Rand = rand;
            Origin = origin;
        }

        public abstract Tile GetTileAt(Vector3i pos);
    }
}
