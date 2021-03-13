using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxeload.Entities;

namespace Voxeload.World
{
    public class Level : ITileAccess
    {
        public const int X_LENGTH = 32;
        public const int Z_LENGTH = 32;

        public readonly Chunk[,] chunks = new Chunk[Z_LENGTH, X_LENGTH];
        private IChunkGenerator generator;
        private Queue<(int x, int z)> chunksToGenerate = new();

        public Level(IChunkGenerator generator)
        {
            this.generator = generator;
        }

        public Chunk GetChunk(int x, int z)
        {
            if (x < 0 || x >= X_LENGTH) return null;
            if (z < 0 || z >= Z_LENGTH) return null;

            Chunk chunk = chunks[z, x];

            if (chunk == null && chunksToGenerate.Contains((x, z)) == false) chunksToGenerate.Enqueue((x, z));

            return chunk;
        }

        public void GenerateNextChunks()
        {
            if (chunksToGenerate.Count == 0) return;

            int numToGenerate = chunksToGenerate.Count / 5;
            if (numToGenerate == 0) numToGenerate = 1;

            for (int i = 0; i < numToGenerate; i++)
            {
                (int x, int z) = chunksToGenerate.Dequeue();

                chunks[z, x] = new(this, generator, x, z);
            }
        }

        public byte GetTileID(int x, int y, int z)
        {
            if (x < 0 || x >= Level.X_LENGTH * Chunk.X_LENGTH) return 0;
            if (y < 0 || y >= Chunk.Y_LENGTH) return 0;
            if (z < 0 || z >= Level.Z_LENGTH * Chunk.Z_LENGTH) return 0;

            int chunkX = x / Chunk.X_LENGTH;
            int chunkZ = z / Chunk.Z_LENGTH;
            int tileX = x % Chunk.X_LENGTH;
            int tileZ = z % Chunk.Z_LENGTH;

            if (chunkX < 0 || chunkX >= Level.X_LENGTH) return 0;
            if (chunkZ < 0 || chunkZ >= Level.Z_LENGTH) return 0; 

            Chunk chunk = chunks[chunkZ, chunkX];
            if (chunk == null) return 0;

            return chunk.GetTileID(tileX, y, tileZ);
        }

        public void SetTileID(int x, int y, int z, byte id)
        {
            if (x < 0 || x >= Level.X_LENGTH * Chunk.X_LENGTH) return;
            if (y < 0 || y >= Chunk.Y_LENGTH) return;
            if (z < 0 || z >= Level.Z_LENGTH * Chunk.Z_LENGTH) return;

            int chunkX = x / Chunk.X_LENGTH;
            int chunkZ = z / Chunk.Z_LENGTH;
            int tileX = x % Chunk.X_LENGTH;
            int tileZ = z % Chunk.Z_LENGTH;

            Chunk chunk = chunks[chunkZ, chunkX];
            if (chunk == null) return;

            chunk.SetTileID(tileX, y, tileZ, id);

            if (tileX == 0 && chunkX > 0)
            {
                Chunk otherChunk = chunks[chunkZ, chunkX - 1];
                if (otherChunk != null) otherChunk.IsDirty = true;
            }
            else if (tileX == Chunk.X_LENGTH - 1 && chunkX < Level.X_LENGTH - 1)
            {
                Chunk otherChunk = chunks[chunkZ, chunkX + 1];
                if (otherChunk != null) otherChunk.IsDirty = true;
            }
            if (tileZ == 0 && chunkZ > 0)
            {
                Chunk otherChunk = chunks[chunkZ - 1, chunkX];
                if (otherChunk != null) otherChunk.IsDirty = true;
            }
            else if (tileZ == Chunk.Z_LENGTH - 1 && chunkZ < Level.Z_LENGTH - 1)
            {
                Chunk otherChunk = chunks[chunkZ + 1, chunkX];
                if (otherChunk != null) otherChunk.IsDirty = true;
            }
        }

        public byte GetVisibleSides(int x, int y, int z)
        {
            byte minusZ = GetTileID(x, y, z - 1);
            byte plusZ = GetTileID(x, y, z + 1);
            byte minusY = GetTileID(x, y - 1, z);
            byte plusY = GetTileID(x, y + 1, z);
            byte minusX = GetTileID(x - 1, y, z);
            byte plusX = GetTileID(x + 1, y, z);

            byte sides = 0;

            if (minusZ == 0) sides |= 1 << 0;
            if (plusZ == 0) sides |= 1 << 1;
            if (minusY == 0) sides |= 1 << 2;
            if (plusY == 0) sides |= 1 << 3;
            if (minusX == 0) sides |= 1 << 4;
            if (plusX == 0) sides |= 1 << 5;

            return sides;
        }

        public List<AABB> GetAABBs(AABB aabb)
        {
            List<AABB> aabbs = new();

            Vector3i a = new((int)aabb.A.X, (int)aabb.A.Y, (int)aabb.A.Z);
            Vector3i b = new((int)(aabb.B.X + 1), (int)(aabb.B.Y + 1), (int)(aabb.B.Z + 1));

            if (a.X < 0) a.X = 0;
            if (a.Y < 0) a.Y = 0;
            if (a.Z < 0) a.Z = 0;
            if (b.X > Level.X_LENGTH * Chunk.X_LENGTH) b.X = Level.X_LENGTH * Chunk.X_LENGTH;
            if (b.Y > Chunk.Y_LENGTH) b.Y = Chunk.Y_LENGTH;
            if (b.Z > Level.Z_LENGTH * Chunk.Z_LENGTH) b.Z = Level.Z_LENGTH * Chunk.Z_LENGTH;

            for (int z = a.Z; z < b.Z; z++)
            {
                for (int y = a.Y; y < b.Y; y++)
                {
                    for (int x = a.X; x < b.X; x++)
                    {
                        byte tile = GetTileID(x, y, z);
                        if (tile == 0) continue;

                        AABB c = new(new(x, y, z), new(x + 1, y + 1, z + 1));

                        aabbs.Add(c);
                    }
                }
            }

            return aabbs;
        }
    }
}
