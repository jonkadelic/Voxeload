﻿using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Voxeload.World;

namespace Voxeload.Render
{
    public class ChunkRenderExchangeQueue : ExchangeQueue<Chunk, (Chunk, ChunkModel[])>
    {
        protected Level level;

        public ChunkRenderExchangeQueue(Level level) : base("ChunkRenderExchangeQueue", ThreadPriority.AboveNormal)
        {
            this.level = level;
        }

        protected override (Chunk, ChunkModel[]) Process(Chunk chunk)
        {

            Console.WriteLine($"Regenerating chunk model at {chunk.X}, {chunk.Y}, {chunk.Z}");

            List<ChunkModel> models = new();

            lock (chunk.chunkDataLock)
            {
                for (int layer = 0; layer < Chunk.LAYER_COUNT; layer++)
                {
                    List<Vector3> vertices = new();
                    List<Vector2> uvs = new();
                    List<byte> faces = new();
                    List<float> brightnesses = new();
                    for (int z = 0; z < Chunk.Z_LENGTH; z++)
                    {
                        for (int y = 0; y < Chunk.Y_LENGTH; y++)
                        {
                            for (int x = 0; x < Chunk.X_LENGTH; x++)
                            {
                                byte sides = level.GetVisibleSides(layer, (chunk.X * Chunk.X_LENGTH) + x, (chunk.Y * Chunk.Y_LENGTH) + y, (chunk.Z * Chunk.Z_LENGTH) + z);
                                byte id = chunk.GetTileID(layer, x, y, z);

                                if (sides == 0 || id == 0) continue;

                                Tile tile = Tile.tiles[id];

                                if (tile == null) continue;

                                Vector3 offset = new(x, y, z);

                                TileModel model = tile.TileModel.GetModel(sides);

                                foreach (Vector3 vert in model.Vertices)
                                {
                                    vertices.Add(vert + offset);
                                }

                                for (int i = 0; i < model.UVs.Length; i++)
                                {
                                    int index = (int)tile.TileAppearance[(Tile.Face)model.UVFaces[i]];
                                    int texX = index % 16;
                                    int texY = 15 - (index / 16);

                                    uvs.Add(new((model.UVs[i].X + texX) / 16, (model.UVs[i].Y + texY) / 16));
                                }

                                faces.AddRange(model.UVFaces);

                                foreach (byte face in model.UVFaces)
                                {
                                    float brightness = 1.0f;

                                    if (face == (byte)Tile.Face.North)
                                    {
                                        brightness = level.GetBrightness((chunk.X * Chunk.X_LENGTH + x), chunk.Y * Chunk.Y_LENGTH + y, chunk.Z * Chunk.Z_LENGTH + z - 1);
                                    }
                                    else if (face == (byte)Tile.Face.South)
                                    {
                                        brightness = level.GetBrightness((chunk.X * Chunk.X_LENGTH + x), chunk.Y * Chunk.Y_LENGTH + y, chunk.Z * Chunk.Z_LENGTH + z + 1);
                                    }
                                    else if (face == (byte)Tile.Face.Bottom)
                                    {
                                        brightness = level.GetBrightness(chunk.X * Chunk.X_LENGTH + x, (chunk.Y * Chunk.Y_LENGTH + y) - 1, chunk.Z * Chunk.Z_LENGTH + z);
                                    }
                                    else if (face == (byte)Tile.Face.Top)
                                    {
                                        brightness = level.GetBrightness(chunk.X * Chunk.X_LENGTH + x, (chunk.Y * Chunk.Y_LENGTH + y) + 1, chunk.Z * Chunk.Z_LENGTH + z);
                                    }
                                    else if (face == (byte)Tile.Face.West)
                                    {
                                        brightness = level.GetBrightness(chunk.X * Chunk.X_LENGTH + x - 1, chunk.Y * Chunk.Y_LENGTH + y, (chunk.Z * Chunk.Z_LENGTH + z));
                                    }
                                    else if (face == (byte)Tile.Face.East)
                                    {
                                        brightness = level.GetBrightness(chunk.X * Chunk.X_LENGTH + x + 1, chunk.Y * Chunk.Y_LENGTH + y, (chunk.Z * Chunk.Z_LENGTH + z));
                                    }

                                    brightnesses.Add(brightness);
                                }
                            }
                        }
                    }

                    models.Add(new(chunk, vertices.ToArray(), uvs.ToArray(), faces.ToArray(), brightnesses.ToArray()));
                    chunk.IsDirty[layer] = false;
                }
            }

            return (chunk, models.ToArray());
        }
    }
}
