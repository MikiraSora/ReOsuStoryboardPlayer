using ReOsuStoryboardPlayer.Core.Utils;
using ReOsuStoryboardPlayer.Kernel;
using ReOsuStoryboardPlayer.Player;
using ReOsuStoryBoardPlayer.Graphics;
using ReOsuStoryBoardPlayer.Kernel;
using ReOsuStoryBoardPlayer.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ReOsuStoryboardPlayer.ProgramCommandParser;
using Path = System.IO.Path;
using OpenTK.Wpf;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ReOsuStoryboardPlayer.WPFControl
{
    /// <summary>
    /// StoryboardPlayer.xaml 的交互逻辑
    /// </summary>
    public partial class StoryboardPlayer : UserControl
    {
        private bool _isInit;
        private GLWpfControlSettings settings;

        public PlayerBase SourcePlayer { get; private set; }

        public event Action<PlayerBase, string> InitializePlayer;
        public event Action StoryboardUpdated;
        public event EventHandler<UnhandledExceptionEventArgs> ExceptionOccurred;

        public bool AutoUpdateViewSize
        {
            get => (bool)GetValue(AutoUpdateViewSizeProperty);
            set => SetValue(AutoUpdateViewSizeProperty, value);
        }

        public static readonly DependencyProperty AutoUpdateViewSizeProperty =
            DependencyProperty.Register("AutoUpdateViewSize", typeof(bool), typeof(StoryboardPlayer), new PropertyMetadata(false));

        public StoryboardPlayer()
        {
            InitializeComponent();

            settings = new GLWpfControlSettings();

            MyGLControl.SizeChanged += MyGLControl_SizeChanged;
            MyGLControl.Ready += MyGLControl_Ready;
            MyGLControl.Start(settings);
        }

        private void MyGLControl_Ready()
        {
            RenderKernel.Init();
        }

        public void SetPlayer(PlayerBase player)
        {
            SourcePlayer = player;
            MusicPlayerManager.ApplyPlayer(SourcePlayer);
        }

        public Task SwitchStoryboard(string path, bool playAfterLoad = false)
        {
            if (SourcePlayer == null) throw new Exception("SourcePlayer was not set");
            return ExecutorSync.PostTask(() =>
            {
                var fi = new FileInfo(path);
                var ext = fi.Extension;
                var folder = Path.GetDirectoryName(path);
                Parameters args = null;
                if (string.Equals(ext, ".osu", StringComparison.OrdinalIgnoreCase))
                {
                    var folderInfo = BeatmapFolderInfo.Parse(folder);
                    args = new Parameters();
                    args.Args.Add("diff", folderInfo.DifficultFiles.FirstOrDefault(k => k.Value == fi.FullName).Key);
                }

                var info = BeatmapFolderInfoEx.Parse(folder, args);
                var instance = StoryboardInstance.Load(info);

                LoadStoryboardInstance(instance);
                InitializePlayer?.Invoke(SourcePlayer, info.audio_file_path);

                Resize();

                if (playAfterLoad)
                {
                    MusicPlayerManager.ActivityPlayer?.Play();
                }
            });
        }

        private void MyGLControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_isInit || !AutoUpdateViewSize)
                return;

            ExecutorSync.PostTask(() => Resize());
        }

        private void MyGLControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isInit)
                return;

            Resize();
            MyGLControl.Render += MyGLControl_Render;

            _isInit = true;
        }

        private void LoadStoryboardInstance(StoryboardInstance instance)
        {
            RenderKernel.LoadStoryboardInstance(instance);
            UpdateKernel.LoadStoryboardInstance(instance);
        }

        private void Resize()
        {
            var width = (int)MyGLControl.ActualWidth;
            var height = (int)MyGLControl.ActualHeight;

            PlayerSetting.FrameHeight = height;

            if (UpdateKernel.Instance?.Info?.IsWidescreenStoryboard ?? false)
            {
                PlayerSetting.FrameWidth = (int)(RenderKernel.SB_WIDE_WIDTH / RenderKernel.SB_HEIGHT * height);
            }
            else
            {
                PlayerSetting.FrameWidth = (int)(RenderKernel.SB_WIDTH / RenderKernel.SB_HEIGHT * height);
            }

            PlayerSetting.FrameWidth = width;

            RenderKernel.ApplyWindowRenderSize(width, height);
        }

        private void MyGLControl_Render(TimeSpan obj)
        {
            RenderKernel.DefaultFrameBuffer = MyGLControl.Framebuffer;
            UpdateKernel.Update();
            RenderKernel.Draw();
            StoryboardUpdated?.Invoke();
        }
    }
}
