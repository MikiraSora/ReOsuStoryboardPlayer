using ReOsuStoryboardPlayer.Core.Utils;
using ReOsuStoryboardPlayer.Kernel;
using ReOsuStoryboardPlayer.Player;
using ReOsuStoryBoardPlayer.Graphics;
using ReOsuStoryBoardPlayer.Kernel;
using ReOsuStoryBoardPlayer.Parser;
using System;
using System.Collections.Generic;
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

namespace ReOsuStoryboardPlayer.WPFControl
{
    /// <summary>
    /// StoryboardPlayer.xaml 的交互逻辑
    /// </summary>
    public partial class StoryboardPlayer : UserControl
    {
        private bool inited;

        public event Action StoryboardUpdated;

        public bool IsPerformanceRendering
        {
            get { return (bool)GetValue(IsPerformanceRenderingProperty); }
            set { SetValue(IsPerformanceRenderingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsPerformanceRendering.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPerformanceRenderingProperty =
            DependencyProperty.Register("IsPerformanceRendering", typeof(bool), typeof(StoryboardPlayer), new PropertyMetadata(false));

        public StoryboardPlayer()
        {
            InitializeComponent();

            MyGLControl.SizeChanged += MyGLControl_SizeChanged;
            MyGLControl.ExceptionOccurred += MyGLControl_ExceptionOccurred;

            MyGLControl.PropertyChanged += (s,b) => Dispatcher.Invoke(() => IsPerformanceRendering = MyGLControl.IsUsingNVDXInterop);
        }

        private void MyGLControl_ExceptionOccurred(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Log.Error($"OpenTKControlBase throw errors : {ex.Message} \n {ex.StackTrace}");
            }
        }

        private void MyGLControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!inited)
                return;

            var width = (int)MyGLControl.ActualWidth;
            var height = (int)MyGLControl.ActualHeight;
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
            if (inited)
                return;

            MyGLControl.GlRender += MyGLControl_GlRender;

            MusicPlayer = new MusicPlayer();
            MusicPlayerManager.ApplyPlayer(MusicPlayer);

            inited = true;
        }

        public Task SwitchStoryboard(BeatmapFolderInfoEx info, bool play_after_load = false)
        {
            return ExecutorSync.PostTask(() =>
            {
                var instance = StoryboardInstance.Load(info);

                LoadStoryboardInstance(instance);
                InitAudio(info);

                Resize();

                if (play_after_load)
                {
                    MusicPlayerManager.ActivityPlayer?.Play();
                }
            });
        }

        private void LoadStoryboardInstance(StoryboardInstance instance)
        {
            RenderKernel.LoadStoryboardInstance(instance);
            UpdateKernel.LoadStoryboardInstance(instance);
        }

        private void InitAudio(BeatmapFolderInfoEx info)
        {
            MusicPlayer.Load(info.audio_file_path);
        }

        public MusicPlayer MusicPlayer { get; private set; }

        private void Resize()
        {
            var width = (int)this.ActualWidth;
            var height = (int)this.ActualHeight;
            
            PlayerSetting.FrameHeight = height;

            if (UpdateKernel.Instance?.Info?.IsWidescreenStoryboard??false)
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
