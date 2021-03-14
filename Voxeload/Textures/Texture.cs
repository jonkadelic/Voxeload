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

            byte[] data = new byte[4 * Length * Length];

            for (int y = 0; y < Length; y++)
            {
                var row = image.GetPixelRowSpan(y);
                for (int x = 0; x < Length; x++)
                {
                    data[y * Length * 4 + x * 4 + 0] = row[x].R;
                    data[y * Length * 4 + x * 4 + 1] = row[x].G;
                    data[y * Length * 4 + x * 4 + 2] = row[x].B;
                    data[y * Length * 4 + x * 4 + 3] = row[x].A;
                }
            }

            // Bind
            GL.BindTexture(TextureTarget.Texture2D, handle);

            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, new[] { (int)TextureWrapMode.Repeat });
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, new[] { (int)TextureWrapMode.Repeat });
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new[] { (int)TextureMinFilter.Nearest });
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new[] { (int)TextureMagFilter.Nearest });


            // Set texture data
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Length, Length, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
        }

        public void Use(TextureUnit unit = TextureUnit.Texture0)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, handle);
        }
    }
}
