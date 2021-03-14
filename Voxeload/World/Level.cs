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
        public const int X_LENGTH = 16;
        public const int Y_LENGTH = 4;
        public const int Z_LENGTH = 16;

        public readonly Chunk[,,] chunks = new Chunk[Z_LENGTH, Y_LENGTH, X_LENGTH];
        private ChunkGenExchangeQueue generator;
        private Queue<(int x, int y, int z)> chunksToGenerate = new();

        public Level(IChunkGenerator generator)
        {
            this.generator = new(this, generator);
        }

        public Chunk GetChunk(int x, int y, int z)
        {
            if (x < 0 || x >= X_LENGTH) return null;
            if (y < 0 || y >= Y_LENGTH) return null;
            if (z < 0 || z >= Z_LENGTH) return null;

            Chunk chunk = chunks[z, y, x];

            if (chunk == null && chunksToGenerate.Contains((x, y, z)) == false) chunksToGenerate.Enqueue((x, y, z));

            return chunk;
        }

        public void GenerateNextChunks()
        {
            int counter = 0;
            while (chunksToGenerate.Count > 0 && counter < 1)
            {
                (int x, int y, int z) = chunksToGenerate.Dequeue();

                generator.Request(new(x, y, z));

                counter++;
            }

            Chunk chunk;
            counter = 0;
            while (counter < 1 && (chunk = generator.Receive()) != null)
            {
                chunks[chunk.Z, chunk.Y, chunk.X] = chunk;
                counter++;
            }
        }

        public byte GetTileID(int layer, int x, int y, int z)
        {
            if (x < 0 || x >= Level.X_LENGTH * Chunk.X_LENGTH) return 0;
            if (y < 0 || y >= Level.Y_LENGTH * Chunk.Y_LENGTH) return 0;
            if (z < 0 || z >= Level.Z_LENGTH * Chunk.Z_LENGTH) return 0;

            int chunkX = x / Chunk.X_LENGTH;
            int chunkY = y / Chunk.Y_LENGTH;
            int chunkZ = z / Chunk.Z_LENGTH;
            int tileX = x % Chunk.X_LENGTH;
            int tileY = y % Chunk.Y_LENGTH;
            int tileZ = z % Chunk.Z_LENGTH;

            if (chunkX < 0 || chunkX >= Level.X_LENGTH) return 0;
            if (chunkY < 0 || chunkY >= Level.X_LENGTH) return 0;
            if (chunkZ < 0 || chunkZ >= Level.Z_LENGTH) return 0; 

            Chunk chunk = chunks[chunkZ, chunkY, chunkX];
            if (chunk == null) return 0;

            return chunk.GetTileID(layer, tileX, tileY, tileZ);
        }

        public void SetTileID(int layer, int x, int y, int z, byte id)
        {
            if (x < 0 || x >= Level.X_LENGTH * Chunk.X_LENGTH) return;
            if (y < 0 || y >= Level.Y_LENGTH * Chunk.Y_LENGTH) return;
            if (z < 0 || z >= Level.Z_LENGTH * Chunk.Z_LENGTH) return;

            int chunkX = x / Chunk.X_LENGTH;
            int chunkY = y / Chunk.Y_LENGTH;
            int chunkZ = z / Chunk.Z_LENGTH;
            int tileX = x % Chunk.X_LENGTH;
            int tileY = y % Chunk.Y_LENGTH;
            int tileZ = z % Chunk.Z_LENGTH;

            Chunk chunk = chunks[chunkZ, chunkY, chunkX];
            if (chunk == null) return;

            chunk.SetTileID(layer, tileX, tileY, tileZ, id);

            if (tileX == 0 && chunkX > 0)
            {
                Chunk otherChunk = chunks[chunkZ, chunkY, chunkX - 1];
                if (otherChunk != null) otherChunk.IsDirty[layer] = true;
            }
            else if (tileX == Chunk.X_LENGTH - 1 && chunkX < Level.X_LENGTH - 1)
            {
                Chunk otherChunk = chunks[chunkZ, chunkY, chunkX + 1];
                if (otherChunk != null) otherChunk.IsDirty[layer] = true;
            }
            if (tileY == 0 && chunkY > 0)
            {
                Chunk otherChunk = chunks[chunkZ, chunkY - 1, chunkX];
                if (otherChunk != null) otherChunk.IsDirty[layer] = true;
            }
            else if (tileY == Chunk.Y_LENGTH - 1 && chunkY < Level.Y_LENGTH - 1)
            {
                Chunk otherChunk = chunks[chunkZ, chunkY + 1, chunkX];
                if (otherChunk != null) otherChunk.IsDirty[layer] = true;
            }
            if (tileZ == 0 && chunkZ > 0)
            {
                Chunk otherChunk = chunks[chunkZ - 1, chunkY, chunkX];
                if (otherChunk != null) otherChunk.IsDirty[layer] = true;
            }
            else if (tileZ == Chunk.Z_LENGTH - 1 && chunkZ < Level.Z_LENGTH - 1)
            {
                Chunk otherChunk = chunks[chunkZ + 1, chunkY, chunkX];
                if (otherChunk != null) otherChunk.IsDirty[layer] = true;
            }
        }

        public byte GetVisibleSides(int layer, int x, int y, int z)
        {
            byte minusZ = GetTileID(layer, x, y, z - 1);
            byte plusZ = GetTileID(layer, x, y, z + 1);
            byte minusY = GetTileID(layer, x, y - 1, z);
            byte plusY = GetTileID(layer, x, y + 1, z);
            byte minusX = GetTileID(layer, x - 1, y, z);
            byte plusX = GetTileID(layer, x + 1, y, z);

            byte sides = 0;

            if (minusZ == 0) sides |= 1 << 0;
            if (plusZ == 0) sides |= 1 << 1;
            if (minusY == 0) sides |= 1 << 2;
            if (plusY == 0) sides |= 1 << 3;
            if (minusX == 0) sides |= 1 << 4;
            if (plusX == 0) sides |= 1 << 5;

            return sides;
        }

        public List<AABB> GetAABBs(int layer, AABB aabb)
        {
            List<AABB> aabbs = new();

            Vector3i a = new((int)aabb.A.X, (int)aabb.A.Y, (int)aabb.A.Z);
            Vector3i b = new((int)(aabb.B.X + 1), (int)(aabb.B.Y + 1), (int)(aabb.B.Z + 1));

            if (a.X < 0) a.X = 0;
            if (a.Y < 0) a.Y = 0;
            if (a.Z < 0) a.Z = 0;
            if (b.X > Level.X_LENGTH * Chunk.X_LENGTH) b.X = Level.X_LENGTH * Chunk.X_LENGTH;
            if (b.Y > Level.Y_LENGTH * Chunk.Y_LENGTH) b.Y = Level.Y_LENGTH * Chunk.Y_LENGTH;
            if (b.Z > Level.Z_LENGTH * Chunk.Z_LENGTH) b.Z = Level.Z_LENGTH * Chunk.Z_LENGTH;

            for (int z = a.Z; z < b.Z; z++)
            {
                for (int y = a.Y; y < b.Y; y++)
                {
                    for (int x = a.X; x < b.X; x++)
                    {
                        byte tile = GetTileID(layer, x, y, z);
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
