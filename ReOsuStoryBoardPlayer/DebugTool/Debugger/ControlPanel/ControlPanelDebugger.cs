using ReOsuStoryBoardPlayer.Kernel;
using ReOsuStoryBoardPlayer.Player;
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
            window=new ControllerWindow(StoryboardInstanceManager.ActivityInstance);
            window.Show();
            window.progressBar1.Minimum=0;
            window.progressBar1.Maximum=100000;
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
