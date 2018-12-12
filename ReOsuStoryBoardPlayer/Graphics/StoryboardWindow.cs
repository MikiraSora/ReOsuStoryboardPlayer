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

namespace ReOsuStoryBoardPlayer
{
    public class StoryboardWindow : GameWindow
    {
        private const string TITLE = "Esu!StoryBoardPlayer ({0}x{1}) OpenGL:{2}.{3} Update: {4:F0}ms Render: {5:F0}ms FPS: {6:F2}";

        public static StoryboardWindow CurrentWindow { get; set; }

        private StoryBoardInstance instance;

        private bool ready = false;

        private List<SpriteInstanceGroup> register_sprites = new List<SpriteInstanceGroup>();

        public const float SB_WIDTH = 640f, SB_HEIGHT = 480f;

        public static Matrix4 CameraViewMatrix { get; set; } = Matrix4.Identity;

        public static Matrix4 ProjectionMatrix { get; set; } = Matrix4.Identity;

        private ConcurrentQueue<Action> other_thread_action = new ConcurrentQueue<Action>();

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
            ApplyWindowRenderSize();
        }

        private void InitGraphics()
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        public void ApplyWindowRenderSize()
        {
            CameraViewMatrix = Matrix4.Identity;

            //裁剪View
            float radio = (float)Width / (float)Height;

            ProjectionMatrix = Matrix4.Identity * Matrix4.CreateOrthographic(SB_HEIGHT * radio, SB_HEIGHT, 0, 100);
            CameraViewMatrix = CameraViewMatrix;
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
                                Easing=EasingConverter.CacheEasingInterpolatorMap[Easing.Linear],
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

            if (this.instance!=null)
                Clean();

            this.instance=instance;
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

            foreach (var obj in instance?.StoryboardObjectList)
                obj.RenderGroup=null;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.ClearColor(Color.Black);

            if (ready)
            {
                DebuggerManager.TrigBeforeRender();
                PostDrawStoryBoard();
                DebuggerManager.TrigAfterRender();
            }

            SwapBuffers();
        }

        private const double SYNC_THRESHOLD_MIN = 17;// 1/60fps

        private double _timestamp = 0;
        private Stopwatch _stopwatch = new Stopwatch();

        private double GetSyncTime()
        {
            var audioTime = MusicPlayerManager.ActivityPlayer.CurrentTime;
            var playbackRate = MusicPlayerManager.ActivityPlayer.PlaybackSpeed;

            double step = _stopwatch.ElapsedMilliseconds*playbackRate;
            _stopwatch.Restart();

            if (MusicPlayerManager.ActivityPlayer.IsPlaying&&Setting.EnableTimestamp)
            {
                double nextTime = _timestamp + step;

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

        private double title_update_timer = 0;

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            
            ExecutorSync.ClearTask();
            if (title_update_timer > 0.1)
            {
                Title = string.Format(TITLE, Width, Height, GL.GetInteger(GetPName.MajorVersion),
                    GL.GetInteger(GetPName.MinorVersion), UpdateTime * 1000, RenderTime * 1000,
                    1.0 / (RenderTime + UpdateTime));
                title_update_timer = 0;
            }

            title_update_timer += (RenderTime + UpdateTime);

            var time = GetSyncTime();

            if (!ready)
                return;

            instance.Update((float)time);

            DebuggerManager.FrameUpdate();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Environment.Exit(0);
        }
        
        #region Storyboard Rendering

        private void PostDrawStoryBoard()
        {
            if (instance.UpdatingStoryboardObjects.Count==0)
                return;

            DrawStoryBoardObjects(instance.UpdatingStoryboardObjects);

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

                if (obj.Color.w<=0)
                    continue;//skip
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

        protected override void OnFocusedChanged(EventArgs e) { }

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