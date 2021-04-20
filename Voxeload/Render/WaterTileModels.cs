using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxeload.World;

namespace Voxeload.Render
{
    public class WaterTileModels : BaseTileModels
    {
        private static readonly Vector3[] allVertices =
        {
            new(0.0f, 0.0f, 0.0f),
            new(0.0f, 0.0f, 1.0f),
            new(0.0f, 0.875f, 0.0f),
            new(0.0f, 0.875f, 1.0f),
            new(1.0f, 0.0f, 0.0f),
            new(1.0f, 0.0f, 1.0f),
            new(1.0f, 0.875f, 0.0f),
            new(1.0f, 0.875f, 1.0f)
        };
        private static readonly byte[,] faceMaps = { { 0, 2, 4, 6 }, { 1, 3, 5, 7 }, { 0, 1, 4, 5 }, { 2, 3, 6, 7 }, { 0, 1, 2, 3 }, { 4, 5, 6, 7 } };
        private static readonly Vector2[,] allUVs =
        {
            {
                new(1.0f, 0.0f),
                new(1.0f, 1.0f),
                new(0.0f, 0.0f),
                new(0.0f, 1.0f),
                new(0.0f, 0.0f),
                new(1.0f, 1.0f)
            },
            {
                new(1.0f, 0.0f),
                new(0.0f, 1.0f),
                new(0.0f, 0.0f),
                new(0.0f, 1.0f),
                new(1.0f, 0.0f),
                new(1.0f, 1.0f)
            },
            {
                new(1.0f, 1.0f),
                new(0.0f, 0.0f),
                new(0.0f, 1.0f),
                new(0.0f, 0.0f),
                new(1.0f, 1.0f),
                new(1.0f, 0.0f)
            },
            {
                new(0.0f, 1.0f),
                new(0.0f, 0.0f),
                new(1.0f, 1.0f),
                new(1.0f, 0.0f),
                new(1.0f, 1.0f),
                new(0.0f, 0.0f)
            },
            {
                new(0.0f, 0.0f),
                new(1.0f, 0.0f),
                new(0.0f, 1.0f),
                new(1.0f, 1.0f),
                new(0.0f, 1.0f),
                new(1.0f, 0.0f)
            },
            {
                new(1.0f, 1.0f),                
                new(0.0f, 0.0f),
                new(1.0f, 0.0f),
                new(0.0f, 0.0f),
                new(1.0f, 1.0f),
                new(0.0f, 1.0f)
            },
        };

        protected static readonly TileModel[] models;

        public override TileModel GetModel(byte sides)
        {
            if (sides > models.Length) throw new ArgumentException("Requested an invalid cube model!", nameof(sides));

            return models[sides];
        }

        static WaterTileModels()
        {
            models = new TileModel[1 << 6];

            for (byte i = 0; i < models.Length; i++)
            {
                List<Vector3> vertices = new();
                List<Vector2> uvs = new();
                List<byte> uvFaces = new();

                for (byte face = 0; face < 6; face++)
                {
                    if ((i & (1 << face)) > 0)
                    {
                        if (face == 1 || face == 2 || face == 5)
                        {
                            vertices.Add(allVertices[faceMaps[face, 2]]);
                            vertices.Add(allVertices[faceMaps[face, 1]]);
                            vertices.Add(allVertices[faceMaps[face, 0]]);
                            vertices.Add(allVertices[faceMaps[face, 1]]);
                            vertices.Add(allVertices[faceMaps[face, 2]]);
                            vertices.Add(allVertices[faceMaps[face, 3]]);
                        }
                        else
                        {
                            vertices.Add(allVertices[faceMaps[face, 0]]);
                            vertices.Add(allVertices[faceMaps[face, 1]]);
                            vertices.Add(allVertices[faceMaps[face, 2]]);
                            vertices.Add(allVertices[faceMaps[face, 3]]);
                            vertices.Add(allVertices[faceMaps[face, 2]]);
                            vertices.Add(allVertices[faceMaps[face, 1]]);
                        }

                        uvs.Add(allUVs[face, 0]);
                        uvs.Add(allUVs[face, 1]);
                        uvs.Add(allUVs[face, 2]);
                        uvs.Add(allUVs[face, 3]);
                        uvs.Add(allUVs[face, 4]);
                        uvs.Add(allUVs[face, 5]);

                        for (int j = 0; j < 6; j++)
                        {
                            uvFaces.Add((byte)(1 << face));
                        }

                        if (face == 3)
                        {
                            vertices.Add(new(1.0f, 0.875f, 0.0f));
                            vertices.Add(new(0.0f, 0.875f, 1.0f));
                            vertices.Add(new(0.0f, 0.875f, 0.0f));
                            vertices.Add(new(0.0f, 0.875f, 1.0f));
                            vertices.Add(new(1.0f, 0.875f, 0.0f));
                            vertices.Add(new(1.0f, 0.875f, 1.0f));

                            uvs.Add(allUVs[2, 0]);
                            uvs.Add(allUVs[2, 1]);
                            uvs.Add(allUVs[2, 2]);
                            uvs.Add(allUVs[2, 3]);
                            uvs.Add(allUVs[2, 4]);
                            uvs.Add(allUVs[2, 5]);

                            for (int j = 0; j < 6; j++)
                            {
                                uvFaces.Add((byte)(1 << 2));
                            }
                        }
                    }
                }

                models[i] = new TileModel(vertices.ToArray(), uvs.ToArray(), uvFaces.ToArray());
            }

        }
    }
}
