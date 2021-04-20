using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.Render
{
    public class Framebuffer
    {
        protected Voxeload voxeload;

        protected int fbo;
        protected int textureColour;
        public int textureDepth;

        protected static int screenvbo;
        protected static int screenvao;
        protected static int screenuvbo;

        public static Framebuffer ActiveFramebuffer { get; private set; }

        public Framebuffer(Voxeload voxeload, int width, int height)
        {
            this.voxeload = voxeload;

            // Generate framebuffer
            fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);

            // Generate framebuffer colour texture
            textureColour = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureColour);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            // Generate framebuffer depth texture
            textureDepth = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureDepth);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            // Attach textures to framebuffer
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, textureColour, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, textureDepth, 0);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // Generate screen tris
            if (screenvao == 0)
            {
                screenvbo = GL.GenBuffer();
                screenvao = GL.GenVertexArray();
                screenuvbo = GL.GenBuffer();

                GL.BindVertexArray(screenvao);

                GL.BindBuffer(BufferTarget.ArrayBuffer, screenvbo);
                GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 2 * 6, new Vector2[] { new(-1, -1), new(1, -1), new(-1, 1), new(1, 1), new(-1, 1), new(1, -1) }, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2, 0);
                GL.EnableVertexAttribArray(0);

                GL.BindBuffer(BufferTarget.ArrayBuffer, screenuvbo);
                GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 2 * 6, new Vector2[] { new(0, 0), new(1, 0), new(0, 1), new(1, 1), new(0, 1), new(1, 0) }, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2, 0);
                GL.EnableVertexAttribArray(1);

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindVertexArray(0);
            }
        }

        ~Framebuffer()
        {
            GL.DeleteBuffer(screenvbo);
            GL.DeleteVertexArray(screenvao);
            GL.DeleteBuffer(screenuvbo);

            GL.DeleteFramebuffer(fbo);
            GL.DeleteTexture(textureColour);
            GL.DeleteTexture(textureDepth);
        }

        public void Resize(int width, int height)
        {
            GL.BindTexture(TextureTarget.Texture2D, textureColour);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

            GL.BindTexture(TextureTarget.Texture2D, textureDepth);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        }

        public void Use()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);

            ActiveFramebuffer = this;
        }

        public static void Disuse()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            ActiveFramebuffer = null;
        }

        public virtual void Render()
        {
            // Draw framebuffer to screen
            voxeload.ShaderProgramManager.GetProgram("frame").Use();
            GL.BindVertexArray(screenvao);
            GL.BindTexture(TextureTarget.Texture2D, textureColour);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
            voxeload.ActiveShader.Use();
        }

        public void Clear()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
    }
}
