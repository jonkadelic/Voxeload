using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.Render
{
    public class FramebufferManager
    {
        protected Dictionary<string, Framebuffer> framebuffers = new();
        private Voxeload voxeload;

        public FramebufferManager(Voxeload voxeload)
        {
            this.voxeload = voxeload;
        }

        public void CreateFramebuffer(string name, Framebuffer framebuffer = null)
        {
            if (framebuffers.ContainsKey(name)) throw new ArgumentException($"Framebuffer with name {name} already exists!");

            if (framebuffer == null)
            {
                framebuffer = new(voxeload, voxeload.ClientSize.X, voxeload.ClientSize.Y);
            }

            framebuffers[name] = framebuffer;
        }

        public Framebuffer GetFramebuffer(string name)
        {
            if (framebuffers.TryGetValue(name, out Framebuffer framebuffer))
            {
                return framebuffer;
            }

            return null;
        }

        public void ResizeFramebuffers(int width, int height)
        {
            foreach (Framebuffer framebuffer in framebuffers.Values)
            {
                framebuffer.Resize(width, height);
            }
        }
    }
}
