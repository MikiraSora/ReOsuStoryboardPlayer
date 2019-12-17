using ReOsuStoryboardPlayer.Core.Utils;
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
using Microsoft.Win32;
using ReOsuStoryboardPlayer.Player;
using Path = System.IO.Path;

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
                var player = ((MainWindow)e).MyStoryboardPlayer.SourcePlayer;
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
                var player = ((MainWindow)e).MyStoryboardPlayer.SourcePlayer;
                var s = (double)o.NewValue;
                player.PlaybackSpeed = (float)s;
            }));

        #endregion

        public MainWindow()
        {
            Log.AbleDebugLog = true;

            InitializeComponent();

            DataContext = this;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            /*
            if (MyStoryboardPlayer.MusicPlayer == null)
                return;

            var val = e.NewValue;

            var time = MyStoryboardPlayer.MusicPlayer.Length * val / 100;

            MyStoryboardPlayer.MusicPlayer.Jump((float)time, true);
            */
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ControlPanelVisibility = Visibility.Hidden;
        }

        private void Grid_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            ControlPanelVisibility = Visibility.Visible;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
#if DEBUG
            //懒得翻
            //dialog.InitialDirectory = @"G:\SBTest";
#endif
            //dialog.InitialDirectory = System.IO.Path.GetFullPath(@".");
            dialog.Title = "选择一个SB文件";
            dialog.Filter = "支持的文件(*.osu;*.osb)|*.osu;*.osb|All files (*.*)|*.*";
            var result = dialog.ShowDialog();
            var path = dialog.FileName;

            if (result == true && File.Exists(path))
            {
                MyStoryboardPlayer.InitializePlayer += OnMyStoryboardPlayerOnInitializePlayer;
                await MyStoryboardPlayer.SwitchStoryboard(path, true);

                var current_time = MyStoryboardPlayer.SourcePlayer.Length;

                var span = TimeSpan.FromMilliseconds(current_time);

                //Dispatcher.Invoke(() =>
                //{
                CurrentPlayingText = Path.GetDirectoryName(path);
                PlaybackLength = $"{span.TotalMinutes:F0}:{span.Seconds:F0}";
                //});
            }
        }

        private void OnMyStoryboardPlayerOnInitializePlayer(PlayerBase player, string audioPath)
        {
            ((MusicPlayer)player).Load(audioPath);
            MyStoryboardPlayer.InitializePlayer -= OnMyStoryboardPlayerOnInitializePlayer;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (MyStoryboardPlayer.SourcePlayer.IsPlaying)
                MyStoryboardPlayer.SourcePlayer.Pause();
            else
                MyStoryboardPlayer.SourcePlayer.Play();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MyStoryboardPlayer.SourcePlayer.Stop();
        }

        private void MyStoryboardPlayer_StoryboardUpdated()
        {
            if (UpdateKernel.Instance == null || RenderKernel.Instance == null)
                return;

            var current_time = MyStoryboardPlayer.SourcePlayer.CurrentTime;

            var span = TimeSpan.FromMilliseconds(current_time);

            Dispatcher.Invoke(() =>
            {
                CurrentPlayPosition = $"{span.TotalMinutes:F0}:{span.Seconds:F0}";
                PlayProgress.Value = current_time / MyStoryboardPlayer.SourcePlayer.Length * 100;
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MyStoryboardPlayer.SetPlayer(new MusicPlayer());
        }
    }
}
