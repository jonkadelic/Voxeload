using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.Shaders
{
    public class Shader : IDisposable
    {
        public ShaderType ShaderType { get; }

        public int Handle { get; }

        protected int program = 0;
        public int Program
        {
            get => program;
            set
            {
                if (program == 0) program = value;
                else throw new Exception("Cannot set shader program multiple times!");
            }
        }

        public Shader(string path, ShaderType type)
        {
            ShaderType = type;

            if (!File.Exists(path)) throw new FileNotFoundException("Could not find shader file!", path);

            using StreamReader reader = new(path, Encoding.UTF8);
            string code = reader.ReadToEnd();

            Handle = GL.CreateShader(type);
            GL.ShaderSource(Handle, code);

            GL.CompileShader(Handle);

            string log = GL.GetShaderInfoLog(Handle);

            if (log != string.Empty) throw new Exception(log);
        }

        public void Dispose()
        {
            GL.DeleteShader(Handle);
            GC.SuppressFinalize(this);
        }
    }
}
