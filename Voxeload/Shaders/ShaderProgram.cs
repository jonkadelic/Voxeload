using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.Shaders
{
    public class ShaderProgram : IDisposable
    {
        public int Handle { get; }

        private bool disposed = false;

        public ShaderProgram(List<Shader> shaders) : this(shaders.ToArray())
        {
        }

        public ShaderProgram(params Shader[] shaders)
        {
            Handle = GL.CreateProgram();

            foreach (Shader shader in shaders)
            {
                GL.AttachShader(Handle, shader.Handle);
                shader.Program = Handle;
            }

            GL.LinkProgram(Handle);
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                GL.DeleteProgram(Handle);
                disposed = true;
            }
        }

        ~ShaderProgram()
        {
            GL.DeleteProgram(Handle);
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
