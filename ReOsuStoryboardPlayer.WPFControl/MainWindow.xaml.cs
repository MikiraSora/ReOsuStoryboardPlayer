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
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PlayerSetting.Init();

            var info = BeatmapFolderInfoEx.Parse(@"G:\SBTest\183467 Marika - quantum jump", null);
            var instance = StoryboardInstance.Load(info);

            LoadStoryboardInstance(instance);
            InitAudio(info);

            MusicPlayerManager.ActivityPlayer?.Play();
        }

        private void InitAudio(BeatmapFolderInfoEx info)
        {
            var player = new MusicPlayer();
            player.Load(info.audio_file_path);
            MusicPlayerManager.ApplyPlayer(player); 
            MusicPlayerManager.ActivityPlayer.Volume = PlayerSetting.Volume;
        }

        private void LoadStoryboardInstance(StoryboardInstance instance)
        {
            RenderKernel.LoadStoryboardInstance(instance);
            UpdateKernel.LoadStoryboardInstance(instance);
            RenderKernel.ApplyWindowRenderSize((int)MyGLControl.Width, (int)MyGLControl.Height);
        }

        private void UiOpenTkControl_GlRender(object sender, OpenTkControl.OpenTkControlBase.GlRenderEventArgs e)
        {
            UpdateKernel.Update();
            RenderKernel.Draw();
        }

        private void UiOpenTkControl_Loaded(object sender, RoutedEventArgs e)
        {
            RenderKernel.Init();
        }
    }
}
