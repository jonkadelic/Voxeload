using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxeload.World;

namespace Voxeload.Entities
{
    public class EntityPlayer : Entity
    {
        protected static Tile[] placeableTiles = { Tile.stone, Tile.dirt, Tile.planks, Tile.sand, Tile.gravel, Tile.glass, Tile.log, Tile.leaves, Tile.roseFlower, Tile.water };
        protected int heldTile = 0;

        public int HeldTileIndex
        {
            get => heldTile;
            set
            {
                heldTile = (value % placeableTiles.Length + placeableTiles.Length) % placeableTiles.Length;
            }
        }

        public Tile HeldTile => placeableTiles[heldTile];

        public EntityPlayer(Voxeload voxeload, Level level) : base(voxeload, level)
        {
            SetPos(new(16, Level.Y_LENGTH * Chunk.Y_LENGTH, 16));
            YRotation = 135;
        }

        public override void Tick()
        {
            float xa = 0.0f, ya = 0.0f;
            if (voxeload.KeyboardState.IsKeyDown(Keys.W))
            {
                ya = -1.0f;
            }
            if (voxeload.KeyboardState.IsKeyDown(Keys.S))
            {
                ya = 1.0f;
            }
            if (voxeload.KeyboardState.IsKeyDown(Keys.A))
            {
                xa = -1.0f;
            }
            if (voxeload.KeyboardState.IsKeyDown(Keys.D))
            {
                xa = 1.0f;
            }
            if (voxeload.KeyboardState.IsKeyDown(Keys.Space) && (OnGround || InWater))
            {
                PosDelta.Y = 0.2f;

                if (InWater) PosDelta.Y = 0.1f;
            }

            MoveRelative(xa, ya, OnGround ? 0.05f : 0.01f);
            PosDelta.Y = (float)(PosDelta.Y - 0.01);

            if (InWater && PosDelta.Y < -0.05f) PosDelta.Y = -0.05f;

            Move(PosDelta);

            PosDelta.X *= 0.91f;
            PosDelta.Y *= 0.98f;
            PosDelta.Z *= 0.91f;

            if (OnGround)
            {
                PosDelta.X *= 0.7f;
                PosDelta.Z *= 0.7f;
            }
        }
    }
}
