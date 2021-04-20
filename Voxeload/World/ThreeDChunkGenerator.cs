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
            Noise.Seed = baseSeed;

            for (int tileZ = 0; tileZ < Chunk.Z_LENGTH; tileZ++)
            {
                int worldZ = chunkZ * Chunk.Z_LENGTH + tileZ;
                for (int tileX = 0; tileX < Chunk.X_LENGTH; tileX++)
                {
                    int worldX = chunkX * Chunk.X_LENGTH + tileX;

                    // Generate base values
                    Noise.Seed = (int)(baseSeed ^ 0x1fc65241);
                    float elevation = Noise.CalcPixel2D(worldX, worldZ, 0.001f) / 512.0f - 0.25f;

                    Noise.Seed = (int)(baseSeed ^ 0x94ae628d);
                    float roughness = Noise.CalcPixel2D(worldX, worldZ, 0.005f) / 256.0f - 0.5f;

                    Noise.Seed = (int)(baseSeed ^ 0xfc6d5abb);
                    float detail = Noise.CalcPixel2D(worldX, worldZ, 0.05f) / 1024.0f - 0.125f;

                    Noise.Seed = baseSeed;

                    int baseY = (int)((elevation + (roughness * detail)) * 64 + 64);

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
