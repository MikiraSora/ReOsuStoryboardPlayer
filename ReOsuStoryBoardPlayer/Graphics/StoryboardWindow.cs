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
        private static readonly uint DrawCallInstanceCountMax = 50;

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
            Title = $"Esu!StoryBoardPlayer ({Width}x{Height}) OpenGL:{GL.GetInteger(GetPName.MajorVersion)}.{GL.GetInteger(GetPName.MinorVersion)}";
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

                group=CacheDrawSpriteInstanceMap[image_name]=new SpriteInstanceGroup(DrawCallInstanceCountMax, file_path, tex);

                Log.Debug($"Created storyboard sprite instance from image file :{file_path}");

                return group!=null;
            }
        }

        public void PushDelegate(Action action)
        {
            other_thread_action.Enqueue(action);
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

        private const double SYNC_THRESHOLD_MIN = 10;
        private const double SYNC_THRESHOLD_MAX = 100;

        private double _timestamp = 0;
        private Stopwatch _stopwatch = new Stopwatch();

        private double GetSyncTime()
        {
            Debug.Assert(other_thread_action!=null, nameof(other_thread_action)+" != null");
            while (!other_thread_action.IsEmpty)
            {
                other_thread_action.TryDequeue(out var action);
                action();
            }

            var audioTime = MusicPlayerManager.ActivityPlayer.CurrentTime;

            double step = _stopwatch.ElapsedMilliseconds;
            _stopwatch.Restart();

            if (MusicPlayerManager.ActivityPlayer.IsPlaying)
            {
                double time = _timestamp+step;

                double diffAbs = Math.Abs(_timestamp-audioTime);
                if (diffAbs<SYNC_THRESHOLD_MAX&&diffAbs>SYNC_THRESHOLD_MIN)//不同步
                {
                    if (audioTime>_timestamp)//音频快
                    {
                        time=_timestamp+diffAbs*0.5;//SB快速接近音频
                    }
                    else//SB快
                    {
                        time=_timestamp;//SB不动
                    }
                }

                return _timestamp=time;
            }
            else
            {
                return _timestamp=MusicPlayerManager.ActivityPlayer.CurrentTime;
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

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
            foreach (var layout_list in instance.UpdatingStoryboardObjects)
            {
                if (layout_list.Value.Count==0)
                {
                    continue;
                }

                DrawStoryBoardObjects(layout_list.Value);
            }

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