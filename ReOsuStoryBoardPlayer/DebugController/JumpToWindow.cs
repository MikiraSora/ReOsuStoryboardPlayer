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
    public partial class JumpToWindow : Form
    {
        StoryBoardInstance instance;

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

            jump = jump >= instance.player.Length ? instance.player.Length : jump;

            instance.player.Jump(jump);
            instance.player.Pause();

            Close();
        }
    }
}
