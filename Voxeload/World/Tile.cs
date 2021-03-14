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

        public BaseTileModels TileModel { get; } = new CubeTileModels();

        public Tile(byte id, ITileAppearance appearance)
        {
            ID = id;
            TileAppearance = appearance;
            if (tiles[id] != null) throw new Exception("Attempted to overwrite tile!");
            tiles[id] = this;
        }

        public Tile(byte id, uint texture) : this(id, new TextureTileAppearance(texture))
        {
        }

        public static AABB GetAABB(int x, int y, int z)
        {
            return new AABB(new(x, y, z), new(x + 1, y + 1, z + 1));
        }

        public static readonly Tile[] tiles = new Tile[256];
        public static readonly Tile grass = new(1, new TextureTileAppearance(0, 2, 3));
        public static readonly Tile stone = new(2, new TextureTileAppearance(1));
        public static readonly Tile dirt = new(3, new TextureTileAppearance(2));
        public static readonly Tile sand = new(4, new TextureTileAppearance(18));
        public static readonly Tile gravel = new(5, new TextureTileAppearance(19));
        public static readonly Tile water = new(6, new TextureTileAppearance(205));
    }
}
