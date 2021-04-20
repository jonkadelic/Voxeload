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

        private Voxeload voxeload;
        public readonly Chunk[,,] chunks = new Chunk[Z_LENGTH, Y_LENGTH, X_LENGTH];
        private ChunkGenExchangeQueue generator;
        private List<(int x, int y, int z)> chunksToGenerate = new();
        private List<(Vector3i pos, byte tile)> structureBuffer = new();
        private GameTickQueueThread gameTickThread;

        public Level(Voxeload voxeload, IChunkGenerator generator)
        {
            this.voxeload = voxeload;
            this.generator = new(ref structureBuffer, this, generator);

            gameTickThread = new(this);
        }

        public Chunk GetChunk(int x, int y, int z)
        {
            if (x < 0 || x >= X_LENGTH) return null;
            if (y < 0 || y >= Y_LENGTH) return null;
            if (z < 0 || z >= Z_LENGTH) return null;

            Chunk chunk = chunks[z, y, x];

            if (chunk == null && chunksToGenerate.Contains((x, y, z)) == false) chunksToGenerate.Add((x, y, z));

            return chunk;
        }

        public void GenerateNextChunks()
        {
            int counter = 0;
            while (chunksToGenerate.Count > 0 && counter < 1)
            {
                (int x, int y, int z) = chunksToGenerate.First();

                if (chunks[z, y, x] == null)
                {
                    generator.Request(new(x, y, z));
                    chunksToGenerate.RemoveAt(0);
                }

                counter++;
            }

            Chunk chunk;
            counter = 0;
            while (counter < 1 && (chunk = generator.Receive()) != null)
            {
                chunks[chunk.Z, chunk.Y, chunk.X] = chunk;
                counter++;

                for (int i = 0; i < structureBuffer.Count; i++)
                {
                    var item = structureBuffer[i];
                    Vector3i chunkPos = new(item.pos.X / Chunk.X_LENGTH, item.pos.Y / Chunk.Y_LENGTH, item.pos.Z / Chunk.Z_LENGTH);

                    if (chunkPos.X >= 0 && chunkPos.X < X_LENGTH && chunkPos.Y >= 0 && chunkPos.Y < Y_LENGTH && chunkPos.Z >= 0 && chunkPos.Z < Z_LENGTH)
                    {
                        Chunk structureChunk = chunks[chunkPos.Z, chunkPos.Y, chunkPos.X];
                        if (structureChunk != null)
                        {
                            Vector3i tilePos = new(item.pos.X % Chunk.X_LENGTH, item.pos.Y % Chunk.Y_LENGTH, item.pos.Z % Chunk.Z_LENGTH);
                            if (structureChunk.GetTileID(0, tilePos.X, tilePos.Y, tilePos.Z) == Tile.air.ID)
                            {
                                structureChunk.SetTileID(0, tilePos.X, tilePos.Y, tilePos.Z, item.tile);
                            }
                            structureBuffer.RemoveAt(i);
                            i--;
                        }
                    }
                    else
                    {
                        structureBuffer.RemoveAt(i);
                        i--;
                    }
                }
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

            // Set block
            chunk.SetTileID(layer, tileX, tileY, tileZ, id);

            // Dirty surrounding chunks
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

        public void SetTileIDNotify(int layer, Vector3i pos, Vector3i source, byte id)
        {
            if (pos.X != -1 && pos.Y != -1 && pos.Z != -1)
            {
                SetTileID(layer, pos.X, pos.Y, pos.Z, id);

                //Tile.tiles[id].OnUpdate(this, pos.X, pos.Y, pos.Z);
                QueueTick(new(pos.X, pos.Y, pos.Z), Tile.tiles[id]);
                for (int i = 0; i < Chunk.LAYER_COUNT; i++)
                {
                    if (source.X != pos.X - 1) QueueTick(new(pos.X - 1, pos.Y, pos.Z), Tile.tiles[GetTileID(i, pos.X - 1, pos.Y, pos.Z)]);
                    if (source.X != pos.X + 1) QueueTick(new(pos.X + 1, pos.Y, pos.Z), Tile.tiles[GetTileID(i, pos.X + 1, pos.Y, pos.Z)]);
                    if (source.Y != pos.Y - 1) QueueTick(new(pos.X, pos.Y - 1, pos.Z), Tile.tiles[GetTileID(i, pos.X, pos.Y - 1, pos.Z)]);
                    if (source.Y != pos.Y + 1) QueueTick(new(pos.X, pos.Y + 1, pos.Z), Tile.tiles[GetTileID(i, pos.X, pos.Y + 1, pos.Z)]);
                    if (source.Z != pos.Z - 1) QueueTick(new(pos.X, pos.Y, pos.Z - 1), Tile.tiles[GetTileID(i, pos.X, pos.Y, pos.Z - 1)]);
                    if (source.Z != pos.Z + 1) QueueTick(new(pos.X, pos.Y, pos.Z + 1), Tile.tiles[GetTileID(i, pos.X, pos.Y, pos.Z + 1)]);

                    //if (source.X != pos.X - 1) Tile.tiles[GetTileID(i, pos.X - 1, pos.Y, pos.Z)].OnUpdate(this, pos.X - 1, pos.Y, pos.Z);
                    //if (source.X != pos.X + 1) Tile.tiles[GetTileID(i, pos.X + 1, pos.Y, pos.Z)].OnUpdate(this, pos.X + 1, pos.Y, pos.Z);
                    //if (source.Y != pos.Y - 1) Tile.tiles[GetTileID(i, pos.X, pos.Y - 1, pos.Z)].OnUpdate(this, pos.X, pos.Y - 1, pos.Z);
                    //if (source.Y != pos.Y + 1) Tile.tiles[GetTileID(i, pos.X, pos.Y + 1, pos.Z)].OnUpdate(this, pos.X, pos.Y + 1, pos.Z);
                    //if (source.Z != pos.Z - 1) Tile.tiles[GetTileID(i, pos.X, pos.Y, pos.Z - 1)].OnUpdate(this, pos.X, pos.Y, pos.Z - 1);
                    //if (source.Z != pos.Z + 1) Tile.tiles[GetTileID(i, pos.X, pos.Y, pos.Z + 1)].OnUpdate(this, pos.X, pos.Y, pos.Z + 1);
                }

                Console.WriteLine($"Notifying block update at {pos} from {source}.");
            }
        }

        public void QueueTick(Vector3i pos, Tile tile)
        {
            gameTickThread.QueueTick(pos, tile);
        }

        public byte GetVisibleSides(int layer, int x, int y, int z)
        {
            if (Tile.tiles[GetTileID(layer, x, y, z)] is TileTransparent) return 0b00111111;

            byte minusZ = GetTileID(layer, x, y, z - 1);
            byte plusZ = GetTileID(layer, x, y, z + 1);
            byte minusY = GetTileID(layer, x, y - 1, z);
            byte plusY = GetTileID(layer, x, y + 1, z);
            byte minusX = GetTileID(layer, x - 1, y, z);
            byte plusX = GetTileID(layer, x + 1, y, z);

            byte sides = 0;

            if (minusZ == 0 || Tile.tiles[minusZ] is TileTransparent) sides |= 1 << 0;
            if (plusZ == 0 || Tile.tiles[plusZ] is TileTransparent) sides |= 1 << 1;
            if (minusY == 0 || Tile.tiles[minusY] is TileTransparent) sides |= 1 << 2;
            if (plusY == 0 || Tile.tiles[plusY] is TileTransparent) sides |= 1 << 3;
            if (minusX == 0 || Tile.tiles[minusX] is TileTransparent) sides |= 1 << 4;
            if (plusX == 0 || Tile.tiles[plusX] is TileTransparent) sides |= 1 << 5;

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
                        if (tile == 0 || Tile.tiles[tile].Collides == false) continue;

                        AABB c = new(new(x, y, z), new(x + 1, y + 1, z + 1));

                        aabbs.Add(c);
                    }
                }
            }

            return aabbs;
        }

        public void TickChunks(int counter)
        {
            if (voxeload.player == null) return;
            for (int z = 0; z < Z_LENGTH; z++)
            {
                for (int y = 0; y < Y_LENGTH; y++)
                {
                    for (int x = 0; x < X_LENGTH; x++)
                    {
                        if (z == (int)(voxeload.player.Pos.Z / Chunk.Z_LENGTH) && y == (int)(voxeload.player.Pos.Y / Chunk.Y_LENGTH) && x == (int)(voxeload.player.Pos.X / Chunk.X_LENGTH))
                        {
                            Console.WriteLine("Ticked player chunk!");
                        }
                        if (Vector3.DistanceSquared(voxeload.player.Pos, new(x * Chunk.X_LENGTH, y * Chunk.Y_LENGTH, z * Chunk.Z_LENGTH)) < 16384) chunks[z, y, x]?.TickTiles(counter);
                    }
                }
            }
        }
    }
}
