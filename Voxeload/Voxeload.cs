using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Linq;
using System.Runtime.InteropServices;

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
        public FramebufferManager FramebufferManager { get; private set; }
        protected LevelRenderer levelRenderer;
        protected Level level;

        protected DebugProc debugProc = DebugProc;

        public ShaderProgram ActiveShader { get; protected set; } = null;

        EntityModel selectionModel;

        protected Vector2i lastClientSize;
        protected MouseState lastMouseState;

        protected Matrix4 model;
        protected Matrix4 view;
        protected Matrix4 projection;

        public EntityPlayer player;

        protected (Vector3i pos, Tile.Face face)? lookPos = null;

        protected FirstPersonHeldRenderer firstPersonHeldRenderer;

        protected double frameAvg = 0;
        protected double frameAvgCount = 0;

        public Voxeload(GameWindowSettings gws, NativeWindowSettings nws) : base(gws, nws)
        {
            ClientRectangle = new(256, 256, 1920 + 256, 1080 + 256);
            level = new(this, new DefaultChunkGenerator());
            levelRenderer = new(this, level);
            player = new(this, level);
        }

        protected override void OnLoad()
        {
            GL.Enable(EnableCap.DepthTest);

            ShaderProgramManager.LoadProgram("tile", new("Shaders/tile.vert", ShaderType.VertexShader), new("Shaders/tile.frag", ShaderType.FragmentShader));
            ShaderProgramManager.LoadProgram("frame", new("Shaders/frame.vert", ShaderType.VertexShader), new("Shaders/frame.frag", ShaderType.FragmentShader));
            ShaderProgramManager.LoadProgram("selection", new("Shaders/selection.vert", ShaderType.VertexShader), new("Shaders/selection.frag", ShaderType.FragmentShader));
            ShaderProgramManager.LoadProgram("water_frame", new("Shaders/water_frame.vert", ShaderType.VertexShader), new("Shaders/water_frame.frag", ShaderType.FragmentShader));
            ShaderProgramManager.LoadProgram("water_tile", new("Shaders/water_tile.vert", ShaderType.VertexShader), new("Shaders/water_tile.frag", ShaderType.FragmentShader));

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

            FramebufferManager = new(this);

            FramebufferManager.CreateFramebuffer("tiles");
            FramebufferManager.CreateFramebuffer("water", new WaterFramebuffer(this, ClientSize.X, ClientSize.Y, FramebufferManager.GetFramebuffer("tiles").textureDepth));
            FramebufferManager.CreateFramebuffer("overlay");

            firstPersonHeldRenderer = new(this);

            //GL.DebugMessageCallback(debugProc, (IntPtr)0);

            base.OnLoad();
        }

        public static void DebugProc(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            Console.WriteLine($"{source} {type} {id} {severity} {Marshal.PtrToStringAnsi(message)}");
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
                        level.SetTileIDNotify(0, point, point, 0);
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
                        level.SetTileIDNotify(0, placePoint, placePoint, player.HeldTile.ID);
                    }
                }
                placeCounter = (int)UpdateFrequency / 4;
            }
            if (placeCounter > 0) placeCounter--;

            player.Rotate((MouseState.Y - lastMouseState.Y) * -0.5f, (MouseState.X - lastMouseState.X) * 0.5f);

            if (lastMouseState.Scroll.Y > MouseState.Scroll.Y)
            {
                player.HeldTileIndex--;
            }
            else if (lastMouseState.Scroll.Y < MouseState.Scroll.Y)
            {
                player.HeldTileIndex++;
            }

            lastMouseState = MouseState.GetSnapshot();

            player.Tick();

            base.OnUpdateFrame(args);

            if (renderCounter < 0)
            {
                renderCounter = UpdateFrequency;
                frameAvg = 0;
                frameAvgCount = 0;
            }
            renderCounter--;
            frameAvg += (1 / args.Time);
            frameAvgCount++;
        }

        double renderCounter;
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            ErrorCode err = GL.GetError();
            if (err != ErrorCode.NoError) throw new Exception(err.ToString());

            GL.ClearColor(127 / 255.0f, 167 / 255.0f, 255 / 255.0f, 255 / 255.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (ClientSize != lastClientSize)
            {
                GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

                FramebufferManager.ResizeFramebuffers(ClientSize.X, ClientSize.Y);
            }


            ActiveShader = ShaderProgramManager.GetProgram("tile");

            TextureManager.GetTexture("terrain").Use();

            if (player.HeldTile != firstPersonHeldRenderer.Tile) firstPersonHeldRenderer.SetTile(player.HeldTile);

            model = Matrix4.Identity;
            view = Matrix4.CreateTranslation(-player.Pos) * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(player.YRotation)) * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(player.XRotation));
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90), (float)ClientSize.X / ClientSize.Y, 0.1f, 256.0f);

            ShaderProgram waterTile = ShaderProgramManager.GetProgram("water_tile");
            waterTile.Use();
            waterTile.SetInt("texture0", 0);
            GL.UniformMatrix4(GL.GetUniformLocation(waterTile.Handle, "model"), false, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(waterTile.Handle, "view"), false, ref view);
            GL.UniformMatrix4(GL.GetUniformLocation(waterTile.Handle, "projection"), false, ref projection);

            ActiveShader.Use();
            ActiveShader.SetInt("texture0", 0);
            GL.UniformMatrix4(GL.GetUniformLocation(ActiveShader.Handle, "model"), false, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(ActiveShader.Handle, "view"), false, ref view);
            GL.UniformMatrix4(GL.GetUniformLocation(ActiveShader.Handle, "projection"), false, ref projection);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            levelRenderer.Render();

            if (lookPos != null)
            {
                FramebufferManager.GetFramebuffer("tiles").Use();
                ShaderProgram selection = ShaderProgramManager.GetProgram("selection");
                selection.Use();
                GL.UniformMatrix4(GL.GetUniformLocation(selection.Handle, "model"), false, ref model);
                GL.UniformMatrix4(GL.GetUniformLocation(selection.Handle, "view"), false, ref view);
                GL.UniformMatrix4(GL.GetUniformLocation(selection.Handle, "projection"), false, ref projection);
                Vector3 pos = lookPos.Value.pos;
                GL.Uniform4(GL.GetUniformLocation(selection.Handle, "offset"), (int)pos.X + 0.5f, (int)pos.Y + 0.5f, (int)pos.Z + 0.5f, 0.0f);

                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                selectionModel.Render();

                GL.Disable(EnableCap.Blend);
                Framebuffer.Disuse();
                ActiveShader.Use();
            }


            FramebufferManager.GetFramebuffer("overlay").Use();
            firstPersonHeldRenderer.Render();
            Framebuffer.Disuse();

            FramebufferManager.GetFramebuffer("tiles").Render();
            FramebufferManager.GetFramebuffer("water").Render();
            FramebufferManager.GetFramebuffer("overlay").Render();

            FramebufferManager.GetFramebuffer("tiles").Clear();
            FramebufferManager.GetFramebuffer("water").Clear();
            FramebufferManager.GetFramebuffer("overlay").Clear();

            GL.Disable(EnableCap.CullFace);

            Context.SwapBuffers();

            base.OnRenderFrame(args);

            if (renderCounter < 0)
            {
                Console.WriteLine($"{frameAvg / frameAvgCount:F4} FPS, {player.Pos}");
            }
        }
    }
}
