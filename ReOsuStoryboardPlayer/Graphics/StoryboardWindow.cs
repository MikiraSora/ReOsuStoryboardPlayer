using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Commands;
using ReOsuStoryboardPlayer.Core.Utils;
using ReOsuStoryboardPlayer.Tools;
using ReOsuStoryboardPlayer.Graphics;
using ReOsuStoryboardPlayer.Graphics.PostProcesses;
using ReOsuStoryboardPlayer.Kernel;
using ReOsuStoryboardPlayer.OutputEncoding.Kernel;
using ReOsuStoryboardPlayer.Player;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using ReOsuStoryBoardPlayer.OutputEncoding.Graphics.PostProcess;
using ReOsuStoryBoardPlayer.Graphics;
using ReOsuStoryBoardPlayer.Kernel;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Windows.Forms;

namespace ReOsuStoryboardPlayer
{
    public class StoryboardWindow : GameWindow
    {
        #region Field&Property

        int prev_playing_time;
        DateTime prev_encoding_time = DateTime.Now;

        private static double title_update_timer = 0;
        private static Stopwatch _title_update_stopwatch = new Stopwatch();

        private const string TITLE = "Esu!StoryboardPlayer ({0}x{1}) OpenGL:{2}.{3} Update: {4}ms Render: {5}ms Other: {6}ms FPS: {7:F2} Objects: {8} {9}";

        public static StoryboardWindow CurrentWindow { get; set; }

        private int WindowedWidth, WindowedHeight;

        public bool IsFullScreen => WindowState==WindowState.Fullscreen;

        public bool IsBorderless => WindowBorder==WindowBorder.Hidden;

        public int Width => Size.X;
        public int Height => Size.Y;

        #endregion Field&Property

        private static NativeWindowSettings StoryboardWindowSettings = new NativeWindowSettings 
        {
            API = ContextAPI.OpenGL ,
            APIVersion = new Version(3,3),
            Flags = ContextFlags.ForwardCompatible,
            Location =new Vector2i(800,600),
            Profile = ContextProfile.Compatability,
            Title = "Esu!StoryboardPlayer",
            IsEventDriven = true
        };

        public StoryboardWindow(int width = 640, int height = 480) : 
            //opentk >= 4.0.0 NOT WORK
            base(GameWindowSettings.Default, NativeWindowSettings.Default/*StoryboardWindowSettings*/) 
            //opentk < 4.0.0 WORK
            /*base(width, height, new GraphicsMode(ColorFormat.Empty, 32), "Esu!StoryboardPlayer"
            , GameWindowFlags.FixedWindow, DisplayDevice.Default, 3, 3, GraphicsContextFlags.ForwardCompatible)*/
        {
            //VSync = VSyncMode.Off;
            CurrentWindow = this;
            RenderKernel.Init();

            ApplyBorderless(PlayerSetting.EnableBorderless);
            SwitchFullscreen(PlayerSetting.EnableFullScreen);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            RenderKernel.ApplyWindowRenderSize(e.Width,e.Height);
        }

        public void ApplyBorderless(bool is_borderless)
        {
            WindowBorder=is_borderless ? WindowBorder.Hidden : WindowBorder.Fixed;
        }

        public void SwitchFullscreen(bool fullscreen)
        {
            if (fullscreen==IsFullScreen)
                return;

            SwitchFullscreen();
        }

        public void SwitchFullscreen()
        {
            if (!IsFullScreen)
            {
                WindowedWidth=Width;
                WindowedHeight=Height;
                WindowState=WindowState.Fullscreen;
                //todo Size = new Vector2i(DisplayDevice.Default.Width,DisplayDevice.Default.Height);
                RenderKernel.ApplyWindowRenderSize(Width, Height);
            }
            else
            {
                WindowState=WindowState.Normal;
                Size = new Vector2i(WindowedWidth, WindowedHeight);
                RenderKernel.ApplyWindowRenderSize(Width, Height);
            }
        }

        public void RefreshResize()
        {
            RenderKernel.ApplyWindowRenderSize(Width, Height);
        }

        public void LoadStoryboardInstance(StoryboardInstance instance)
        {
            UpdateKernel.LoadStoryboardInstance(instance);
            RenderKernel.LoadStoryboardInstance(instance);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            UpdateKernel.Update();

            UpdateKernel.FrameRateLimit();

            UpdateTitle();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            RenderKernel.Draw();

            SwapBuffers();
        }

        private void UpdateTitle()
        {
            long total_time = _title_update_stopwatch.ElapsedMilliseconds;
            _title_update_stopwatch.Restart();

            //UpdateTitle
            if (title_update_timer>0.2)
            {
                string title_encoding_part = string.Empty;

                if (PlayerSetting.EncodingEnvironment)
                {
                    var kernel = ToolManager.GetTool<EncodingKernel>();

                    //calc ETA
                    var now_time = DateTime.Now;
                    var encoded_time = (now_time-prev_encoding_time).TotalMilliseconds;
                    prev_encoding_time=now_time;

                    var time=(int)(MusicPlayerManager.ActivityPlayer?.CurrentTime??0);
                    var end_time = kernel.Option.IsExplicitTimeRange ? (uint)kernel.Option.EndTime : MusicPlayerManager.ActivityPlayer.Length;
                    var past_time = (time-prev_playing_time);
                    prev_playing_time=time;

                    var eta = past_time==0?0:encoded_time*((end_time-time)/past_time);
                    var span = TimeSpan.FromMilliseconds(eta);

                    title_encoding_part =$" Encoding Frame:{kernel.Writer.ProcessedFrameCount} Timestamp:{kernel.Writer.ProcessedTimestamp} ETA:{span}";
                }

                Title=string.Format(TITLE, Width, Height,
                    GL.GetInteger(GetPName.MajorVersion),
                    GL.GetInteger(GetPName.MinorVersion),
                    UpdateKernel.UpdateCostTime,
                    RenderKernel.RenderCostTime,
                    (total_time- UpdateKernel.UpdateCostTime - RenderKernel.RenderCostTime)
                    , RenderFrequency, UpdateKernel.Instance?.Updater.UpdatingStoryboardObjects?.Count??0, title_encoding_part);
                title_update_timer=0;
            }

            title_update_timer+=total_time* UpdateKernel.THOUSANDTH;
        }

        protected override void OnClosed()
        {
            base.OnClosed();

            RenderKernel.Clean();

            MainProgram.Exit();
        }

        #region Storyboard Rendering

        #endregion Storyboard Rendering

        #region Input Process

        //解决窗口失去/获得焦点时鼠标xjb移动
        protected override void OnFocusedChanged(FocusedChangedEventArgs e) { }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            ToolManager.TrigKeyPress(e.Key);
        }

        private float downX, downY;
        private bool mouseDown = false;
        
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            //如果是无边窗就当作拖曳窗口操作
            if (WindowBorder==WindowBorder.Hidden)
            {
                downX= MousePosition.X;
                downY= MousePosition.Y;
            }
            else
            {
                ToolManager.TrigClick(MousePosition.X, MousePosition.Y, e.Button == MouseButton.Right && e.Action  == InputAction.Press ? MouseInput.Right : MouseInput.Left);
            }

            mouseDown=true;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            ToolManager.TrigMouseWheel(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            mouseDown=false;
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);

            if (mouseDown && WindowBorder == WindowBorder.Hidden)
                Location = new Vector2i((int)(MousePosition.X + Location.X - downX), (int)(MousePosition.Y + Location.Y - downY));
            else
                ToolManager.TrigMove(e.X, e.Y);
        }

        #endregion Input Process
    }
}