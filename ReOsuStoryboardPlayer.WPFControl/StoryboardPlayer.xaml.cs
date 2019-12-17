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

namespace ReOsuStoryboardPlayer.WPFControl
{
    /// <summary>
    /// StoryboardPlayer.xaml 的交互逻辑
    /// </summary>
    public partial class StoryboardPlayer : UserControl
    {
        private bool _isInit;

        public PlayerBase SourcePlayer { get; private set; }

        public event Action<PlayerBase, string> InitializePlayer;
        public event Action StoryboardUpdated;
        public event EventHandler<UnhandledExceptionEventArgs> ExceptionOccurred;

        public bool IsPerformanceRendering
        {
            get => (bool)GetValue(IsPerformanceRenderingProperty);
            set => SetValue(IsPerformanceRenderingProperty, value);
        }

        public bool AutoUpdateViewSize
        {
            get => (bool)GetValue(AutoUpdateViewSizeProperty);
            set => SetValue(AutoUpdateViewSizeProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsPerformanceRendering.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPerformanceRenderingProperty =
            DependencyProperty.Register("IsPerformanceRendering", typeof(bool), typeof(StoryboardPlayer),
                new PropertyMetadata(false, Callback));

        private static void Callback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StoryboardPlayer sbp)
            {
                if (sbp.MyGLControl.IsUsingNVDXInterop != sbp.IsPerformanceRendering)
                    sbp.MyGLControl.IsUsingNVDXInterop = sbp.IsPerformanceRendering;
                if (sbp.MyGLControl.PerferPerfomance != sbp.IsPerformanceRendering)
                    sbp.MyGLControl.PerferPerfomance = sbp.IsPerformanceRendering;
            }
        }

        public static readonly DependencyProperty AutoUpdateViewSizeProperty =
            DependencyProperty.Register("AutoUpdateViewSize", typeof(bool), typeof(StoryboardPlayer), new PropertyMetadata(false));

        public StoryboardPlayer()
        {
            InitializeComponent();

            MyGLControl.SizeChanged += MyGLControl_SizeChanged;
            MyGLControl.ExceptionOccurred += (sender, args) => ExceptionOccurred?.Invoke(sender, args);
            MyGLControl.PropertyChanged += (s, b) => Dispatcher?.Invoke(() => IsPerformanceRendering = MyGLControl.IsUsingNVDXInterop);
            MyGLControl.IsUsingNVDXInterop = IsPerformanceRendering;
            MyGLControl.PerferPerfomance = IsPerformanceRendering;
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

        private void MyGLControl_GlRender(object sender, OpenTkControl.OpenTkControlBase.GlRenderEventArgs e)
        {
            if (e.NewContext)
            {
                RenderKernel.Init();
                Resize();
            }

            if (MyGLControl.IsUsingNVDXInterop)
                RenderKernel.DefaultFrameBuffer = MyGLControl.NVDXInteropFramebuffer;

            UpdateKernel.Update();
            RenderKernel.Draw();
            StoryboardUpdated?.Invoke();

            //UpdateKernel.FrameRateLimit();
        }

        private void MyGLControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isInit)
                return;

            MyGLControl.GlRender += MyGLControl_GlRender;

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
    }
}
