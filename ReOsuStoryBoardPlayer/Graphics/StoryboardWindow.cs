using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using ReOsuStoryBoardPlayer.Base;
using ReOsuStoryBoardPlayer.Commands;
using ReOsuStoryBoardPlayer.DebugTool;
using ReOsuStoryBoardPlayer.Graphics;
using ReOsuStoryBoardPlayer.Player;
using ReOsuStoryBoardPlayer.Utils;
using System;
using System.Collections.Generic;
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

        public void InitWindowRenderSize()
        {
            CameraViewMatrix = Matrix4.Identity;

            //裁剪View
            float radio = (float)Width / (float)Height;

            ProjectionMatrix = Matrix4.Identity * Matrix4.CreateOrthographic(SB_HEIGHT * radio, SB_HEIGHT, 0, 100);
            CameraViewMatrix = CameraViewMatrix;
        }
        
        internal void BuildCacheDrawSpriteBatch(IEnumerable<StoryBoardObject> StoryboardObjectList,string folder_path)
        {
            List<string> pic_list = new List<string>();
            pic_list.AddRange(Directory.EnumerateFiles(folder_path, "*.png", SearchOption.AllDirectories));
            pic_list.AddRange(Directory.EnumerateFiles(folder_path, "*.jpg", SearchOption.AllDirectories));

            Dictionary<string, SpriteInstanceGroup> CacheDrawSpriteInstanceMap = new Dictionary<string, SpriteInstanceGroup>();

            pic_list.ForEach((path) =>
            {
                Texture tex = new Texture(path);
                string absolute_path = path.Replace(folder_path, string.Empty).Trim().TrimStart('/','\\');

                CacheDrawSpriteInstanceMap[absolute_path.ToLower()]=new SpriteInstanceGroup(DrawCallInstanceCountMax, absolute_path, tex);

                Log.Debug($"Created storyboard sprite instance from image file :{path}");
            });


            foreach (var obj in StoryboardObjectList)
            {
                switch (obj)
                {
                    case StoryboardBackgroundObject background:
                        if (!CacheDrawSpriteInstanceMap.TryGetValue(obj.ImageFilePath.ToLower(), out obj.RenderGroup))
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
                            if (!CacheDrawSpriteInstanceMap.TryGetValue(path, out group))
                            {
                                Log.Warn($"not found image:{path}");
                                continue;
                            }
                            list.Add(group);
                        }

                        animation.backup_group=list.ToArray();
                        break;

                    default:
                        if (!CacheDrawSpriteInstanceMap.TryGetValue(obj.ImageFilePath.ToLower(), out obj.RenderGroup))
                            Log.Warn($"not found image:{obj.ImageFilePath}");
                        break;
                }
            }
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

            InitWindowRenderSize();

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

            foreach (var obj in instance?.StoryboardObjectList)
                obj.RenderGroup=null;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.ClearColor(Color.Black);

            if (ready)
                PostDrawStoryBoard();

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);


            if (!ready)
                return;

            var current_time = MusicPlayerManager.ActivityPlayer.CurrentTime;

            instance.Update(current_time);

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