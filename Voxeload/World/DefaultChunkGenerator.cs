using OpenTK.Mathematics;

using SimplexNoise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.World
{
    public class DefaultChunkGenerator : IChunkGenerator
    {
        JavaImports.Random rand;

        private static int baseSeed = (int)DateTime.Now.Ticks;

        public byte[,,,] GenerateChunk(ref List<(Vector3i pos, byte tile)> structureBuffer, int chunkX, int chunkY, int chunkZ)
        {
            rand = new(baseSeed);

            byte[,,,] tiles = new byte[Chunk.LAYER_COUNT, Chunk.Z_LENGTH, Chunk.Y_LENGTH, Chunk.X_LENGTH];

            for (int tileZ = 0; tileZ < Chunk.Z_LENGTH; tileZ++)
            {
                int worldZ = chunkZ * Chunk.Z_LENGTH + tileZ;
                for (int tileX = 0; tileX < Chunk.X_LENGTH; tileX++)
                {
                    int worldX = chunkX * Chunk.X_LENGTH + tileX;

                    // Generate base values
                    Noise.Seed = (int)(baseSeed ^ 0x1fc65241);
                    float elevation = Noise.CalcPixel2D(worldX, worldZ, 0.002f) / 512.0f - 0.25f;

                    Noise.Seed = (int)(baseSeed ^ 0x94ae628d);
                    float roughness = Noise.CalcPixel2D(worldX, worldZ, 0.005f) / 256.0f - 0.5f;

                    Noise.Seed = (int)(baseSeed ^ 0xfc6d5abb);
                    float detail = Noise.CalcPixel2D(worldX, worldZ, 0.05f) / 1024.0f - 0.125f;

                    int baseY = (int)((elevation + (roughness * detail)) * 64 + 64);

                    Noise.Seed = (int)(baseSeed ^ 0x730c659a);
                    if (Noise.CalcPixel2D(worldX, worldZ, 0.005f) < 96)
                    {
                        Noise.Seed = (int)(baseSeed ^ 0xcf02e105);
                        int offset = (int)((Noise.CalcPixel2D(worldX, worldZ, 0.01f) / 256.0f) * 12);
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
                                    tiles[0, tileZ, tileY, tileX] = Tile.sand.ID;
                                }
                                else
                                {
                                    tiles[0, tileZ, tileY, tileX] = Tile.gravel.ID;
                                }

                            }
                            else
                            {
                                tiles[0, tileZ, tileY, tileX] = Tile.dirt.ID;
                            }
                        }

                        if (myTileY + 2 >= 0 && myTileY + 2 < Chunk.Y_LENGTH)
                        {
                            if (baseY < 64)
                            {
                                if (Noise.CalcPixel2D(worldX, worldZ, 0.1f) < 128.0f)
                                {
                                    tiles[0, tileZ, myTileY + 2, tileX] = Tile.sand.ID;
                                }
                                else
                                {
                                    tiles[0, tileZ, myTileY + 2, tileX] = Tile.gravel.ID;
                                }
                            }
                            else
                            {
                                tiles[0, tileZ, myTileY + 2, tileX] = Tile.grass.ID;
                            }
                        }
                    }
                    else if (baseChunkY > chunkY)
                    {
                        for (int tileY = 0; tileY < Chunk.Y_LENGTH; tileY++)
                        {
                            tiles[0, tileZ, tileY, tileX] = Tile.stone.ID;
                        }
                    }
                    else
                    {
                        for (int tileY = 0; tileY < baseTileY; tileY++)
                        {
                            tiles[0, tileZ, tileY, tileX] = Tile.stone.ID;
                        }

                        for (int tileY = baseTileY; tileY < baseTileY + 2 && tileY < Chunk.Y_LENGTH; tileY++)
                        {
                            if (baseY < 64)
                            {
                                if (Noise.CalcPixel2D(worldX, worldZ, 0.1f) < 128.0f)
                                {
                                    tiles[0, tileZ, tileY, tileX] = Tile.sand.ID;
                                }
                                else
                                {
                                    tiles[0, tileZ, tileY, tileX] = Tile.gravel.ID;
                                }

                            }
                            else
                            {
                                tiles[0, tileZ, tileY, tileX] = Tile.dirt.ID;
                            }
                        }

                        if (baseTileY + 2 < Chunk.Y_LENGTH)
                        {
                            if (baseY < 64)
                            {
                                if (Noise.CalcPixel2D(worldX, worldZ, 0.1f) < 128.0f)
                                {
                                    tiles[0, tileZ, baseTileY + 2, tileX] = Tile.sand.ID;
                                }
                                else
                                {
                                    tiles[0, tileZ, baseTileY + 2, tileX] = Tile.gravel.ID;
                                }
                            }
                            else
                            {
                                tiles[0, tileZ, baseTileY + 2, tileX] = Tile.grass.ID;
                            }
                        }
                    }

                    // Do water
                    for (int tileY = 0; tileY < Chunk.Y_LENGTH; tileY++)
                    {
                        int worldY = chunkY * Chunk.Y_LENGTH + tileY;

                        if (worldY < 64 && tiles[0, tileZ, tileY, tileX] == 0)
                        {
                            tiles[1, tileZ, tileY, tileX] = Tile.water.ID;
                        }
                    }

                    // Do cave carving
                    for (int tileY = 0; tileY < Chunk.Y_LENGTH; tileY++)
                    {
                        int worldY = chunkY * Chunk.Y_LENGTH + tileY;

                        if (worldY == 0) continue;

                        if (Noise.CalcPixel3D(worldX, worldY, worldZ, 0.1f) > 128 && Noise.CalcPixel3D(worldX, worldY, worldZ, 0.02f) > 192)
                        {
                            tiles[0, tileZ, tileY, tileX] = 0;
                        }
                    }

                    int pix = (int)Noise.CalcPixel2D(worldX, worldZ, 0.005f);
                    if (Noise.CalcPixel2D(worldX, worldZ, 0.03f) > 128 &&  rand.NextInt(pix) >= pix - 1)
                    {

                        if (baseTileY + 2 > 0 && baseTileY + 2 < Chunk.Y_LENGTH - 1 && tiles[0, tileZ, baseTileY + 2, tileX] == Tile.grass.ID)
                        {
                            // Place tree
                            TreeStructure tree = new(rand, new(worldX, worldZ, baseY + 2));

                            PlaceStructure(ref structureBuffer, tree, new(worldX, baseY + 3, worldZ));

                        }
                    }
                }
            }

            return tiles;
        }

        protected void PlaceStructure(ref List<(Vector3i pos, byte tile)> structureBuffer, BaseStructure structure, Vector3i origin)
        {
            for (int z = 0; z < structure.LengthZ; z++)
            {
                for (int y = 0; y < structure.LengthY; y++)
                {
                    for (int x = 0; x < structure.LengthX; x++)
                    {
                        Vector3i pos = new(x + origin.X + structure.Offset.X, y + origin.Y + structure.Offset.Y, z + origin.Z + structure.Offset.Z);
                        byte tile = structure.GetTileAt(new(x, y, z)).ID;

                        if (tile == 0) continue;

                        structureBuffer.Add((pos, tile));
                    }
                }
            }
        }
    }
}
