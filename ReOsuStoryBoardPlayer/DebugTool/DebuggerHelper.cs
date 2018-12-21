using ReOsuStoryBoardPlayer.DebugTool.Debugger.CLIController;
using ReOsuStoryBoardPlayer.DebugTool.Debugger.ControlPanel;
using ReOsuStoryBoardPlayer.DebugTool.Debugger.ObjectInfoVisualizer;
using ReOsuStoryBoardPlayer.DebugTool.Debugger.ObjectsSequenceViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.DebugTool
{
    public static class DebuggerHelper
    {
        public static void SetupDebugEnvironment()
        {
            DebuggerManager.AddDebugger(new ControlPanelDebugger());
            DebuggerManager.AddDebugger(new ObjectVisualizerDebugger());
            DebuggerManager.AddDebugger(new ObjectsSequenceViewerDebugger());
            DebuggerManager.AddDebugger(new CLIControllerDebugger());

            Log.AbleDebugLog=true;
        }

        public static void SetupReleaseEnvironment()
        {
            //提供个控制器
            DebuggerManager.AddDebugger(new ControlPanelDebugger());
        }
    }
}
