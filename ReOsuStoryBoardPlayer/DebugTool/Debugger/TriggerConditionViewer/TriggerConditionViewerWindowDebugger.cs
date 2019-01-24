using ReOsuStoryBoardPlayer.Kernel;

namespace ReOsuStoryBoardPlayer.DebugTool.Debugger.TriggerConditionViewer
{
    internal class TriggerConditionViewerDebugger : DebuggerBase
    {
        public TriggerConditionViewerWindow Window { get; private set; }
        private StoryboardInstance instance;

        public override void Init()
        {
            Window=new TriggerConditionViewerWindow();
        }

        public override void Term()
        {
            Window.Close();
        }

        public override void Update()
        {
            var i = StoryboardInstanceManager.ActivityInstance;

            if (i!=instance)
                Window?.Reset();

            instance=i;
        }
    }
}