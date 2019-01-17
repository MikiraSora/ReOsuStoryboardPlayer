using ReOsuStoryBoardPlayer.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.DebugTool.Debugger.TriggerConditionViewer
{
    class TriggerConditionViewerDebugger : DebuggerBase
    {
        TriggerConditionViewerWindow window;
        StoryBoardInstance instance;

        public override void Init()
        {
            window=new TriggerConditionViewerWindow();
            window.Show();
        }

        public override void Term()
        {
            window.Close();
        }

        public override void Update()
        {
            var i = StoryboardInstanceManager.ActivityInstance;

            if (i!=instance)
                window?.Reset();

            instance=i;
        }
    }
}
