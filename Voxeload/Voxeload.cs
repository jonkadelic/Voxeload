using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using Voxeload.Entities;
using Voxeload.Render;
using Voxeload.Shaders;
using Voxeload.World;
using ErrorCode = OpenTK.Graphics.OpenGL.ErrorCode;

namespace Voxeload
{
    public class Voxeload : GameWindow
    {
        protected ShaderProgramManager shaderProgramManager = new();
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
            level = new(new FlatChunkGenerator());
            levelRenderer = new(this, level);
            player = new(this, level);
        }

        protected override void OnLoad()
        {
            GL.ClearColor(1.0F, 1.0F, 1.0F, 1.0F);
            GL.Enable(EnableCap.DepthTest);

            shaderProgramManager.LoadProgram("tile", new("Shaders/tile.vert", ShaderType.VertexShader), new("Shaders/tile.frag", ShaderType.FragmentShader));
            shaderProgramManager.LoadProgram("selection", new("Shaders/selection.vert", ShaderType.VertexShader), new("Shaders/selection.frag", ShaderType.FragmentShader));

            lastClientSize = ClientSize;

            CursorGrabbed = true;

            lastMouseState = MouseState.GetSnapshot();

            Model model = new CubeTileModel().GetModel(0b00111111);
            Vector3[] vertices = new Vector3[model.Vertices.Length];
            model.Vertices.CopyTo(vertices, 0);
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] -= new Vector3(0.5f);
                vertices[i] *= 1.001f;
            }
            selectionModel = new(vertices, model.Indices);
            selectionModel.SetColours(new uint[] { 0x7FFFFFFF, 0x7FFFFFFF, 0x7FFFFFFF, 0x7FFFFFFF, 0x7FFFFFFF, 0x7FFFFFFF, 0x7FFFFFFF, 0x7FFFFFFF });

            base.OnLoad();
        }

        protected override void OnUnload()
        {
            base.OnUnload();
        }

        bool wasGPressed = false;
        bool wasXPressed = false;
        int breakCounter = 0, placeCounter = 0;
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            level.GenerateNextChunks();

            if (KeyboardState.IsKeyDown(Keys.G))
            {
                if (!wasGPressed)
                {
                    foreach (Chunk chunk in level.chunks)
                    {
                        chunk.IsDirty = true;
                    }
                    wasGPressed = true;
                }
            }
            else wasGPressed = false;
            if (KeyboardState.IsKeyDown(Keys.X))
            {
                if (!wasXPressed)
                {
                    for (int z = (int)player.Pos.Z - 4; z < (int)player.Pos.Z + 4; z++)
                    {
                        for (int y = (int)player.Pos.Y - 4; y < (int)player.Pos.Y + 4; y++)
                        {
                            for (int x = (int)player.Pos.X - 4; x < (int)player.Pos.X + 4; x++)
                            {
                                level.SetTileID(x, y, z, 0);
                            }
                        }
                    }
                    wasXPressed = true;
                }
            }
            else wasXPressed = false;

            lookPos = RayCaster.CastIntoWorld(level, player.Pos, player.XRotation, player.YRotation, 5.0f, 0.05f);

            if (MouseState.IsButtonDown(MouseButton.Button1) && breakCounter == 0)
            {
                if (lookPos.HasValue)
                {
                    (Vector3i point, Tile.Face _) = lookPos.Value;
                    byte id = level.GetTileID(point.X, point.Y, point.Z);
                    if (id != 0)
                    {
                        level.SetTileID(point.X, point.Y, point.Z, 0);
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
                    byte id = level.GetTileID(placePoint.X, placePoint.Y, placePoint.Z);
                    if (id == 0)
                    {
                        level.SetTileID(placePoint.X, placePoint.Y, placePoint.Z, 2);
                    }
                }
                placeCounter = (int)UpdateFrequency / 4;
            }
            if (placeCounter > 0) placeCounter--;

            player.Rotate((MouseState.Y - lastMouseState.Y) * -0.5f, (MouseState.X - lastMouseState.X) * 0.5f);

            lastMouseState = MouseState.GetSnapshot();

            player.Tick();

            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            ErrorCode err = GL.GetError();
            if (err != ErrorCode.NoError) throw new Exception(err.ToString());
            
            if (ClientSize != lastClientSize)
            {
                GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
            }
            
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            ActiveShader = shaderProgramManager.GetProgram("tile");
            ActiveShader.Use();

            model = Matrix4.Identity;
            view = Matrix4.CreateTranslation(-player.Pos) * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(player.YRotation)) * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(player.XRotation));
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45), (float)ClientSize.X / ClientSize.Y, 0.1f, 200.0f);

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
