using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using OpenTK.Mathematics;

namespace Voxeload
{
    public abstract class ExchangeQueue<incoming, outgoing>
    {
        protected Queue<incoming> inQueue = new();
        protected Queue<outgoing> outQueue = new();
        protected bool stopping = false;
        protected object inQueueLock = new();
        protected object outQueueLock = new();

        public ExchangeQueue(ThreadPriority priority)
        {
            Thread t = new(() => Run());
            t.IsBackground = true;
            t.Priority = priority;
            t.Start();
        }

        protected void Run()
        {
            while (!stopping)
            {
                if (inQueue.Count > 0)
                {
                    incoming incoming;
                    lock (inQueueLock)
                    {
                        incoming = inQueue.Dequeue();
                    }
                    lock (outQueueLock)
                    {
                        outQueue.Enqueue(Process(incoming));
                    }
                }
            }
        }

        public bool Request(incoming incoming)
        {
            if (!inQueue.Contains(incoming))
            {
                lock (inQueueLock)
                {
                    inQueue.Enqueue(incoming);
                }
                return true;
            }
            else return false;
        }

        public outgoing Receive()
        {
            if (outQueue.Count > 0)
            {
                return outQueue.Dequeue();
            }
            return default;
        }

        protected abstract outgoing Process(incoming incoming);
    }
}
