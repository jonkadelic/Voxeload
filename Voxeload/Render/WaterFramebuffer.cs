using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;

using Voxeload.Shaders;

namespace Voxeload.Render
{
    public class WaterFramebuffer : Framebuffer
    {
        private int worldTextureDepth;

        public WaterFramebuffer(Voxeload voxeload, int width, int height, int worldTextureDepth) : base(voxeload, width, height)
        {
            this.worldTextureDepth = worldTextureDepth;
        }

        public override void Render()
        {
            // Draw framebuffer to screen
            ShaderProgram shader = voxeload.ShaderProgramManager.GetProgram("water_frame");
            shader.Use();
            GL.BindVertexArray(screenvao);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureColour);
            shader.SetInt("screenTexture", 0);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, textureDepth);
            shader.SetInt("screenDepthTexture", 1);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, worldTextureDepth);
            shader.SetInt("worldDepthTexture", 2);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
            voxeload.ActiveShader.Use();
        }
    }
}
