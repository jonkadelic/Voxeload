using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.World
{
    public class ChunkStack
    {
        public int ChunkX { get; }
        public int ChunkZ { get; }
        public Chunk[] Stack { get; } = new Chunk[Level.Y_LENGTH];
        public int[,] Heightmap { get; } = new int[Chunk.Z_LENGTH, Chunk.X_LENGTH];

        public ChunkStack(int chunkX, int chunkZ)
        {
            ChunkX = chunkX;
            ChunkZ = chunkZ;
        }

        public byte GetTileID(int layer, int x, int y, int z)
        {
            if (x < 0 || x >= Chunk.X_LENGTH) return 0;
            if (y < 0 || y >= (Level.Y_LENGTH * Chunk.Y_LENGTH)) return 0;
            if (z < 0 || z >= Chunk.Z_LENGTH) return 0;

            int chunkY = y / Chunk.Y_LENGTH;
            int localY = y % Chunk.Y_LENGTH;

            Chunk chunk = Stack[chunkY];
            if (chunk == null) return 0;

            return chunk.GetTileID(layer, x, localY, z);
        }

        public void BuildHeightmap(int x, int z, int xLength, int zLength)
        {
            for (int dz = 0; dz < zLength; dz++)
            {
                for (int dx = 0; dx < xLength; dx++)
                {
                    int newHeight = -1;
                    for (int dy = Level.Y_LENGTH * Chunk.Y_LENGTH; dy >= 0; dy--)
                    {
                        for (int layer = 0; layer < Chunk.LAYER_COUNT; layer++)
                        {
                            int tileID = GetTileID(layer, x + dx, dy, z + dz);

                            if (!Tile.tiles[tileID].IsTransparent)
                            {
                                newHeight = dy;
                                break;
                            }
                        }

                        if (newHeight != -1)
                        {
                            break;
                        }
                    }

                    if (newHeight == -1)
                    {
                        newHeight = 0;
                    }

                    Heightmap[z + dz, x + dx] = newHeight;
                }
            }

            //for (int y = 0; y < Stack.Length; y++)
            //{
            //    if (Stack[y] == null) continue;
            //    Stack[y].IsDirty[0] = true;
            //}
        }
    }
}
