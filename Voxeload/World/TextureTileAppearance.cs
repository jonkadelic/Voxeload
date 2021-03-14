using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.World
{
    public class TextureTileAppearance : ITileAppearance
    {
        private readonly Dictionary<Tile.Face, uint> indexes = new();

        public uint this[Tile.Face face] => indexes[face];

        public TextureTileAppearance(uint index) : this(index, index, index, index, index, index)
        {
        }

        public TextureTileAppearance(uint topBottom, uint sides) : this(sides, sides, topBottom, topBottom, sides, sides)
        {
        }

        public TextureTileAppearance(uint top, uint bottom, uint sides) : this(sides, sides, bottom, top, sides, sides)
        {
        }

        public TextureTileAppearance(uint north, uint south, uint bottom, uint top, uint west, uint east)
        {
            indexes[Tile.Face.North] = north;
            indexes[Tile.Face.South] = south;
            indexes[Tile.Face.Bottom] = bottom;
            indexes[Tile.Face.Top] = top;
            indexes[Tile.Face.West] = west;
            indexes[Tile.Face.East] = east;
        }
    }
}
