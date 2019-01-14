using ReOsuStoryBoardPlayer.Kernel;
using ReOsuStoryBoardPlayer.Player;
using System;
using System.Windows.Forms;

namespace ReOsuStoryBoardPlayer.DebugTool.Debugger.ControlPanel
{
    public partial class JumpToWindow : Form
    {
        private StoryBoardInstance instance;

        public JumpToWindow(StoryBoardInstance instance)
        {
            InitializeComponent();

            this.instance = instance;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
                return;

            uint jump;

            if (!uint.TryParse(textBox1.Text, out jump))
                return;

            jump = jump >=MusicPlayerManager.ActivityPlayer.Length ? MusicPlayerManager.ActivityPlayer.Length : jump;

            MusicPlayerManager.ActivityPlayer.Jump(jump,true);

            Close();
        }
    }
}