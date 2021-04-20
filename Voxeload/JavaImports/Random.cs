using System;

namespace Voxeload.JavaImports
{
    public class Random
    {
        private long seed;
        private double nextNextGaussian;
        private bool haveNextNextGaussian = false;

        public Random()
        {
            seed = DateTime.Now.Ticks;
        }

        public Random(long seed)
        {
            SetSeed(seed);
        }

        public void SetSeed(long seed)
        {
            this.seed = (seed ^ 0x5DEECE66DL) & ((1L << 48) - 1);
            haveNextNextGaussian = false;
        }

        protected int Next(int bits)
        {
            seed = (seed * 0x5DEECE66DL + 0xBL) & ((1L << 48) - 1);

            return (int)((ulong)seed >> (48 - bits));
        }

        public void NextBytes(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length;)
            {
                for (int rnd = NextInt(), n = Math.Min(bytes.Length - i, 4); n-- > 0; rnd >>= 8)
                {
                    bytes[i++] = (byte)rnd;
                }
            }
        }

        public int NextInt()
        {
            return Next(32);
        }

        public int NextInt(int n)
        {
            if (n <= 0) throw new ArgumentException("n must be positive");

            if ((n & -n) == n) return (int)((n * (long)Next(31)) >> 31);

            int bits, val;

            do
            {
                bits = Next(31);
                val = bits % n;
            } while (bits - val + (n - 1) < 0);

            return val;
        }

        public long NextLong()
        {
            return ((long)Next(32) << 32) + Next(32);
        }

        public bool NextBoolean()
        {
            return Next(1) != 0;
        }

        public float NextFloat()
        {
            return Next(24) / ((float)(1 << 24));
        }

        public double NextDouble()
        {
            return (((long)Next(26) << 27) + Next(27)) / (double)(1L << 53);
        }

        public double NextGaussian()
        {
            if (haveNextNextGaussian)
            {
                haveNextNextGaussian = false;
                return nextNextGaussian;
            }
            else
            {
                double v1, v2, s;
                do
                {
                    v1 = 2 * NextDouble() - 1;
                    v2 = 2 * NextDouble() - 1;
                    s = v1 * v1 + v2 * v2;
                } while (s >= 1 || s == 0);
                double multiplier = Math.Sqrt(-2 * Math.Log(s) / s);
                nextNextGaussian = v2 * multiplier;
                haveNextNextGaussian = true;
                return v1 * multiplier;
            }
        }
    }
}
