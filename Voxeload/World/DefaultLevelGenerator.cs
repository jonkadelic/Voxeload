using OpenTK.Graphics.ES30;
using SimplexNoise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.World
{
    public class DefaultLevelGenerator : ILevelGenerator
    {
        Random rand = new();

        private int baseSeed = (int)DateTime.Now.Ticks;

        public byte[,,] GenerateChunk(int chunkX, int chunkY, int chunkZ)
        {
            byte[,,] tiles = new byte[Chunk.X_LENGTH, Chunk.Y_LENGTH, Chunk.Z_LENGTH];

            for (int tileZ = 0; tileZ < Chunk.Z_LENGTH; tileZ++)
            {
                int worldZ = chunkZ * Chunk.Z_LENGTH + tileZ;
                for (int tileX = 0; tileX < Chunk.X_LENGTH; tileX++)
                {
                    int worldX = chunkX * Chunk.X_LENGTH + tileX;

                    // Generate base values
                    Noise.Seed = (int)(baseSeed ^ 0x1fc65241);
                    float elevation = Noise.CalcPixel2D(worldX, worldZ, 0.001f) / 256.0f - 0.25f;

                    Noise.Seed = (int)(baseSeed ^ 0x94ae628d);
                    float roughness = Noise.CalcPixel2D(worldX, worldZ, 0.005f) / 256.0f - 0.5f;

                    Noise.Seed = (int)(baseSeed ^ 0xfc6d5abb);
                    float detail = Noise.CalcPixel2D(worldX, worldZ, 0.1f) / 1024.0f - 0.125f;

                    int baseY = (int)((elevation + (roughness * detail)) * 64 + 64);

                    Noise.Seed = (int)(baseSeed ^ 0x730c659a);
                    if (Noise.CalcPixel2D(worldX, worldZ, 0.005f) < 96)
                    {
                        Noise.Seed = (int)(baseSeed ^ 0xcf02e105);
                        int offset = (int)((Noise.CalcPixel2D(worldX, worldZ, 0.01f) / 256.0f) * 8);
                        baseY += offset;
                    }

                    int baseChunkY = baseY / Chunk.Y_LENGTH;
                    int baseTileY = baseY % Chunk.Y_LENGTH;

                    // Perform stone, dirt/sand/gravel and grass layering
                    if (baseChunkY < chunkY)
                    {
                        int myTileY = baseTileY - (chunkY - baseChunkY) * Chunk.Y_LENGTH;
                        for (int tileY = 0; tileY < myTileY + 2; tileY++)
                        {
                            if (baseY < 64)
                            {
                                if (Noise.CalcPixel2D(worldX, worldZ, 0.1f) < 128.0f)
                                {
                                    tiles[tileZ, tileY, tileX] = Tile.sand.ID;
                                }
                                else
                                {
                                    tiles[tileZ, tileY, tileX] = Tile.gravel.ID;
                                }

                            }
                            else
                            {
                                tiles[tileZ, tileY, tileX] = Tile.dirt.ID;
                            }
                        }

                        if (myTileY + 2 >= 0 && myTileY + 2 < Chunk.Y_LENGTH)
                        {
                            if (baseY < 64)
                            {
                                if (Noise.CalcPixel2D(worldX, worldZ, 0.1f) < 128.0f)
                                {
                                    tiles[tileZ, myTileY + 2, tileX] = Tile.sand.ID;
                                }
                                else
                                {
                                    tiles[tileZ, myTileY + 2, tileX] = Tile.gravel.ID;
                                }
                            }
                            else
                            {
                                tiles[tileZ, myTileY + 2, tileX] = Tile.grass.ID;
                            }
                        }
                    }
                    else if (baseChunkY > chunkY)
                    {
                        for (int tileY = 0; tileY < Chunk.Y_LENGTH; tileY++)
                        {
                            tiles[tileZ, tileY, tileX] = Tile.stone.ID;
                        }
                    }
                    else
                    {
                        for (int tileY = 0; tileY < baseTileY; tileY++)
                        {
                            tiles[tileZ, tileY, tileX] = Tile.stone.ID;
                        }

                        for (int tileY = baseTileY; tileY < baseTileY + 2 && tileY < Chunk.Y_LENGTH; tileY++)
                        {
                            if (baseY < 64)
                            {
                                if (Noise.CalcPixel2D(worldX, worldZ, 0.1f) < 128.0f)
                                {
                                    tiles[tileZ, tileY, tileX] = Tile.sand.ID;
                                }
                                else
                                {
                                    tiles[tileZ, tileY, tileX] = Tile.gravel.ID;
                                }

                            }
                            else
                            {
                                tiles[tileZ, tileY, tileX] = Tile.dirt.ID;
                            }
                        }

                        if (baseTileY + 2 < Chunk.Y_LENGTH)
                        {
                            if (baseY < 64)
                            {
                                if (Noise.CalcPixel2D(worldX, worldZ, 0.1f) < 128.0f)
                                {
                                    tiles[tileZ, baseTileY + 2, tileX] = Tile.sand.ID;
                                }
                                else
                                {
                                    tiles[tileZ, baseTileY + 2, tileX] = Tile.gravel.ID;
                                }
                            }
                            else
                            {
                                tiles[tileZ, baseTileY + 2, tileX] = Tile.grass.ID;
                            }
                        }
                    }

                    // Do cave carving
                    for (int tileY = 0; tileY < Chunk.Y_LENGTH; tileY++)
                    {
                        int worldY = chunkY * Chunk.Y_LENGTH + tileY;

                        if (worldY == 0) continue;

                        if (Noise.CalcPixel3D(worldX, worldY, worldZ, 0.1f) > 128 && Noise.CalcPixel3D(worldX, worldY, worldZ, 0.02f) > 192)
                        {
                            tiles[tileZ, tileY, tileX] = 0;
                        }
                    }

                }
            }

            return tiles;
        }
    }
}
