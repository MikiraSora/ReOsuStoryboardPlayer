using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
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

        /*
        private void InitBackgroundDrawing(StoryBoardInstance instance)
        {
            var match = Regex.Match(File.ReadAllText(instance.osu_file_path), @"\""((.+?)\.((jpg)|(png)|(jpeg)))\""", RegexOptions.IgnoreCase);
            string bgPath = Path.Combine(instance.folder_path,match.Groups[1].Value);

            try
            {
                //load background image
                background_texture = new Texture(bgPath);

                if ((background_texture.Width * 1.0 / background_texture.Height) < (Width * 1.0 / Height))
                {
                    float w_scale = (float)Width / background_texture.Width;
                    float actual_height = background_texture.Height * w_scale;

                    float y_offset = actual_height - Height / 2;

                    background_rect = new RectangleF(-Width / 2, -y_offset, w_scale, w_scale);
                }
                else
                {
                    float h_scale = (float)Height / background_texture.Height;
                    float actual_height = background_texture.Width * h_scale;

                    float x_offset = actual_height - Height / 2;

                    background_rect = new RectangleF(-x_offset, -Height / 2, h_scale, h_scale);
                }
            }
            catch (Exception e)
            {
                Log.Warn($"load background image failed, from {bgPath} ,message:{e.Message}");
                background_texture = null;
            }
        }
        */

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

                Console.WriteLine($"Mouse:({x},{y})");

                instance.DebugToolInstance.SelectObjectIntoVisualizer(x, y);
            }
            else if (e.Mouse.RightButton == ButtonState.Pressed)
            {
                instance.DebugToolInstance.CannelSelectObject();
            }
        }

#endif
    }
}