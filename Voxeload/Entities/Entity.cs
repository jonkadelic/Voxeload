using OpenTK.Mathematics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxeload.World;

namespace Voxeload.Entities
{
    public abstract class Entity
    {
        public Vector3 Pos = new(10, 10, 10);
        public Vector3 PosDelta = new();
        public AABB AABB;
        public float XRotation = 0;
        public float YRotation = 0;
        public bool OnGround = false;
        protected float aabbWidth = 0.6f;
        protected float aabbHeight = 1.8f;
        public float eyeOffset = 1.6f;

        protected Level level;
        protected Voxeload voxeload;

        public Entity(Voxeload voxeload, Level level)
        {
            this.level = level;
            this.voxeload = voxeload;
        }

        public abstract void Tick();

        public virtual void Move(Vector3 a)
        {
            Vector3 o = new(a);

            List<AABB> aabbs = level.GetAABBs(AABB.Expand(a));

            aabbs.Add(new(new(0, 0, 0), new(0, Chunk.Y_LENGTH, Level.Z_LENGTH * Chunk.Z_LENGTH)));
            aabbs.Add(new(new(0, 0, 0), new(Level.X_LENGTH * Chunk.X_LENGTH, Chunk.Y_LENGTH, 0)));
            aabbs.Add(new(new(Level.X_LENGTH * Chunk.X_LENGTH, 0, 0), new(Level.X_LENGTH * Chunk.X_LENGTH, Chunk.Y_LENGTH, Level.Z_LENGTH * Chunk.Z_LENGTH)));
            aabbs.Add(new(new(0, 0, Level.Z_LENGTH * Chunk.Z_LENGTH), new(Level.X_LENGTH * Chunk.X_LENGTH, Chunk.Y_LENGTH, Level.Z_LENGTH * Chunk.Z_LENGTH)));
            aabbs.Add(new(new(0, Chunk.Y_LENGTH, 0), new(Level.X_LENGTH * Chunk.X_LENGTH, Chunk.Y_LENGTH, Level.Z_LENGTH * Chunk.Z_LENGTH)));
            aabbs.Add(new(new(0, 0, 0), new(Level.X_LENGTH * Chunk.X_LENGTH, 0, Level.Z_LENGTH * Chunk.Z_LENGTH)));

            foreach (AABB aabb in aabbs)
            {
                a.Y = aabb.ClipYCollide(AABB, a.Y);
            }
            AABB.Move(new(0, a.Y, 0));
            foreach (AABB aabb in aabbs)
            {
                a.X = aabb.ClipXCollide(AABB, a.X);
            }
            AABB.Move(new(a.X, 0, 0));
            foreach (AABB aabb in aabbs)
            {
                a.Z = aabb.ClipZCollide(AABB, a.Z);
            }
            AABB.Move(new(0, 0, a.Z));

            OnGround = o.Y != a.Y && o.Y < 0;

            if (o.X != a.X) PosDelta.X = 0;
            if (o.Y != a.Y) PosDelta.Y = 0;
            if (o.Z != a.Z) PosDelta.Z = 0;

            Pos.X = (AABB.A.X + AABB.B.X) / 2.0f;
            Pos.Y = AABB.A.Y + eyeOffset;
            Pos.Z = (AABB.A.Z + AABB.B.Z) / 2.0f;
        }

        public virtual void MoveRelative(float dx, float dz, float speed)
        {
            float dist = dx * dx + dz * dz;
            if (dist < 0.01f) return;

            dist = speed / (float)Math.Sqrt(dist);
            float sin = (float)Math.Sin(MathHelper.DegreesToRadians(YRotation));
            float cos = (float)Math.Cos(MathHelper.DegreesToRadians(YRotation));

            PosDelta.X += (dx *= dist) * cos - (dz *= dist) * sin;
            PosDelta.Z += dz * cos + dx * sin;
        }

        public virtual void Jump(float dy)
        {
            if (OnGround) PosDelta.Y = dy;
        }

        public virtual void SetPos(Vector3 pos)
        {
            Pos = pos;
            float w = aabbWidth / 2.0f;
            float h = aabbHeight / 2.0f;
            AABB = new AABB(new(pos.X - w, pos.Y - h, pos.Z - w), new(pos.X + w, pos.Y + h, pos.Z + w));
        }

        public virtual void Rotate(float rotX, float rotY)
        {
            XRotation = (float)(XRotation - rotX * 0.15);
            YRotation = (float)((YRotation + rotY * 0.15) % 360.0);
            if (XRotation < -90.0f) XRotation = -90.0f;
            if (XRotation > 90.0f) XRotation = 90.0f;
        }
    }
}
