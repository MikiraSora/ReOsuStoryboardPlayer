using ReOsuStoryBoardPlayer.DebugTool.Debugger.CLIController;
using ReOsuStoryBoardPlayer.DebugTool.Debugger.ControlPanel;
using ReOsuStoryBoardPlayer.DebugTool.Debugger.InputController;
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
        private static void SetupCommonEnvironment()
        {
            DebuggerManager.AddDebugger(new ControlPanelDebugger());
            DebuggerManager.AddDebugger(new InputControllerDebugger());
        }

        public static void SetupDebugEnvironment()
        {
            Log.AbleDebugLog=true;

            SetupCommonEnvironment();

            DebuggerManager.AddDebugger(new ObjectVisualizerDebugger());
            DebuggerManager.AddDebugger(new ObjectsSequenceViewerDebugger());
            DebuggerManager.AddDebugger(new CLIControllerDebugger());
        }

        public static void SetupReleaseEnvironment()
        {
            SetupCommonEnvironment();
        }
    }
}
