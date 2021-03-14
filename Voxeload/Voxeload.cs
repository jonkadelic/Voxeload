using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Linq;
using Voxeload.Entities;
using Voxeload.Render;
using Voxeload.Shaders;
using Voxeload.Textures;
using Voxeload.World;
using ErrorCode = OpenTK.Graphics.OpenGL.ErrorCode;

namespace Voxeload
{
    public class Voxeload : GameWindow
    {
        public ShaderProgramManager ShaderProgramManager { get; } = new();
        public TextureManager TextureManager { get; } = new();
        protected LevelRenderer levelRenderer;
        protected Level level;

        public ShaderProgram ActiveShader { get; protected set; } = null;

        EntityModel selectionModel;

        protected Vector2i lastClientSize;
        protected MouseState lastMouseState;

        protected Matrix4 model;
        protected Matrix4 view;
        protected Matrix4 projection;

        public EntityPlayer player;

        protected (Vector3i pos, Tile.Face face)? lookPos = null;

        public Voxeload(GameWindowSettings gws, NativeWindowSettings nws) : base(gws, nws)
        {
            ClientRectangle = new(256, 256, 640 + 256, 480 + 256);
            level = new(new DefaultChunkGenerator());
            levelRenderer = new(this, level);
            player = new(this, level);
        }

        protected override void OnLoad()
        {
            GL.Enable(EnableCap.DepthTest);

            ShaderProgramManager.LoadProgram("tile", new("Shaders/tile.vert", ShaderType.VertexShader), new("Shaders/tile.frag", ShaderType.FragmentShader));
            ShaderProgramManager.LoadProgram("frame", new("Shaders/frame.vert", ShaderType.VertexShader), new("Shaders/frame.frag", ShaderType.FragmentShader));

            TextureManager.LoadTexture("terrain", "Textures/terrain.png");

            lastClientSize = new(0, 0);

            CursorGrabbed = true;

            lastMouseState = MouseState.GetSnapshot();

            Model model = new CubeTileModels().GetModel(0b00111111);
            Vector3[] vertices = new Vector3[model.Vertices.Length];
            model.Vertices.CopyTo(vertices, 0);
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] -= new Vector3(0.5f);
                vertices[i] *= 1.001f;
            }
            selectionModel = new(vertices);
            selectionModel.SetColours(Enumerable.Repeat<uint>(0x7FFFFFFF, 36).ToArray());

            renderCounter = RenderFrequency;

            base.OnLoad();
        }

        protected override void OnUnload()
        {
            base.OnUnload();
        }

        int breakCounter = 0, placeCounter = 0;
        double updateCounter = 0;
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            level.GenerateNextChunks();

            lookPos = RayCaster.CastIntoWorld(level, 0, player.Pos, player.XRotation, player.YRotation, 5.0f, 0.05f);

            if (MouseState.IsButtonDown(MouseButton.Button1) && breakCounter == 0)
            {
                if (lookPos.HasValue)
                {
                    (Vector3i point, Tile.Face _) = lookPos.Value;
                    byte id = level.GetTileID(0, point.X, point.Y, point.Z);
                    if (id != 0)
                    {
                        level.SetTileID(0, point.X, point.Y, point.Z, 0);
                    }
                }
                breakCounter = (int)UpdateFrequency / 4;

            }
            if (breakCounter > 0) breakCounter--;

            if (MouseState.IsButtonDown(MouseButton.Button2) && placeCounter == 0)
            {
                if (lookPos.HasValue)
                {
                    (Vector3i point, Tile.Face face) = lookPos.Value;
                    Vector3i placePoint = new(point.X, point.Y, point.Z);
                    switch (face)
                    {
                        case Tile.Face.North:
                            placePoint.Z--;
                            break;
                        case Tile.Face.South:
                            placePoint.Z++;
                            break;
                        case Tile.Face.Bottom:
                            placePoint.Y--;
                            break;
                        case Tile.Face.Top:
                            placePoint.Y++;
                            break;
                        case Tile.Face.West:
                            placePoint.X--;
                            break;
                        case Tile.Face.East:
                            placePoint.X++;
                            break;
                    }
                    byte id = level.GetTileID(0, placePoint.X, placePoint.Y, placePoint.Z);
                    if (id == 0 && !Tile.GetAABB(placePoint.X, placePoint.Y, placePoint.Z).Intersects(player.AABB))
                    {
                        level.SetTileID(0, placePoint.X, placePoint.Y, placePoint.Z, 2);
                    }
                }
                placeCounter = (int)UpdateFrequency / 4;
            }
            if (placeCounter > 0) placeCounter--;

            player.Rotate((MouseState.Y - lastMouseState.Y) * -0.5f, (MouseState.X - lastMouseState.X) * 0.5f);

            lastMouseState = MouseState.GetSnapshot();

            player.Tick();

            if (updateCounter < 0)
            {
                updateCounter = UpdateFrequency;
                Console.WriteLine($"{1 / args.Time:F4} FPS, {player.Pos}");
            }
            updateCounter--;

            base.OnUpdateFrame(args);
        }

        double renderCounter;
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            ErrorCode err = GL.GetError();
            if (err != ErrorCode.NoError) throw new Exception(err.ToString());

            GL.ClearColor(0.347f, 0.789f, 0.851f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (ClientSize != lastClientSize)
            {
                GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

                GL.BindTexture(TextureTarget.Texture2D, ChunkRenderer.watertexc);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, ClientSize.X, ClientSize.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                GL.BindTexture(TextureTarget.Texture2D, ChunkRenderer.watertexd);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, ClientSize.X, ClientSize.Y, 0, PixelFormat.DepthComponent, PixelType.UnsignedInt, IntPtr.Zero);
                GL.BindTexture(TextureTarget.Texture2D, 0);

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, ChunkRenderer.waterfbo);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, ChunkRenderer.watertexc, 0);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, ChunkRenderer.watertexd, 0);
                while (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete) ;
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }


            ActiveShader = ShaderProgramManager.GetProgram("tile");
            ActiveShader.Use();

            ActiveShader.SetInt("texture0", 0);

            TextureManager.GetTexture("terrain").Use();

            model = Matrix4.Identity;
            view = Matrix4.CreateTranslation(-player.Pos) * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(player.YRotation)) * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(player.XRotation));
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90), (float)ClientSize.X / ClientSize.Y, 0.1f, 200.0f);

            GL.UniformMatrix4(GL.GetUniformLocation(ActiveShader.Handle, "model"), false, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(ActiveShader.Handle, "view"), false, ref view);
            GL.UniformMatrix4(GL.GetUniformLocation(ActiveShader.Handle, "projection"), false, ref projection);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            levelRenderer.Render();

            if (lookPos != null)
            {
                Vector3 pos = lookPos.Value.pos;
                GL.Uniform4(GL.GetUniformLocation(ActiveShader.Handle, "offset"), (int)pos.X + 0.5f, (int)pos.Y + 0.5f, (int)pos.Z + 0.5f, 0.0f);

                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                selectionModel.Render();

                GL.Disable(EnableCap.Blend);
            }

            GL.Disable(EnableCap.CullFace);

            Context.SwapBuffers();

            base.OnRenderFrame(args);
        }
    }
}
