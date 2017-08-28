using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReOsuStoryBoardPlayer.DebugController
{
    public partial class ControllerWindow : Form
    {
        StoryBoardInstance CurrentStoryboardIntance;

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

        private void button4_Click(object sender, EventArgs e)
        {

#if DEBUG
            CurrentStoryboardIntance.DumpCurrentStoryboardStatus();
            
#endif
        }

        private void button7_Click(object sender, EventArgs e)
        {
            CreateBreakpointDialog dialog = new CreateBreakpointDialog(CurrentStoryboardIntance);
            dialog.ShowDialog(this);
        }

        private void button8_Click(object sender, EventArgs e)
        {
#if DEBUG
            CurrentStoryboardIntance.ClearBreakpoint();
            MessageBox.Show("Clear braekpoints!");
            
#endif
        }

        public void UpdateInfo()
        {
            label2.Text = CurrentStoryboardIntance.folder_path;
            progressBar1.Value = (int)CurrentStoryboardIntance.player.CurrentPlayback;
            label1.Text = CurrentStoryboardIntance.player.PlaybackSpeed+"x";
            label3.Text = $"Time:{CurrentStoryboardIntance.player.CurrentPlayback}({CurrentStoryboardIntance.player.FixCurrentPlayback})/{CurrentStoryboardIntance.player.Length}";
            label5.Text = $"{CurrentStoryboardIntance.RenderCastTime + CurrentStoryboardIntance.UpdateCastTime}\t:{CurrentStoryboardIntance.UpdateCastTime}\t:{CurrentStoryboardIntance.RenderCastTime}";

            button1.Enabled = !CurrentStoryboardIntance.player.IsPlaying;
            button2.Enabled = CurrentStoryboardIntance.player.IsPlaying;
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

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
            CurrentStoryboardIntance.player.PlaybackSpeed -= 0.25f;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            CurrentStoryboardIntance.player.PlaybackSpeed += 0.25f;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            JumpToWindow windows = new JumpToWindow(CurrentStoryboardIntance);
            windows.ShowDialog(this);
        }
    }
}
