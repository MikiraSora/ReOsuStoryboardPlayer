using ReOsuStoryboardPlayer.Graphics;
using ReOsuStoryboardPlayer.Graphics.PostProcesses;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using ReOsuStoryboardPlayer;
using ReOsuStoryboardPlayer.Kernel;
using ReOsuStoryboardPlayer.Core.Utils;
using ReOsuStoryboardPlayer.Tools;
using ReOsuStoryboardPlayer.Core.Base;
using System.Diagnostics;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ReOsuStoryBoardPlayer.Graphics
{
    public static class RenderKernel
    {
        public static int Width { get; private set; }
        public static int Height { get; private set; }

        private static ClipPostProcess _clipPostProcess;
        private static Stopwatch _render_stopwatch = new Stopwatch();
        private static bool ready;

        public static int DefaultFrameBuffer { get; set; } = 0;

        public static PostProcessesManager PostProcessesManager { get; private set; } = new PostProcessesManager();

        public static void Init()
        {
            InitGraphics();
            DrawUtils.Init();
            PostProcessesManager.Init();

            _clipPostProcess = new ClipPostProcess();
        }

        public static void NewContextReInit()
        {
            Init();

            try
            {
                //rebuild resource beacuse they use textures.
                Clean();
                BuildResource(Instance);
            }
            catch
            {

            }
        }

        private static void InitGraphics()
        {
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        public const float SB_WIDTH = 640f, SB_WIDE_WIDTH = SB_WIDTH + 2 * 107, SB_HEIGHT = 480f;

        public static float ViewWidth { get; private set; }
        public static float ViewHeight { get; private set; }

        public static Matrix4 CameraViewMatrix { get; set; } = Matrix4.Identity;

        public static Matrix4 ProjectionMatrix { get; set; } = Matrix4.Identity;
        public static StoryboardInstance Instance { get; private set; }

        public static void ApplyWindowRenderSize(int width,int height)
        {
            Log.Debug($"Width {Width} -> {width}  , Height {Height} -> {height}");

            //裁剪View
            Width = width;
            Height = height;
            GL.Viewport(0, 0, Width, Height);
            float radio = (float)Width / Height;

            ViewHeight = SB_HEIGHT;
            ViewWidth = SB_HEIGHT * radio;

            ProjectionMatrix = Matrix4.Identity * Matrix4.CreateOrthographic(ViewWidth, ViewHeight, -1, 1);
            CameraViewMatrix = Matrix4.Identity;

            if (PlayerSetting.FrameHeight != Height ||
                PlayerSetting.FrameWidth != Width)
                PostProcessesManager.Resize(PlayerSetting.FrameWidth, PlayerSetting.FrameHeight);
            else
                PostProcessesManager.Resize(Width, Height);
            SetupClipPostProcesses();
        }

        private static void SetupClipPostProcesses()
        {
            if (!(Instance?.Info?.IsWidescreenStoryboard??false))
            {
                PostProcessesManager.AddPostProcess(_clipPostProcess);
            }
            else
            {
                PostProcessesManager.RemovePostProcess(_clipPostProcess);
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public static void Clean()
        {
            Instance.Resource.Dispose();
            Instance.Resource = null;
        }

        /// <summary>
        /// 将SB实例加载到Window上，后者将会自动调用instance.Update()并渲染
        /// </summary>
        /// <param name="instance"></param>
        public static void LoadStoryboardInstance(StoryboardInstance instance)
        {
            ready = false;

            if (Instance != null)
                Clean();

            Instance = instance;
            StoryboardInstanceManager.ApplyInstance(instance);
            SetupClipPostProcesses();

            BuildResource(instance);

            ready = true;
        }

        private static void BuildResource(StoryboardInstance instance)
        {
            if (instance.Resource!=null)
            {
                try
                {
                    instance.Resource.Dispose();
                    instance.Resource = null;
                }
                catch{}
            }

            using (StopwatchRun.Count("Loaded image resouces and sprite instances."))
            {
                var resource = StoryboardResource.BuildDefaultResource(instance.Updater.StoryboardObjectList, instance.Info.folder_path);
                instance.Resource = resource;
            }
        }

        public static void Draw()
        {
            _render_stopwatch.Restart();

            if (ready)
            {
                ToolManager.TrigBeforeRender();
                PostProcessesManager.Begin();
                {
                   PostDrawStoryboard();
                   PostProcessesManager.Process();
                }
                PostProcessesManager.End();
                ToolManager.TrigAfterRender();
            }

            _render_stopwatch.Stop();
        }

        private static void PostDrawStoryboard()
        {
            if (Instance.Updater.UpdatingStoryboardObjects.Count == 0)
                return;

            DrawStoryboardObjects(Instance.Updater.UpdatingStoryboardObjects);

            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        private static void DrawStoryboardObjects(List<StoryboardObject> draw_list)
        {
            SpriteInstanceGroup group = Instance.Resource.GetSprite(draw_list.First().ImageFilePath);

            bool additive_trigger;
            ChangeAdditiveStatus(draw_list.First().IsAdditive);

            foreach (var obj in draw_list)
            {
                if (!obj.IsVisible)
                    continue;
#if DEBUG
                if (!obj.DebugShow)
                    continue;//skip
#endif
                var obj_group = Instance.Resource.GetSprite(obj);

                if (group != obj_group || additive_trigger != obj.IsAdditive)
                {
                    group?.FlushDraw();

                    //应该是现在设置Blend否则Group自动渲染来不及钦定
                    ChangeAdditiveStatus(obj.IsAdditive);

                    group = obj_group;
                }

                group?.PostRenderCommand(obj.Postion, obj.Z, obj.Rotate, obj.Scale, obj.OriginOffset, obj.Color, obj.IsVerticalFlip, obj.IsHorizonFlip);
            }

            //ChangeAdditiveStatus(false);

            if (group?.CurrentPostCount != 0)
                group?.FlushDraw();

            void ChangeAdditiveStatus(bool is_additive_blend)
            {
                additive_trigger = is_additive_blend;
                GL.BlendFunc(BlendingFactor.SrcAlpha, additive_trigger ? BlendingFactor.One : BlendingFactor.OneMinusSrcAlpha);
            }
        }

        public static long RenderCostTime => _render_stopwatch.ElapsedMilliseconds;
    }
}
