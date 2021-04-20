using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;

using Voxeload.World;

namespace Voxeload.Render
{
    public class FlatTileModels : BaseTileModels
    {
        private static TileModel model;

        public override TileModel GetModel(byte sides)
        {
            return model;
        }

        static FlatTileModels()
        {
            Vector3[] vertices = new Vector3[24]
            {
                new(0, 0, 0),
                new(1, 0, 1),
                new(1, 1, 1),
                new(0, 1, 0),
                new(1, 1, 1),
                new(0, 0, 0),

                new(1, 1, 1),
                new(1, 0, 1),
                new(0, 0, 0),
                new(0, 0, 0),
                new(1, 1, 1),
                new(0, 1, 0),

                new(1, 0, 0),
                new(0, 0, 1),
                new(0, 1, 1),
                new(0, 1, 1),
                new(1, 1, 0),
                new(1, 0, 0),

                new(0, 1, 1),
                new(0, 0, 1),
                new(1, 0, 0),
                new(1, 0, 0),
                new(1, 1, 0),
                new(0, 1, 1),
            };

            Vector2[] uvs = new Vector2[24]
            {
                new(1, 0),
                new(0, 0),
                new(0, 1),
                new(1, 1),
                new(0, 1),
                new(1, 0),

                new(1, 1),
                new(1, 0),
                new(0, 0),
                new(0, 0),
                new(1, 1),
                new(0, 1),

                new(0, 0),
                new(1, 0),
                new(1, 1),
                new(1, 1),
                new(0, 1),
                new(0, 0),

                new(0, 1),
                new(0, 0),
                new(1, 0),
                new(1, 0),
                new(1, 1),
                new(0, 1),
            };

            byte[] uvFaces = new byte[24]
            {
                (int)Tile.Face.North, (int)Tile.Face.North, (int)Tile.Face.North,
                (int)Tile.Face.North, (int)Tile.Face.North, (int)Tile.Face.North,

                (int)Tile.Face.South, (int)Tile.Face.South, (int)Tile.Face.South,
                (int)Tile.Face.South, (int)Tile.Face.South, (int)Tile.Face.South,

                (int)Tile.Face.West, (int)Tile.Face.West, (int)Tile.Face.West,
                (int)Tile.Face.West, (int)Tile.Face.West, (int)Tile.Face.West,

                (int)Tile.Face.East, (int)Tile.Face.East, (int)Tile.Face.East,
                (int)Tile.Face.East, (int)Tile.Face.East, (int)Tile.Face.East,
            };

            model = new(vertices, uvs, uvFaces);
        }
    }
}
