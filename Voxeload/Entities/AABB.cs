using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.Entities
{
    public class AABB
    {
        private float epsilon = 0.0f;
        public Vector3 A { get; protected set; }
        public Vector3 B { get; protected set; }

        public AABB(Vector3 a, Vector3 b)
        {
            A = a;
            B = b;
        }

        public AABB Expand(Vector3 size)
        {
            Vector3 a = new(A);
            Vector3 b = new(B);

            if (size.X < 0.0f) a.X += size.X;
            if (size.X > 0.0f) b.X += size.X;
            if (size.Y < 0.0f) a.Y += size.Y;
            if (size.Y > 0.0f) b.Y += size.Y;
            if (size.Z < 0.0f) a.Z += size.Z;
            if (size.Z > 0.0f) b.Z += size.Z;

            return new AABB(a, b);
        }

        public AABB Grow(Vector3 size)
        {
            Vector3 a = A - size;
            Vector3 b = B + size;
            return new AABB(a, b);
        }

        public float ClipXCollide(AABB c, float xa)
        {
            float max;
            if (c.B.Y <= this.A.Y || c.A.Y >= this.B.Y) return xa;
            if (c.B.Z <= this.A.Z || c.A.Z >= this.B.Z) return xa;

            if (xa > 0.0f && c.B.X <= this.A.X && (max = this.A.X - c.B.X - this.epsilon) < xa) xa = max;
            if (xa < 0.0f && c.A.X >= this.B.X && (max = this.B.X - c.A.X + this.epsilon) > xa) xa = max;

            return xa;
        }

        public float ClipYCollide(AABB c, float ya)
        {
            float max;
            if (c.B.X <= this.A.X || c.A.X >= this.B.X) return ya;
            if (c.B.Z <= this.A.Z || c.A.Z >= this.B.Z) return ya;

            if (ya > 0.0f && c.B.Y <= this.A.Y && (max = this.A.Y - c.B.Y - this.epsilon) < ya) ya = max;
            if (ya < 0.0f && c.A.Y >= this.B.Y && (max = this.B.Y - c.A.Y + this.epsilon) > ya) ya = max;

            return ya;
        }

        public float ClipZCollide(AABB c, float za)
        {
            float max;
            if (c.B.X <= this.A.X || c.A.X >= this.B.X) return za;
            if (c.B.Y <= this.A.Y || c.A.Y >= this.B.Y) return za;

            if (za > 0.0f && c.B.Z <= this.A.Z && (max = this.A.Z - c.B.Z - this.epsilon) < za) za = max;
            if (za < 0.0f && c.A.Z >= this.B.Z && (max = this.B.Z - c.A.Z + this.epsilon) > za) za = max;

            return za;
        }

        public bool Intersects(AABB c)
        {
            if (c.B.X <= this.A.X || c.A.X >= this.B.X) return false;
            if (c.B.Y <= this.A.Y || c.A.Y >= this.B.Y) return false;

            return !(c.B.Z <= this.A.Z) && !(c.A.Z >= this.B.Z);
        }

        public void Move(Vector3 a)
        {
            A += a;
            B += a;
        }
    }
}
