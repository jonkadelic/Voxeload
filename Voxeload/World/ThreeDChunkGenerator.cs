using OpenTK.Mathematics;

using SimplexNoise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.World
{
    public class ThreeDChunkGenerator : IChunkGenerator
    {
        Random rand = new();

        private static int baseSeed = (int)DateTime.Now.Ticks;

        public byte[,,,] GenerateChunk(ref List<(Vector3i pos, byte tile)> structureBuffer, int chunkX, int chunkY, int chunkZ)
        {
            byte[,,,] tiles = new byte[Chunk.LAYER_COUNT, Chunk.X_LENGTH, Chunk.Y_LENGTH, Chunk.Z_LENGTH];

            int xScale = 4;
            int yScale = 8;
            int zScale = 4;
            
            float[,,] noise = PerlinGenerator.GetNoise(baseSeed, (chunkX * Chunk.X_LENGTH) / xScale, (chunkY * Chunk.Y_LENGTH) / yScale, (chunkZ * Chunk.Z_LENGTH) / zScale, 
                ((chunkX + 1) * Chunk.X_LENGTH) / xScale, ((chunkY + 1) * Chunk.Y_LENGTH) / yScale, ((chunkZ + 1) * Chunk.Z_LENGTH) / zScale, 0.009f);

            float currentXDensity = 0;
            float currentYDensity = 0;
            float currentZDensity = 0;

            for (int tileZ = 0; tileZ < Chunk.Z_LENGTH; tileZ++)
            {
                int worldZ = chunkZ * Chunk.Z_LENGTH + tileZ;

                for (int tileX = 0; tileX < Chunk.X_LENGTH; tileX++)
                {
                    int worldX = chunkX * Chunk.X_LENGTH + tileX;

                    int baseY = 0;

                    for (int tileY = 0; tileY < Chunk.Y_LENGTH; tileY++)
                    {
                        int worldY = chunkY * Chunk.Y_LENGTH + tileY;

                        float baseDensity = Noise.CalcPixel3D(worldX, worldY + baseY, worldZ, 0.009f) / 128.0f - 1.0f;

                        float modifier = (32 - (worldY - 64)) / 32.0f;

                        float moddedDensity = baseDensity + modifier;

                        if (moddedDensity >= -0.2f)
                        {
                            tiles[0, tileZ, tileY, tileX] = Tile.stone.ID;
                        }
                        else
                        {
                            tiles[0, tileZ, tileY, tileX] = 0;
                        }
                    }

                }
            }

            return tiles;
        }
    }
}
