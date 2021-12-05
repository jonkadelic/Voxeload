using SimplexNoise;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.World
{
    public class PerlinGenerator
    {
        public static float[,,] GetNoise(int seed, int minX, int minY, int minZ, int maxX, int maxY, int maxZ, float scale)
        {
            int width = maxX - minX;
            int height = maxY - minY;
            int depth = maxZ - minZ;

            Noise.Seed = seed;

            float[,,] data = new float[width, height, depth];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        data[x, y, z] = Noise.CalcPixel3D(minX + x, minY + y, minZ + z, scale);
                    }
                }
            }

            return data;
        }
    }
}
