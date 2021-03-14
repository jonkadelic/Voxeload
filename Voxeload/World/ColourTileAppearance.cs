using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.World
{
    public class ColourTileAppearance : ITileAppearance
    {
        private readonly Dictionary<Tile.Face, uint> colours = new();

        public uint this[Tile.Face face] => colours[face];

        public ColourTileAppearance(uint colour) : this(colour, colour, colour, colour, colour, colour)
        {
        }

        public ColourTileAppearance(uint topBottom, uint sides) : this(sides, sides, topBottom, topBottom, sides, sides)
        {
        }

        public ColourTileAppearance(uint top, uint bottom, uint sides) : this(sides, sides, bottom, top, sides, sides)
        {
        }

        public ColourTileAppearance(uint north, uint south, uint bottom, uint top, uint west, uint east)
        {
            colours[Tile.Face.North] = north;
            colours[Tile.Face.South] = south;
            colours[Tile.Face.Bottom] = bottom;
            colours[Tile.Face.Top] = top;
            colours[Tile.Face.West] = west;
            colours[Tile.Face.East] = east;
        }
    }
}
