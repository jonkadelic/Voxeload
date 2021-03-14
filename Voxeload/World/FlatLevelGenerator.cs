using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.World
{
    public class FlatLevelGenerator : ILevelGenerator
    {
        Random rand = new();

        public byte[,,] GenerateChunk(int x, int y, int z)
        {
            byte[,,] tiles = new byte[Chunk.X_LENGTH, Chunk.Y_LENGTH, Chunk.Z_LENGTH];

            for (int tileZ = 0; tileZ < Chunk.Z_LENGTH; tileZ++)
            {
                int worldZ = z * Chunk.Z_LENGTH + tileZ;
                for (int tileY = 0; tileY < Chunk.Y_LENGTH; tileY++)
                {
                    int worldY = y * Chunk.Y_LENGTH + tileY;
                    for (int tileX = 0; tileX < Chunk.X_LENGTH; tileX++)
                    {
                        int worldX = x * Chunk.X_LENGTH + tileX;

                        if (worldY <= 64 && rand.Next(0, 100) < 98) tiles[tileZ, tileY, tileX] = Tile.stone.ID;
                    }
                }
            }

            return tiles;
        }
    }
}
