using OpenTK.Mathematics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.World
{
    internal class FlatChunkGenerator : IChunkGenerator
    {
        public byte[,,,] GenerateChunk(ref List<(Vector3i pos, byte tile)> structureBuffer, int chunkX, int chunkY, int chunkZ)
        {
            byte[,,,] tiles = new byte[Chunk.LAYER_COUNT, Chunk.Z_LENGTH, Chunk.Y_LENGTH, Chunk.X_LENGTH];

            for (int x = 0; x < Chunk.X_LENGTH; x++)
            {
                for (int z = 0; z < Chunk.Z_LENGTH; z++)
                {
                    for (int y = 0; y < Chunk.Y_LENGTH; y++)
                    {
                        if (chunkY * Chunk.Y_LENGTH + y < 16)
                        {
                            tiles[0, z, y, x] = Tile.stone.ID;
                        }
                    }
                }
            }

            return tiles;
        }
    }
}
