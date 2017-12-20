using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using OpenTK.Input;

namespace ReOsuStoryBoardPlayer
{
    public class StoryboardWindow : GameWindow
    {
        public static StoryboardWindow CurrentWindow { get; set; }

        StoryBoardInstance instance;

        public static Matrix4 CameraViewMatrix { get; set; } = Matrix4.Identity;

        public static Matrix4 ProjectionMatrix { get; set; } = Matrix4.Identity;

        public StoryboardWindow(int width = 640, int height = 480) : base(width, height, new GraphicsMode(ColorFormat.Empty, 32), "Esu!StoryBoardPlayer"
            , GameWindowFlags.FixedWindow, DisplayDevice.Default, 3, 3, GraphicsContextFlags.Default)
        {
            InitGraphics();
            VSync = VSyncMode.Off;
            Log.Init();
            CurrentWindow = this;
        }

        private void InitGraphics()
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        public new void Run()
        {
            instance.Start();
            base.Run();
        }
        
        public void LoadStoryboardInstance(StoryBoardInstance instance)
        {
            this.instance = instance;
            instance.BuildCacheDrawSpriteBatch();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.ClearColor(Color.Black);

            CameraViewMatrix = Matrix4.Identity;

            ProjectionMatrix = Matrix4.Identity * Matrix4.CreateOrthographic(Width, Height, 0, 100);

            CameraViewMatrix = CameraViewMatrix * Matrix4.CreateTranslation(-0 / (Width / 2.0f), 0 / (Height / 2.0f), 0);

            instance.PostDrawStoryBoard();

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            instance.Update((float)e.Time);
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
            if (e.Mouse.LeftButton == ButtonState.Pressed)
            {
                var x = e.X;
                var y = e.Y;

                instance.DebugToolInstance.SelectObjectIntoVisualizer(x, y);
            }
            else if(e.Mouse.RightButton == ButtonState.Pressed)
            {
                instance.DebugToolInstance.CannelSelectObject();
            }
        }
#endif
    }
}
