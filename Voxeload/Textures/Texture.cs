using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.Textures
{
    public class Texture
    {
        protected int handle;

        public int Length { get; }

        public Texture(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("Could not find texture path!", path);

            Image<Rgba32> image = Image.Load<Rgba32>(path);

            if (image.Width != image.Height) throw new Exception("Image was not square!");

            image.Mutate(x => x.Flip(FlipMode.Vertical));

            Length = image.Width;

            // Generate texture
            handle = GL.GenTexture();

            // Set texture data
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Length, Length, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.GetPixelMemoryGroup().ToArray());
        }

        public void Use()
        {
            GL.BindTexture(TextureTarget.Texture2D, handle);
        }
    }
}
