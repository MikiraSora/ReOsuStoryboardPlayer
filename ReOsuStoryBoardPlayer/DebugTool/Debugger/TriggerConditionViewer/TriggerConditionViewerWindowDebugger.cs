using ReOsuStoryBoardPlayer.Core.Kernel;
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
        public TriggerConditionViewerWindow Window { get; private set; }
        StoryboardInstance instance;

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
