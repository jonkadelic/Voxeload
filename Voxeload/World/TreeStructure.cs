using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;

using SimplexNoise;

namespace Voxeload.World
{
    public class TreeStructure : BaseStructure
    {
        public TreeStructure(JavaImports.Random rand, Vector3i origin) : base(rand, origin)
        {
            Offset = new(-2, -1, -2);
            LengthX = LengthZ = 5;
            LengthY = 8;
            int opt = rand.NextInt(3);
            if (opt == 0) LengthY--;
            else if (opt == 2) LengthY++;
        }

        public override Tile GetTileAt(Vector3i pos)
        {
            if (pos.X == 2 && pos.Y == 0 && pos.Z == 2) return Tile.dirt;
            else if (pos.X == 2 && pos.Y != (LengthY - 1) && pos.Z == 2) return Tile.log;
            else if (pos.Y > (LengthY - 5) && pos.Y <= LengthY - 3) return Tile.leaves;
            else if (pos.Y > LengthY - 3)
            {
                if ((pos.X == 2 && pos.Z >= 1 && pos.Z < LengthZ - 1) || (pos.Z == 2 && pos.X >= 1 && pos.X < LengthX - 1))
                {
                    return Tile.leaves;
                }
                else return Tile.air;
            }

            else return Tile.air;
        }
    }
}
