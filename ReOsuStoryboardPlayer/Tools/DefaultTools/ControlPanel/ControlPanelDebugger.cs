using ReOsuStoryboardPlayer.Kernel;

namespace ReOsuStoryboardPlayer.Tools.DefaultTools.ControlPanel
{
    public class ControlPanelDebugger : ToolBase
    {
        private ControllerWindow window;

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