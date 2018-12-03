using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.DebugTool.Debugger.ControlPanel
{
    public class ControlPanelDebugger : DebuggerBase
    {
        ControllerWindow window;

        public ControlPanelDebugger()
        {
            Priority=UpdatePriority.EveryFrame;
        }

        public override void Init()
        {
            window=new ControllerWindow(StoryBoardInstance.Instance);
            window.Show();
            window.progressBar1.Maximum=(int)StoryBoardInstance.Instance.player.Length;
        }

        public override void Term()
        {
            window.Close();
        }

        public override void Update()
        {
            window.UpdateInfo();
        }
    }
}
