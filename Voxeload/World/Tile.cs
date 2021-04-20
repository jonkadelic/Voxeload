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

        public virtual ITileAppearance TileAppearance { get; }

        public virtual BaseTileModels TileModel { get; } = new CubeTileModels();

        public virtual bool Collides { get; } = true;

        public virtual int TickInterval { get; } = -1;

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

        public virtual void OnUpdate(Level level, int x, int y, int z)
        {
        }

        public virtual void OnTick(Level level, int x, int y, int z)
        {
        }

        public static readonly Tile[] tiles = new Tile[256];
        public static readonly TileTransparent air = new(0, null);
        public static readonly Tile grass = new(1, new TextureTileAppearance(0, 2, 3));
        public static readonly Tile stone = new(2, 1);
        public static readonly Tile dirt = new(3, 2);
        public static readonly Tile planks = new(4, 4);
        public static readonly TileFalling sand = new(5, 18);
        public static readonly TileFalling gravel = new(6, 19);
        public static readonly TileWater water = new(7, new TextureTileAppearance(205));
        public static readonly TileTransparent glass = new(8, new TextureTileAppearance(49));
        public static readonly Tile log = new(9, new TextureTileAppearance(21, 20));
        public static readonly TileTransparent leaves = new(10, new TextureTileAppearance(52));
        public static readonly TileFlower roseFlower = new(11, new TextureTileAppearance(12));
    }
}
