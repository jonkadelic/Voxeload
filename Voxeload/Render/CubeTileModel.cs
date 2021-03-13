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
    public class CubeTileModel : BaseTileModel
    {
        private static readonly Vector3[] allVertices =
{
                new(0.0f, 0.0f, 0.0f),
                new(0.0f, 0.0f, 1.0f),
                new(0.0f, 1.0f, 0.0f),
                new(0.0f, 1.0f, 1.0f),
                new(1.0f, 0.0f, 0.0f),
                new(1.0f, 0.0f, 1.0f),
                new(1.0f, 1.0f, 0.0f),
                new(1.0f, 1.0f, 1.0f)
            };
        private static readonly byte[,] faceMaps = { { 0, 2, 4, 6 }, { 1, 3, 5, 7 }, { 0, 1, 4, 5 }, { 2, 3, 6, 7 }, { 0, 1, 2, 3 }, { 4, 5, 6, 7 } };

        protected static readonly Model[] models;

        public override Model GetModel(byte sides)
        {
            if (sides > models.Length) throw new ArgumentException("Requested an invalid cube model!", nameof(sides));

            return models[sides];
        }

        public override byte[] GetColours(ITileAppearance appearance, byte sides)
        {
            Model model = GetModel(sides);
            List<byte> colours = new();
            Random rand = new();

            foreach (Vector3 _ in model.Vertices)
            {
                colours.AddRange(BitConverter.GetBytes(rand.Next(0, 0x01000000) | 0xFF000000));//appearance[Tile.Face.Top]));
            }

            return colours.ToArray();
        }

        static CubeTileModel()
        {
            models = new Model[1 << 6];

            for (byte i = 0; i < models.Length; i++)
            {
                List<Vector3> vertices = new();
                List<uint> indices = new();

                for (byte face = 0; face < 6; face++)
                {
                    if ((i & (1 << face)) > 0)
                    {
                        for (int vert = 0; vert < 4; vert++)
                        {
                            Vector3 vtex = allVertices[faceMaps[face, vert]];
                            if (!vertices.Contains(vtex)) vertices.Add(vtex);
                        }

                        if (face == 1 || face == 2 || face == 5)
                        {
                            indices.Add((uint)vertices.IndexOf(allVertices[faceMaps[face, 2]]));
                            indices.Add((uint)vertices.IndexOf(allVertices[faceMaps[face, 1]]));
                            indices.Add((uint)vertices.IndexOf(allVertices[faceMaps[face, 0]]));
                            indices.Add((uint)vertices.IndexOf(allVertices[faceMaps[face, 1]]));
                            indices.Add((uint)vertices.IndexOf(allVertices[faceMaps[face, 2]]));
                            indices.Add((uint)vertices.IndexOf(allVertices[faceMaps[face, 3]]));
                        }
                        else
                        {
                            indices.Add((uint)vertices.IndexOf(allVertices[faceMaps[face, 0]]));
                            indices.Add((uint)vertices.IndexOf(allVertices[faceMaps[face, 1]]));
                            indices.Add((uint)vertices.IndexOf(allVertices[faceMaps[face, 2]]));
                            indices.Add((uint)vertices.IndexOf(allVertices[faceMaps[face, 3]]));
                            indices.Add((uint)vertices.IndexOf(allVertices[faceMaps[face, 2]]));
                            indices.Add((uint)vertices.IndexOf(allVertices[faceMaps[face, 1]]));
                        }


                    }
                }

                models[i] = new Model(vertices.ToArray(), indices.ToArray());
            }

        }
    }
}
