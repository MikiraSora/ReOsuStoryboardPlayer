using ReOsuStoryboardPlayer.Kernel;
using ReOsuStoryboardPlayer.Player;
using System;
using System.Windows.Forms;

namespace ReOsuStoryboardPlayer.DebugTool.Debugger.ControlPanel
{
    public partial class JumpToWindow : Form
    {
        private StoryboardInstance instance;

        public JumpToWindow(StoryboardInstance instance)
        {
            InitializeComponent();

            this.instance=instance;
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

            jump=jump>=MusicPlayerManager.ActivityPlayer.Length ? MusicPlayerManager.ActivityPlayer.Length : jump;

            MusicPlayerManager.ActivityPlayer.Jump(jump, true);

            Close();
        }
    }
}