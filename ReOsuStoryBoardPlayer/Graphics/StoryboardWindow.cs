using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using ReOsuStoryBoardPlayer.DebugTool;
using ReOsuStoryBoardPlayer.Graphics;
using ReOsuStoryBoardPlayer.Utils;
using System;
using System.Drawing;

namespace ReOsuStoryBoardPlayer
{
    public class StoryboardWindow : GameWindow
    {
        public static StoryboardWindow CurrentWindow { get; set; }

        private StoryBoardInstance instance;

        public const float SB_WIDTH = 640f, SB_HEIGHT = 480f;

        public static Matrix4 CameraViewMatrix { get; set; } = Matrix4.Identity;

        public static Matrix4 ProjectionMatrix { get; set; } = Matrix4.Identity;

        public StoryboardWindow(int width = 640, int height = 480) : base(width, height, new GraphicsMode(ColorFormat.Empty, 32), "Esu!StoryBoardPlayer"
            , GameWindowFlags.FixedWindow, DisplayDevice.Default, 3, 3, GraphicsContextFlags.Default)
        {
            InitGraphics();
            DrawUtils.Init();
            VSync = VSyncMode.Off;
            Log.Init();
            CurrentWindow = this;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Width, Height);
            Title = $"Esu!StoryBoardPlayer ({Width}x{Height}) OpenGL:{GL.GetInteger(GetPName.MajorVersion)}.{GL.GetInteger(GetPName.MinorVersion)}";
        }

        private void InitGraphics()
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        public void InitWindowRenderSize(bool is_wide_screen = false)
        {
            Log.User("Init window size as " + (is_wide_screen ? "WideScreen" : "DefaultScreen"));

            CameraViewMatrix = Matrix4.Identity;

            //裁剪View
            float radio = (float)Width / (float)Height;

            ProjectionMatrix = Matrix4.Identity * Matrix4.CreateOrthographic(SB_HEIGHT * radio, SB_HEIGHT, 0, 100);
            CameraViewMatrix = CameraViewMatrix;
        }

        public new void Run()
        {
            instance.Start();
            base.Run();
        }

        public void LoadStoryboardInstance(StoryBoardInstance instance)
        {
            this.instance = instance;
            InitWindowRenderSize(instance.IsWideScreen);

            using (StopwatchRun.Count("Loaded image resouces and sprite instances."))
            {
                instance.BuildCacheDrawSpriteBatch();
            }

            //InitBackgroundDrawing(instance);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.ClearColor(Color.Black);

            //DrawBackground();

            instance.PostDrawStoryBoard();

            DrawBlackBlank();

            SwapBuffers();
        }

        private void DrawBlackBlank()
        {
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            instance.Update((float)e.Time);

            DebuggerManager.FrameUpdate();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Environment.Exit(0);
        }

#if DEBUG

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            DebuggerManager.TrigClick(e.X, e.Y, 
                e.Mouse.RightButton==ButtonState.Pressed ? MouseInput.Right : MouseInput.Left);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);
            DebuggerManager.TrigMove(e.X, e.Y);
        }

#endif
    }
}