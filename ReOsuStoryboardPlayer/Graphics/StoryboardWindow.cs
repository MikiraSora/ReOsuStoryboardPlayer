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

namespace ReOsuStoryboardPlayer
{
    public class StoryboardWindow : GameWindow
    {
        #region Field&Property

        private const string TITLE = "Esu!StoryboardPlayer ({0}x{1}) OpenGL:{2}.{3} Update: {4}ms Render: {5}ms Other: {6}ms FPS: {7:F2} Objects: {8} {9}";

        public static StoryboardWindow CurrentWindow { get; set; }
        public float ViewWidth { get; private set; }
        public float ViewHeight { get; private set; }

        public StoryboardInstance Instance { get; private set; }

        private bool ready = false;

        private bool _existClipPostProcess = false;
        private ClipPostProcess _clipPostProcess;
        private PostProcessesManager _postProcessesManager;

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

        #endregion Field&Property

        public StoryboardWindow(int width = 640, int height = 480) : base(width, height, new GraphicsMode(ColorFormat.Empty, 32), "Esu!StoryboardPlayer"
            , GameWindowFlags.FixedWindow, DisplayDevice.Default, 3, 3, GraphicsContextFlags.ForwardCompatible)
        {
            InitGraphics();
            DrawUtils.Init();
            VSync=VSyncMode.Off;
            CurrentWindow=this;

            ApplyBorderless(PlayerSetting.EnableBorderless);
            SwitchFullscreen(PlayerSetting.EnableFullScreen);

            _clipPostProcess=new ClipPostProcess();
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
                WindowedWidth=Width;
                WindowedHeight=Height;
                WindowState=WindowState.Fullscreen;
                Width=DisplayDevice.Default.Width;
                Height=DisplayDevice.Default.Height;
                ApplyWindowRenderSize();
            }
            else
            {
                WindowState=WindowState.Normal;
                Width=WindowedWidth;
                Height=WindowedHeight;
                ApplyWindowRenderSize();
            }
        }

        public void ApplyWindowRenderSize()
        {
            //裁剪View
            float radio = (float)Width/Height;

            ViewHeight=SB_HEIGHT;
            ViewWidth=SB_HEIGHT*radio;

            ProjectionMatrix=Matrix4.Identity*Matrix4.CreateOrthographic(ViewWidth, ViewHeight, -1, 1);
            CameraViewMatrix=Matrix4.Identity;

            int sample = 1<<PlayerSetting.SsaaLevel;
            _postProcessesManager=new PostProcessesManager(Width*sample, Height*sample);
            SetupClipPostProcesses();
        }

        private void SetupClipPostProcesses()
        {
            if (_existClipPostProcess)
            {
                _postProcessesManager.RemovePostProcess(_clipPostProcess);
                _existClipPostProcess=false;
            }

            if (Instance.Info.IsWidescreenStoryboard==false)
            {
                if (_postProcessesManager!=null)
                {
                    _postProcessesManager.AddPostProcess(_clipPostProcess);
                    _existClipPostProcess=true;
                }
            }
        }

        internal StoryboardResource BuildDrawSpriteResources(IEnumerable<StoryboardObject> StoryboardObjectList, string folder_path)
        {
            Dictionary<string, SpriteInstanceGroup> CacheDrawSpriteInstanceMap = new Dictionary<string, SpriteInstanceGroup>();

            StoryboardResource resource = new StoryboardResource();

            foreach (var obj in StoryboardObjectList)
            {
                SpriteInstanceGroup group;
                switch (obj)
                {
                    case StoryboardBackgroundObject background:
                        if (!_get(obj.ImageFilePath.ToLower(), out group))
                            Log.Warn($"not found image:{obj.ImageFilePath}");

                        if (group!=null)
                        {
                            var scale = SB_HEIGHT/group.Texture.Height;
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
                            string path = animation.FrameBaseImagePath+index+animation.FrameFileExtension;
                            if (!_get(path, out group))
                            {
                                Log.Warn($"not found image:{path}");
                                continue;
                            }
                            list.Add(group);
                        }

                        break;

                    default:
                        if (!_get(obj.ImageFilePath.ToLower(), out group))
                            Log.Warn($"not found image:{obj.ImageFilePath}");
                        break;
                }
            }

            resource.PinSpriteInstanceGroups(CacheDrawSpriteInstanceMap);

            return resource;

            bool _get(string image_name, out SpriteInstanceGroup group)
            {
                var fix_image = image_name;
                //for Flex
                if (string.IsNullOrWhiteSpace(Path.GetExtension(fix_image)))
                    fix_image+=".png";

                if (CacheDrawSpriteInstanceMap.TryGetValue(image_name, out group))
                    return true;

                //load
                string file_path = Path.Combine(folder_path, fix_image);

                if (!_load_tex(file_path, out var tex))
                {
                    file_path=Path.Combine(PlayerSetting.UserSkinPath, fix_image);
                    _load_tex(fix_image, out tex);
                }

                if (tex!=null)
                {
                    group=CacheDrawSpriteInstanceMap[image_name]=new SpriteInstanceGroup((uint)PlayerSetting.DrawCallInstanceCountMax, fix_image, tex);
                    Log.Debug($"Created Storyboard sprite instance from image file :{fix_image}");
                }

                return group!=null;
            }

            bool _load_tex(string file_path, out Texture texture)
            {
                texture=null;

                try
                {
                    texture=new Texture(file_path);
                }
                catch (Exception e)
                {
                    Log.Warn($"Load texture \"{file_path}\" failed : {e.Message}");
                    texture=null;
                }

                return texture!=null;
            }
        }

        /// <summary>
        /// 将SB实例加载到Window上，后者将会自动调用instance.Update()并渲染
        /// </summary>
        /// <param name="instance"></param>
        public void LoadStoryboardInstance(StoryboardInstance instance)
        {
            ready=false;

            if (this.Instance!=null)
                Clean();

            this.Instance=instance;
            StoryboardInstanceManager.ApplyInstance(instance);
            SetupClipPostProcesses();

            using (StopwatchRun.Count("Loaded image resouces and sprite instances."))
            {
                var resource = BuildDrawSpriteResources(instance.Updater.StoryboardObjectList, instance.Info.folder_path);
                instance.Resource=resource;
            }

            _timestamp=0;

            instance.SetupBackgroundObject();

            ready=true;
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        private void Clean()
        {
            Instance.Resource.Dispose();
        }

        private double GetSyncTime()
        {
            var audioTime = MusicPlayerManager.ActivityPlayer.CurrentTime;
            var playbackRate = MusicPlayerManager.ActivityPlayer.PlaybackSpeed;

            double step = _timestamp_stopwatch.ElapsedMilliseconds*playbackRate;
            _timestamp_stopwatch.Restart();

            if (MusicPlayerManager.ActivityPlayer.IsPlaying&&PlayerSetting.EnableTimestamp)
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

            Instance.Updater.Update((float)time);
            ToolManager.FrameUpdate();

            _update_stopwatch.Stop();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            _render_stopwatch.Restart();

            if (ready)
            {
                ToolManager.TrigBeforeRender();
                _postProcessesManager.Begin();
                {
                    PostDrawStoryboard();
                    _postProcessesManager.Process();
                }
                _postProcessesManager.End();
                ToolManager.TrigAfterRender();
            }

            SwapBuffers();

            _render_stopwatch.Stop();
            FrameRateLimit();
        }

        int prev_playing_time;
        DateTime prev_encoding_time=DateTime.Now;

        private void FrameRateLimit()
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
                    _update_stopwatch.ElapsedMilliseconds,
                    _render_stopwatch.ElapsedMilliseconds,
                    (total_time-_update_stopwatch.ElapsedMilliseconds-_render_stopwatch.ElapsedMilliseconds)
                    , RenderFrequency, Instance?.Updater.UpdatingStoryboardObjects?.Count??0, title_encoding_part);
                title_update_timer=0;
            }

            title_update_timer+=total_time*THOUSANDTH;

            if (PlayerSetting.EnableHighPrecisionFPSLimit)
            {
                if (Math.Abs(TargetUpdateFrequency-PlayerSetting.MaxFPS)>10e-5)
                {
                    TargetUpdateFrequency=PlayerSetting.MaxFPS;
                    TargetRenderFrequency=PlayerSetting.MaxFPS;
                }
            }
            else
            {
                float time = (_update_stopwatch.ElapsedMilliseconds+_render_stopwatch.ElapsedMilliseconds)*THOUSANDTH;
                if (PlayerSetting.MaxFPS!=0)
                {
                    float period = 1.0f/PlayerSetting.MaxFPS;
                    if (period>time)
                    {
                        int sleep = (int)((period-time)*1000);
                        sleep=Math.Max(0, sleep-1);
                        Thread.Sleep(sleep);
                    }
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Clean();

            MainProgram.Exit();
        }

        #region Storyboard Rendering

        private void PostDrawStoryboard()
        {
            if (Instance.Updater.UpdatingStoryboardObjects.Count==0)
                return;

            DrawStoryboardObjects(Instance.Updater.UpdatingStoryboardObjects);

            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        private void DrawStoryboardObjects(List<StoryboardObject> draw_list)
        {
            SpriteInstanceGroup group = Instance.Resource.GetSprite(draw_list.First().ImageFilePath);

            bool additive_trigger;
            ChangeAdditiveStatus(draw_list.First().IsAdditive);

            for (int i = 0; i<draw_list.Count; i++)
            {
                var obj = draw_list[i];

                if (!obj.IsVisible)
                    continue;
#if DEBUG
                if (!obj.DebugShow)
                    continue;//skip
#endif
                var obj_group = Instance.Resource.GetSprite(obj);

                if (group!=obj_group||additive_trigger!=obj.IsAdditive)
                {
                    group?.FlushDraw();

                    //应该是现在设置Blend否则Group自动渲染来不及钦定
                    ChangeAdditiveStatus(obj.IsAdditive);

                    group=obj_group;
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
        protected override void OnFocusedChanged(EventArgs e) { }

        protected override void OnKeyDown(KeyboardKeyEventArgs e) => ToolManager.TrigKeyPress(e.Key);

        private int downX, downY;
        private bool mouseDown = false;
        
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            //如果是无边窗就当作拖曳窗口操作
            if (WindowBorder==WindowBorder.Hidden)
            {
                downX=e.X;
                downY=e.Y;
            }
            else
            {
                ToolManager.TrigClick(e.X, e.Y, e.Mouse.RightButton==ButtonState.Pressed ? MouseInput.Right : MouseInput.Left);
            }

            mouseDown=true;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            var time = -e.DeltaPrecise*125;

            if (MusicPlayerManager.ActivityPlayer is MusicPlayer player)
                player.Jump(player.CurrentTime+time, true);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            mouseDown=false;
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);

            if (mouseDown&&WindowBorder==WindowBorder.Hidden)
                Location=new Point(e.X+Location.X-downX, e.Y+Location.Y-downY);
            else
                ToolManager.TrigMove(e.X, e.Y);
        }

        #endregion Input Process
    }
}