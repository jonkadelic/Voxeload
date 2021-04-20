using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using OpenTK.Mathematics;

namespace Voxeload.World
{
    public class GameTickQueueThread
    {
        protected struct Tick
        {
            public Vector3i pos;
            public Tile tile;
            public int remaining;

            public Tick(Vector3i pos, Tile tile)
            {
                this.pos = pos;
                this.tile = tile;
                remaining = tile.TickInterval;
            }
        }

        protected Thread t;
        protected Level level;
        protected int counter;
        protected bool running = true;
        protected Queue<Tick> tickQueue = new();
        protected object queueLock = new();
        protected long lastTickTimeMs;

        public GameTickQueueThread(Level level)
        {
            this.level = level;

            lastTickTimeMs = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            t = new(() => Run());
            t.IsBackground = true;
            t.Priority = ThreadPriority.Normal;
            t.Start();
        }

        public void QueueTick(Vector3i pos, Tile tile)
        {
            lock (queueLock)
            {
                tickQueue.Enqueue(new(pos, tile));
            }
        }

        protected void Run()
        {
            while (running)
            {
                if (DateTimeOffset.Now.ToUnixTimeMilliseconds() > lastTickTimeMs + 50)
                {
                    for (int i = 0; i < tickQueue.Count; i++)
                    {
                        lastTickTimeMs = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                        Tick t;
                        lock (queueLock)
                        {
                            t = tickQueue.Dequeue();
                        }
                        if (t.remaining > 0)
                        {
                            t.remaining--;
                            lock(queueLock)
                            {
                                tickQueue.Enqueue(t);
                            }
                        }
                        else
                        {
                            t.tile.OnTick(level, t.pos.X, t.pos.Y, t.pos.Z);
                        }
                    }
                }
            }
        }
    }
}
