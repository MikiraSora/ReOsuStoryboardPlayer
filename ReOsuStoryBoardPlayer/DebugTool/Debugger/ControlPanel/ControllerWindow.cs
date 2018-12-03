using ReOsuStoryBoardPlayer.DebugTool;
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
            CurrentStoryboardIntance.player.Play();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CurrentStoryboardIntance.player.Pause();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            CurrentStoryboardIntance.player.Jump(0);
        }

        public void UpdateInfo()
        {
            label2.Text = CurrentStoryboardIntance.folder_path;
            progressBar1.Value = (int)CurrentStoryboardIntance.player.CurrentPlayback;
            label1.Text = CurrentStoryboardIntance.player.PlaybackSpeed + "x";
            label3.Text = $"Time:{CurrentStoryboardIntance.player.CurrentPlayback}({CurrentStoryboardIntance.player.CurrentFixedTime})/{CurrentStoryboardIntance.player.Length}";
            label5.Text = $"{CurrentStoryboardIntance.RenderCastTime + CurrentStoryboardIntance.UpdateCastTime}\t:{CurrentStoryboardIntance.UpdateCastTime}\t:{CurrentStoryboardIntance.RenderCastTime}";

            button1.Enabled = !CurrentStoryboardIntance.player.IsPlaying;
            button2.Enabled = CurrentStoryboardIntance.player.IsPlaying;
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {
            var bar = sender as ProgressBar;
            var normalize_pos = ((e as MouseEventArgs).X /*- bar.Bounds.X*/ * 1.0f) / bar.Bounds.Width;
            var jump_pos = (uint)(normalize_pos * bar.Maximum);

            CurrentStoryboardIntance.player.Jump(jump_pos);
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
            CurrentStoryboardIntance.player.PlaybackSpeed -= 0.125f;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            CurrentStoryboardIntance.player.PlaybackSpeed += 0.125f;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            JumpToWindow windows = new JumpToWindow(CurrentStoryboardIntance);
            windows.ShowDialog(this);
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            CurrentStoryboardIntance.player.Volume = ((TrackBar)sender).Value / 100.0f;
        }
    }
}