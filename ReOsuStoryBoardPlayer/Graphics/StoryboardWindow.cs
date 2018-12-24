using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using ReOsuStoryBoardPlayer.Base;
using ReOsuStoryBoardPlayer.Commands;
using ReOsuStoryBoardPlayer.DebugTool;
using ReOsuStoryBoardPlayer.Graphics;
using ReOsuStoryBoardPlayer.Kernel;
using ReOsuStoryBoardPlayer.Player;
using ReOsuStoryBoardPlayer.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using ReOsuStoryBoardPlayer.Graphics.PostProcesses;

namespace ReOsuStoryBoardPlayer
{
    public class StoryboardWindow : GameWindow
    {
        #region Field&Property

        private const string TITLE = "Esu!StoryBoardPlayer ({0}x{1}) OpenGL:{2}.{3} Update: {4}ms Render: {5}ms Other: {6}ms FPS: {7:F2}";

        public static StoryboardWindow CurrentWindow { get; set; }
        public float ViewWidth { get; private set; }
        public float ViewHeight { get; private set; }

        public StoryBoardInstance StoryBoardInstance { get; private set; }

        private bool ready = false;

        private PostProcessesManager _postProcessesManager;
        private List<SpriteInstanceGroup> register_sprites = new List<SpriteInstanceGroup>();

        public const float SB_WIDTH = 640f, SB_HEIGHT = 480f;
        
        private const double SYNC_THRESHOLD_MIN = 17;// 1/60fps

        private double _timestamp = 0;
        private Stopwatch _timestamp_stopwatch = new Stopwatch();

        private const float THOUSANDTH = 1.0f/1000.0f;
        private Stopwatch _title_update_stopwatch = new Stopwatch();
        private Stopwatch _update_stopwatch = new Stopwatch();
        private Stopwatch _render_stopwatch = new Stopwatch();
        private double title_update_timer = 0;

        private int WindowedWidth, WindowedHeight;

        public bool IsFullScreen => WindowState==WindowState.Fullscreen;

        public bool IsBorderless => WindowBorder==WindowBorder.Hidden;

        public static Matrix4 CameraViewMatrix { get; set; } = Matrix4.Identity;

        public static Matrix4 ProjectionMatrix { get; set; } = Matrix4.Identity;
        
        #endregion

        public StoryboardWindow(int width = 640, int height = 480) : base(width, height, new GraphicsMode(ColorFormat.Empty, 32), "Esu!StoryBoardPlayer"
            , GameWindowFlags.FixedWindow, DisplayDevice.Default, 3, 3, GraphicsContextFlags.Default)
        {
            InitGraphics();
            DrawUtils.Init();
            VSync = VSyncMode.Off;
            Log.Init();
            CurrentWindow = this;

            ApplyBorderless(Setting.EnableBorderless);
            SwitchFullscreen(Setting.EnableFullScreen);
        }

        private void AddDefaultPostProcesses()
        {
            _postProcessesManager.AddPostProcess(new ClipPostProcess());
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Width, Height);
            ApplyWindowRenderSize();
        }

        private void InitGraphics()
        {
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
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
                WindowedWidth = Width;
                WindowedHeight = Height;
                WindowState = WindowState.Fullscreen;
                Width = DisplayDevice.Default.Width;
                Height = DisplayDevice.Default.Height;
                ApplyWindowRenderSize();
            }
            else
            {
                WindowState = WindowState.Normal;
                Width = WindowedWidth;
                Height = WindowedHeight;
                ApplyWindowRenderSize();
            }
        }

        public void ApplyWindowRenderSize()
        {
            //裁剪View
            float radio = (float)Width / Height;

            ViewHeight = SB_HEIGHT;
            ViewWidth = SB_HEIGHT * radio;

            ProjectionMatrix = Matrix4.Identity * Matrix4.CreateOrthographic(ViewWidth, ViewHeight, -1, 1);
            CameraViewMatrix = Matrix4.Identity;

            _postProcessesManager = new PostProcessesManager(Width,Height);
            AddDefaultPostProcesses();
        }
        
        internal void BuildCacheDrawSpriteBatch(IEnumerable<StoryBoardObject> StoryboardObjectList,string folder_path)
        {

            Dictionary<string, SpriteInstanceGroup> CacheDrawSpriteInstanceMap = new Dictionary<string, SpriteInstanceGroup>();

            foreach (var obj in StoryboardObjectList)
            {
                switch (obj)
                {
                    case StoryboardBackgroundObject background:
                        if (!_get(obj.ImageFilePath.ToLower(), out obj.RenderGroup))
                            Log.Warn($"not found image:{obj.ImageFilePath}");

                        if (background.RenderGroup!=null)
                        {
                            var scale = SB_HEIGHT/background.RenderGroup.Texture.Height;
                            background.AddCommand(new ScaleCommand()
                            {
                                Easing=/*EasingConverter.CacheEasingInterpolatorMap[Easing.Linear]*/EasingTypes.None,
                                StartTime=-2857,
                                EndTime=-2857,
                                StartValue=scale,
                                EndValue=scale
                            });
                        }
                        break;

                    case StoryboardAnimation animation:
                        List<SpriteInstanceGroup> list = new List<SpriteInstanceGroup>();

                        for (int index = 0; index<animation.FrameCount; index++)
                        {
                            SpriteInstanceGroup group;
                            string path = animation.FrameBaseImagePath+index+animation.FrameFileExtension;
                            if (!_get(path, out group))
                            {
                                Log.Warn($"not found image:{path}");
                                continue;
                            }
                            list.Add(group);
                        }

                        animation.backup_group=list.ToArray();
                        break;

                    default:
                        if (!_get(obj.ImageFilePath.ToLower(), out obj.RenderGroup))
                            Log.Warn($"not found image:{obj.ImageFilePath}");
                        break;
                }
            }

            register_sprites=CacheDrawSpriteInstanceMap.Values.ToList();

            bool _get(string image_name,out SpriteInstanceGroup group)
            {
                //for Flex
                if (string.IsNullOrWhiteSpace(Path.GetExtension(image_name)))
                    image_name+=".png";
                
                if (CacheDrawSpriteInstanceMap.TryGetValue(image_name, out group))
                    return true;

                //load
                string file_path = Path.Combine(folder_path, image_name);
                Texture tex = new Texture(file_path);

                group=CacheDrawSpriteInstanceMap[image_name]=new SpriteInstanceGroup((uint)Setting.DrawCallInstanceCountMax, file_path, tex);

                Log.Debug($"Created storyboard sprite instance from image file :{file_path}");

                return group!=null;
            }
        }

        public void PushDelegate(Action action)
        {
            ExecutorSync.PostTask(action);
        }

        /// <summary>
        /// 将SB实例加载到Window上，后者将会自动调用instance.Update()并渲染
        /// </summary>
        /// <param name="instance"></param>
        public void LoadStoryboardInstance(StoryBoardInstance instance)
        {
            ready=false;

            if (this.StoryBoardInstance!=null)
                Clean();

            this.StoryBoardInstance=instance;
            StoryboardInstanceManager.ApplyInstance(instance);

            using (StopwatchRun.Count("Loaded image resouces and sprite instances."))
            {
                BuildCacheDrawSpriteBatch(instance.StoryboardObjectList, instance.Info.folder_path);
            }

            instance.Flush();

            ready=true;
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        private void Clean()
        {
            foreach (var sprite in register_sprites)
                sprite.Dispose();

            register_sprites.Clear();

            foreach (var obj in StoryBoardInstance?.StoryboardObjectList)
                obj.RenderGroup=null;
        }

        private double GetSyncTime()
        {
            var audioTime = MusicPlayerManager.ActivityPlayer.CurrentTime;
            var playbackRate = MusicPlayerManager.ActivityPlayer.PlaybackSpeed;

            double step = _timestamp_stopwatch.ElapsedMilliseconds*playbackRate;
            _timestamp_stopwatch.Restart();

            if (MusicPlayerManager.ActivityPlayer.IsPlaying&&Setting.EnableTimestamp)
            {
                double nextTime = _timestamp+step;

                double diffAbs = Math.Abs(nextTime-audioTime)*playbackRate;
                if (diffAbs>SYNC_THRESHOLD_MIN*playbackRate)//不同步
                {
                    if (audioTime>_timestamp)//音频快
                    {
                        nextTime+=diffAbs*0.6;//SB快速接近音频
                    }
                    else//SB快
                    {
                        nextTime=_timestamp;//SB不动
                    }
                }

                return _timestamp=nextTime;
            }
            else
            {
                return _timestamp=MusicPlayerManager.ActivityPlayer.CurrentTime;
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            _update_stopwatch.Restart();

            ExecutorSync.ClearTask();
            var time = GetSyncTime();

            if (!ready)
                return;

            StoryBoardInstance.Update((float)time);
            DebuggerManager.FrameUpdate();

            _update_stopwatch.Stop();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            _render_stopwatch.Restart();

            GL.Clear(ClearBufferMask.ColorBufferBit);

            if (ready)
            {
                DebuggerManager.TrigBeforeRender();
                _postProcessesManager.Begin();
                {
                    PostDrawStoryBoard();
                    _postProcessesManager.Process();
                }
                _postProcessesManager.End();
                DebuggerManager.TrigAfterRender();
            }

            SwapBuffers();

            _render_stopwatch.Stop();
            FrameRateLimit();
        }

        private void FrameRateLimit()
        {
            long total_time = _title_update_stopwatch.ElapsedMilliseconds;
            _title_update_stopwatch.Restart();

            //UpdateTitle
            if (title_update_timer > 0.2)
            {
                Title = string.Format(TITLE, Width, Height,
                    GL.GetInteger(GetPName.MajorVersion),
                    GL.GetInteger(GetPName.MinorVersion),
                    _update_stopwatch.ElapsedMilliseconds,
                    _render_stopwatch.ElapsedMilliseconds,
                    (total_time - _update_stopwatch.ElapsedMilliseconds - _render_stopwatch.ElapsedMilliseconds)
                    , RenderFrequency);
                title_update_timer = 0;
            }

            title_update_timer += total_time * THOUSANDTH;
            
            if (Setting.EnableHighPrecisionFPSLimit)
            {
                if (Math.Abs(TargetUpdateFrequency - Setting.MaxFPS) > 10e-5)
                {
                    TargetUpdateFrequency = Setting.MaxFPS;
                    TargetRenderFrequency = Setting.MaxFPS;
                }
            }
            else
            {
                float time = (_update_stopwatch.ElapsedMilliseconds + _render_stopwatch.ElapsedMilliseconds) * THOUSANDTH;
                if (Setting.MaxFPS != 0)
                {
                    float period = 1.0f / Setting.MaxFPS;
                    if (period > time)
                    {
                        int sleep = (int) ((period - time) * 1000);
                        sleep = Math.Max(0, sleep - 1);
                        Thread.Sleep(sleep);
                    }
                }
            }
            
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Environment.Exit(0);
        }
        
        #region Storyboard Rendering

        private void PostDrawStoryBoard()
        {
            if (StoryBoardInstance.UpdatingStoryboardObjects.Count==0)
                return;

            DrawStoryBoardObjects(StoryBoardInstance.UpdatingStoryboardObjects);

            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        private void DrawStoryBoardObjects(List<StoryBoardObject> draw_list)
        {
            SpriteInstanceGroup group = draw_list.First().RenderGroup;

            bool additive_trigger;
            ChangeAdditiveStatus(draw_list.First().IsAdditive);

            for (int i = 0; i<draw_list.Count; i++)
            {
                var obj = draw_list[i];
#if DEBUG
                if (!obj.DebugShow)
                    continue;//skip
#endif
                if (group!=obj.RenderGroup||additive_trigger!=obj.IsAdditive)
                {
                    group?.FlushDraw();

                    //应该是现在设置Blend否则Group自动渲染来不及钦定
                    ChangeAdditiveStatus(obj.IsAdditive);


                    group=obj.RenderGroup;
                }

                group?.PostRenderCommand(obj.Postion, obj.Z, obj.Rotate, obj.Scale, obj.Anchor, obj.Color, obj.IsVerticalFlip, obj.IsHorizonFlip);
            }

            //ChangeAdditiveStatus(false);

            if (group?.CurrentPostCount!=0)
                group?.FlushDraw();

            void ChangeAdditiveStatus(bool is_additive_blend)
            {
                additive_trigger=is_additive_blend;
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, additive_trigger ? BlendingFactorDest.One : BlendingFactorDest.OneMinusSrcAlpha);
            }
        }

        #endregion Storyboard Rendering

        #region Input Process

        //解决窗口失去/获得焦点时鼠标xjb移动
        protected override void OnFocusedChanged(EventArgs e){}

        protected override void OnKeyDown(KeyboardKeyEventArgs e) => DebuggerManager.TrigKeyPress(e.Key);

        private int downX, downY;
        private bool mouseDown = false;

#if DEBUG
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            //如果是无边窗就当作拖曳窗口操作
            if (WindowBorder == WindowBorder.Hidden)
            {
                downX = e.X;
                downY = e.Y;
            }
            else
            {
                DebuggerManager.TrigClick(e.X, e.Y,e.Mouse.RightButton==ButtonState.Pressed ? MouseInput.Right : MouseInput.Left);
            }

            mouseDown = true;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            mouseDown = false;
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);

            if (mouseDown&&WindowBorder == WindowBorder.Hidden)
            {
                Location = new Point(e.X+Location.X-downX,e.Y+Location.Y-downY);
                //Log.User($"X: ${e.X} Y:${e.Y}");
            }
            else
            {
                DebuggerManager.TrigMove(e.X, e.Y);
            }
        }
#endif

        #endregion
    }
}