using ReOsuStoryboardPlayer.Kernel;
using ReOsuStoryboardPlayer.Parser;
using ReOsuStoryboardPlayer.Player;
using System;
using System.Windows.Forms;

namespace ReOsuStoryboardPlayer.Tools.DefaultTools.ControlPanel
{
    public partial class ControllerWindow : Form
    {
        private StoryboardInstance CurrentStoryboardIntance;

        public ControllerWindow(StoryboardInstance instance)
        {
            InitializeComponent();

            this.CurrentStoryboardIntance=instance;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MusicPlayerManager.ActivityPlayer.Play();
            UpdateTimeText();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MusicPlayerManager.ActivityPlayer.Pause();
            UpdateTimeText();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MusicPlayerManager.ActivityPlayer.Jump(0, true);
            UpdateTimeText();
        }

        private float prev_display_time = float.MinValue;

        private void UpdateProgressTime(float time)
        {
            progressBar1.Value=(int)Math.Max(0, Math.Min(time*1.0f/MusicPlayerManager.ActivityPlayer.Length*progressBar1.Maximum, progressBar1.Maximum));
            prev_display_time=time;
            UpdateTimeText();
        }

        private void UpdateTimeText()
        {
            prev_playback_display=MusicPlayerManager.ActivityPlayer.CurrentTime;
            label3.Text=$"Time:{MusicPlayerManager.ActivityPlayer.CurrentTime}/{MusicPlayerManager.ActivityPlayer.Length}";
        }

        private float prev_playback_display = float.MinValue, prev_playspeed_display = float.MinValue;
        private BeatmapFolderInfo prev_info_display = null;

        public void UpdateInfo()
        {
            var t = MusicPlayerManager.ActivityPlayer.CurrentTime;

            if (Math.Abs(t-prev_display_time)>=1000)
                UpdateProgressTime(t);

            if (prev_info_display!=StoryboardInstanceManager.ActivityInstance.Info)
            {
                prev_info_display=StoryboardInstanceManager.ActivityInstance.Info;
                label2.Text=prev_info_display.folder_path;
            }

            if (Math.Abs(prev_playback_display-MusicPlayerManager.ActivityPlayer.CurrentTime)>250)
            {
                UpdateTimeText();
            }

            if (prev_playspeed_display!=MusicPlayerManager.ActivityPlayer.PlaybackSpeed)
            {
                prev_playspeed_display=MusicPlayerManager.ActivityPlayer.PlaybackSpeed;
                label1.Text=prev_playspeed_display+"x";
            }

            button1.Enabled=!MusicPlayerManager.ActivityPlayer.IsPlaying;
            button2.Enabled=MusicPlayerManager.ActivityPlayer.IsPlaying;
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {
            var bar = sender as ProgressBar;
            var normalize_pos = ((e as MouseEventArgs).X /*- bar.Bounds.X*/ *1.0f)/bar.Bounds.Width;
            var jump_pos = (uint)(normalize_pos*MusicPlayerManager.ActivityPlayer.Length);

            MusicPlayerManager.ActivityPlayer.Jump(jump_pos, true);
            UpdateProgressTime(jump_pos);
        }

        private void ControllerWindow_Load(object sender, EventArgs e)
        {
        }

        private void ControllerWindow_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            MusicPlayerManager.ActivityPlayer.PlaybackSpeed-=0.125f;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            MusicPlayerManager.ActivityPlayer.PlaybackSpeed+=0.125f;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            JumpToWindow windows = new JumpToWindow(CurrentStoryboardIntance);
            windows.ShowDialog(this);
            UpdateTimeText();
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            MusicPlayerManager.ActivityPlayer.Volume=((TrackBar)sender).Value/100.0f;
        }
    }
}