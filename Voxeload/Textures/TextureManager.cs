using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.Textures
{
    public class TextureManager
    {
        protected Dictionary<string, Texture> textures = new();

        public void LoadTexture(string name, string path)
        {
            if (textures.ContainsKey(name)) throw new ArgumentException($"Texture with name {name} is already loaded!");

            Texture tex = new(path);

            textures[name] = tex;
        }

        public Texture GetTexture(string name)
        {
            return textures[name];
        }
    }
}
