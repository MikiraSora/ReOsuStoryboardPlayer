using ReOsuStoryBoardPlayer.DebugTool;
using ReOsuStoryBoardPlayer.Kernel;
using ReOsuStoryBoardPlayer.Player;
using System;
using System.Windows.Forms;

namespace ReOsuStoryBoardPlayer.DebugTool.Debugger.ControlPanel
{
    public partial class ControllerWindow : Form
    {
        private StoryBoardInstance CurrentStoryboardIntance;

        public ControllerWindow(StoryBoardInstance instance)
        {
            InitializeComponent();

            this.CurrentStoryboardIntance = instance;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MusicPlayerManager.ActivityPlayer.Play();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MusicPlayerManager.ActivityPlayer.Pause();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MusicPlayerManager.ActivityPlayer.Jump(0);
        }

        private float prev_display_time= float.MinValue;

        private void UpdateProgressTime(float time)
        {
            progressBar1.Value=(int)Math.Max(0, Math.Min(time*1.0f/MusicPlayerManager.ActivityPlayer.Length*progressBar1.Maximum, progressBar1.Maximum));
            prev_display_time=time;
        }

        public void UpdateInfo()
        {
            var t = MusicPlayerManager.ActivityPlayer.CurrentTime;

            if (Math.Abs(t-prev_display_time)>=1000)
                UpdateProgressTime(t);

            label2.Text = StoryboardInstanceManager.ActivityInstance.Info.folder_path;

            label1.Text = MusicPlayerManager.ActivityPlayer.PlaybackSpeed + "x";
            label3.Text = $"Time:{MusicPlayerManager.ActivityPlayer.CurrentTime}/{MusicPlayerManager.ActivityPlayer.Length}";
            
            button1.Enabled = !MusicPlayerManager.ActivityPlayer.IsPlaying;
            button2.Enabled = MusicPlayerManager.ActivityPlayer.IsPlaying;
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {
            var bar = sender as ProgressBar;
            var normalize_pos = ((e as MouseEventArgs).X /*- bar.Bounds.X*/ * 1.0f) / bar.Bounds.Width;
            var jump_pos = (uint)(normalize_pos *MusicPlayerManager.ActivityPlayer.Length);

            MusicPlayerManager.ActivityPlayer.Jump(jump_pos);
            UpdateProgressTime(jump_pos);
        }

        private void ControllerWindow_Load(object sender, EventArgs e)
        {
        }

        private void ControllerWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            MusicPlayerManager.ActivityPlayer.PlaybackSpeed -= 0.125f;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            MusicPlayerManager.ActivityPlayer.PlaybackSpeed += 0.125f;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            JumpToWindow windows = new JumpToWindow(CurrentStoryboardIntance);
            windows.ShowDialog(this);
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            MusicPlayerManager.ActivityPlayer.Volume = ((TrackBar)sender).Value / 100.0f;
        }
    }
}