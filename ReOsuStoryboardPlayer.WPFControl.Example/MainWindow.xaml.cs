using ReOsuStoryBoardPlayer.Graphics;
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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ReOsuStoryboardPlayer.WPFControl.Example
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region DP

        public Visibility ControlPanelVisibility
        {
            get { return (Visibility)GetValue(ControlPanelVisibilityProperty); }
            set { SetValue(ControlPanelVisibilityProperty, value); }
        }

        public static readonly DependencyProperty ControlPanelVisibilityProperty =
            DependencyProperty.Register("ControlPanelVisibility", typeof(Visibility), typeof(MainWindow), new PropertyMetadata(Visibility.Visible));

        public string CurrentPlayingText
        {
            get { return (string)GetValue(CurrentPlayingTextProperty); }
            set { SetValue(CurrentPlayingTextProperty, value); }
        }

        public static readonly DependencyProperty CurrentPlayingTextProperty =
            DependencyProperty.Register("CurrentPlayingText", typeof(string), typeof(MainWindow), new PropertyMetadata("Unknown"));

        public string CurrentPlayPosition
        {
            get { return (string)GetValue(CurrentPlayPositionProperty); }
            set { SetValue(CurrentPlayPositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentPlayPosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentPlayPositionProperty =
            DependencyProperty.Register("CurrentPlayPosition", typeof(string), typeof(MainWindow), new PropertyMetadata("0"));

        public string PlaybackLength
        {
            get { return (string)GetValue(PlaybackLengthProperty); }
            set { SetValue(PlaybackLengthProperty, value); }
        }

        public static readonly DependencyProperty PlaybackLengthProperty =
            DependencyProperty.Register("PlaybackLength", typeof(string), typeof(MainWindow), new PropertyMetadata("0"));

        public double Volume
        {
            get { return (double)GetValue(VolumeProperty); }
            set { SetValue(VolumeProperty, value); }
        }

        public static readonly DependencyProperty VolumeProperty =
            DependencyProperty.Register("Volume", typeof(double), typeof(MainWindow), new PropertyMetadata(1.0d, (e, o) =>
            {
                var player = ((MainWindow)e).MyStoryboardPlayer.MusicPlayer;
                var s = (double)o.NewValue;
                player.Volume = (float)s;
            }));

        public double PlaybackSpeed
        {
            get { return (double)GetValue(PlaybackSpeedProperty); }
            set { SetValue(PlaybackSpeedProperty, value); }
        }

        public static readonly DependencyProperty PlaybackSpeedProperty =
            DependencyProperty.Register("PlaybackSpeed", typeof(double), typeof(MainWindow), new PropertyMetadata(1.0d, (e, o) =>
            {
                var player = ((MainWindow)e).MyStoryboardPlayer.MusicPlayer;
                var s = (double)o.NewValue;
                player.PlaybackSpeed = (float)s;
            }));

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var val = e.NewValue;

            //todo
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ControlPanelVisibility = Visibility.Hidden;
        }

        private void Grid_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            ControlPanelVisibility = Visibility.Visible;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.ShowNewFolderButton = false;
                dialog.Description = "选择一个含有SB的文件夹";

                var result = dialog.ShowDialog();
                var path = dialog.SelectedPath;

                if ((result == System.Windows.Forms.DialogResult.OK || result == System.Windows.Forms.DialogResult.Yes) && Directory.Exists(path))
                {
                    var info = BeatmapFolderInfoEx.Parse(path, null);

                    RenderKernel.ApplyWindowRenderSize((int)MyStoryboardPlayer.ActualWidth, (int)MyStoryboardPlayer.ActualHeight);

                    CurrentPlayingText = $"{info.folder_path}";

                    MyStoryboardPlayer.SwitchStoryboard(info,true);

                    var current_time = MyStoryboardPlayer.MusicPlayer.Length;

                    var span = TimeSpan.FromMilliseconds(current_time);

                    PlaybackLength = $"{span.TotalMinutes:F0}:{span.Seconds:F0}";
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (MyStoryboardPlayer.MusicPlayer.IsPlaying)
                MyStoryboardPlayer.MusicPlayer.Pause();
            else
                MyStoryboardPlayer.MusicPlayer.Play();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MyStoryboardPlayer.MusicPlayer.Stop();
        }

        private void MyStoryboardPlayer_StoryboardUpdated()
        {
            var current_time = MyStoryboardPlayer.MusicPlayer.CurrentTime;

            var span = TimeSpan.FromMilliseconds(current_time);

            CurrentPlayPosition = $"{span.TotalMinutes:F0}:{span.Seconds:F0}";
        }
    }
}
