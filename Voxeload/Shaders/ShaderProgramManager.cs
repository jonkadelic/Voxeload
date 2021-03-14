using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.Shaders
{
    public class ShaderProgramManager
    {
        protected Dictionary<string, ShaderProgram> programs = new();

        public void LoadProgram(string name, params Shader[] shaders)
        {
            if (programs.ContainsKey(name)) throw new ArgumentException($"Shader with name {name} is already loaded!");

            ShaderProgram sp = new(shaders);

            programs[name] = sp;
        }

        public ShaderProgram GetProgram(string name)
        {
            return programs[name];
        }


        ~ShaderProgramManager()
        {
            foreach (ShaderProgram program in programs.Values)
            {
                program.Dispose();
            }
        }
    }
}
