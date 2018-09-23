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
using ReOsuStoryBoardPlayer.Graphics;
using System.IO;
using System.Text.RegularExpressions;

namespace ReOsuStoryBoardPlayer
{
    public class StoryboardWindow : GameWindow
    {
        public static StoryboardWindow CurrentWindow { get; set; }

        StoryBoardInstance instance;

        Texture background_texture;

        RectangleF background_rect;

        const float SB_WIDTH = 640.0f, SB_WIDE_WIDTH = 747.0f, SB_HEIGHT = 480.0f;

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

        void ResizeByResolutionRadio(float radio)
        {
            if (this.Width * 1.0f / this.Height != radio)
            {
                //force resize
                var actual_width = (int)(radio * this.Height);
                Log.Warn($"Resize window width from {this.Width} to {actual_width}");
                this.Width = actual_width;
            }
        }

        public void InitWindowRenderSize(bool is_wide_screen = false)
        {
            Log.User("Init window size as " + (is_wide_screen ? "WideScreen" : "DefaultScreen"));

            CameraViewMatrix = Matrix4.Identity;

            float radio = (is_wide_screen ? 747.0f : 640.0f) / 480f;
            ResizeByResolutionRadio(radio);

            float x_offset, y_offset;

            ///todo,不确定对不对
            if (!is_wide_screen)
            {
                ProjectionMatrix = Matrix4.Identity * Matrix4.CreateOrthographic(SB_WIDTH, SB_HEIGHT, 0, 100);

                x_offset = (Width - SB_WIDTH) / SB_WIDTH;
                y_offset = -(Height - SB_HEIGHT) / SB_HEIGHT;
            }
            else
            {
                ProjectionMatrix = Matrix4.Identity * Matrix4.CreateOrthographic(SB_WIDE_WIDTH, SB_HEIGHT, 0, 100);

                x_offset = /*(Width - SB_WIDE_WIDTH) / SB_WIDTH*/+107 / 747.0f;
                y_offset = -(Height - SB_HEIGHT) / SB_HEIGHT;
            }

            CameraViewMatrix = CameraViewMatrix * Matrix4.CreateTranslation(x_offset, y_offset, 0);
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
            instance.BuildCacheDrawSpriteBatch();
            InitBackgroundDrawing(instance);
        }

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

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.ClearColor(Color.Black);

            DrawBackground();

            instance.PostDrawStoryBoard();

            DrawBlackBlank();

            SwapBuffers();
        }

        private void DrawBackground()
        {
            if (background_texture == null)
                return;
            
            DrawUtils.DrawTexture(
                background_texture,
                new Vector(-Width/2,-240), 
                new Vector(0,0), 
                new Vector(background_rect.Width,background_rect.Height),
                180, 
                background_texture.Width, 
                background_texture.Height, 
                new Vec4(1, 1, 1, 1));
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
