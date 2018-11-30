using System;
using System.Windows.Forms;

namespace ReOsuStoryBoardPlayer.DebugController
{
    public partial class CreateBreakpointDialog : Form
    {
        private StoryBoardInstance CurrentStoryboardInstance;

        public CreateBreakpointDialog(StoryBoardInstance instance)
        {
            InitializeComponent();

            this.CurrentStoryboardInstance = instance;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Please input content.");
                return;
            }

#if DEBUG
            CurrentStoryboardInstance.DebugToolInstance.CreateBreakpointInCommandExecuting(textBox1.Text, (Event)(Enum.Parse(typeof(Event), textBox2.Text)));
            Close();
#endif
        }
    }
}