using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.World
{
    public class Chunk : ITileAccess
    {
        public const int X_LENGTH = 32;
        public const int Y_LENGTH = 32;
        public const int Z_LENGTH = 32;
        public const int LAYER_COUNT = 2;

        private readonly byte[,,,] tiles;

        private readonly Level level;

        public bool[] IsDirty { get; set; } = new bool[2];

        public int X { get; }
        public int Y { get; }
        public int Z { get; }

        public object chunkDataLock = new();

        public Chunk(Level level, byte[,,,] tiles, int x, int y, int z)
        {
            this.tiles = tiles;
            this.level = level;
            X = x;
            Y = y;
            Z = z;
        }

        public byte GetTileID(int layer, int x, int y, int z)
        {
            if (x < 0 || x >= X_LENGTH) return 0;
            if (y < 0 || y >= Y_LENGTH) return 0;
            if (z < 0 || z >= Z_LENGTH) return 0;

            lock (chunkDataLock)
            {
                return tiles[layer, z, y, x];
            }
        }

        public void SetTileID(int layer, int x, int y, int z, byte id)
        {
            if (x < 0 || x >= X_LENGTH) return;
            if (y < 0 || y >= Y_LENGTH) return;
            if (z < 0 || z >= Z_LENGTH) return;

            lock (chunkDataLock)
            {
                //Console.WriteLine($"Set tile at {x}, {y}, {z} in chunk {X}, {Y}, {Z} from {tiles[layer, z, y, x]} to {id}.");
                tiles[layer, z, y, x] = id;
            }


            IsDirty[layer] = true;
        }

        public byte GetVisibleSides(int layer, int x, int y, int z)
        {
            byte minusZ = GetTileID(layer,x, y, z - 1);
            byte plusZ = GetTileID(layer, x, y, z + 1);
            byte minusY = GetTileID(layer, x, y - 1, z);
            byte plusY = GetTileID(layer, x, y + 1, z);
            byte minusX = GetTileID(layer, x - 1, y, z);
            byte plusX = GetTileID(layer, x + 1, y, z);

            byte sides = 0;

            if (minusZ == 0) sides |= 1 << 0;
            if (plusZ == 0) sides |= 1 << 1;
            if (minusY == 0) sides |= 1 << 2;
            if (plusY == 0) sides |= 1 << 3;
            if (minusX == 0) sides |= 1 << 4;
            if (plusX == 0) sides |= 1 << 5;

            return sides;
        }

        public override bool Equals(object obj)
        {
            if (obj is Chunk other)
            {
                if (X == other.X && Y == other.Y && Z == other.Z) return true;
                else return false;
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return X << 20 ^ Y << 10 ^ Z;
        }

        public void TickTiles(int counter)
        {
            for (int l = 0; l < Chunk.LAYER_COUNT; l++)
            {
                for (int z = 0; z < Chunk.Z_LENGTH; z++)
                {
                    for (int y = 0; y < Chunk.Y_LENGTH; y++)
                    {
                        for (int x = 0; x < Chunk.X_LENGTH; x++)
                        {
                            Tile tile = Tile.tiles[GetTileID(l, x, y, z)];
                            if (tile.TickInterval == -1) continue;
                            if (counter % tile.TickInterval == 0)
                            {
                                tile.OnTick(level, this.X * X_LENGTH + x, this.Y * Y_LENGTH + y, this.Z * Z_LENGTH + z);
                            }
                        }
                    }
                }
            }
        }
    }
}
