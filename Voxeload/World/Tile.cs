using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxeload.Entities;
using Voxeload.Render;

namespace Voxeload.World
{
    public class Tile
    {
        public enum Face
        {
            North = 0x01,
            South = 0x02,
            Bottom = 0x04,
            Top = 0x08,
            West = 0x10,
            East = 0x20
        }

        public byte ID { get; }

        public ITileAppearance TileAppearance { get; }

        public BaseTileModel TileModel { get; } = new CubeTileModel();

        public Tile(byte id, ITileAppearance appearance)
        {
            ID = id;
            TileAppearance = appearance;
            if (tiles[id] != null) throw new Exception("Attempted to overwrite tile!");
            tiles[id] = this;
        }

        public Tile(byte id, uint colour) : this(id, new DefaultTileAppearance(colour))
        {
        }

        public static AABB GetAABB(int x, int y, int z)
        {
            return new AABB(new(x, y, z), new(x + 1, y + 1, z + 1));
        }

        public static readonly Tile[] tiles = new Tile[256];
        public static readonly Tile grass = new(1, new DefaultTileAppearance(0x71895F, 0x7C623A, 0x7C623A));
        public static readonly Tile stone = new(2, 0x7F7E72);
    }
}
