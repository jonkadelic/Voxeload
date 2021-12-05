using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.World
{
    public class ChunkCoord
    {
        public ChunkCoord(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public int X { get; }
        public int Y { get; }
        public int Z { get; }

        public override bool Equals(object obj)
        {
            if (obj is not ChunkCoord coord) return false;

            else return coord.X == X && coord.Y == Y && coord.Z == Z;
        }

        public override int GetHashCode()
        {
            return (X << 30) ^ (Y << 15) ^ Z;
        }

        public override string ToString()
        {
            return $"{X}, {Y}, {Z}";
        }
    }
}
