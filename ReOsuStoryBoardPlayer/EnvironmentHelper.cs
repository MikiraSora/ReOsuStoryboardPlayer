using ReOsuStoryBoardPlayer.Core.Utils;
using ReOsuStoryBoardPlayer.DebugTool;
using ReOsuStoryBoardPlayer.DebugTool.Debugger.CLIController;
using ReOsuStoryBoardPlayer.DebugTool.Debugger.ControlPanel;
using ReOsuStoryBoardPlayer.DebugTool.Debugger.InputController;
using ReOsuStoryBoardPlayer.DebugTool.Debugger.ObjectInfoVisualizer;
using ReOsuStoryBoardPlayer.DebugTool.Debugger.ObjectsSequenceViewer;
using ReOsuStoryBoardPlayer.DebugTool.Debugger.TriggerConditionViewer;

namespace ReOsuStoryBoardPlayer
{
    public static class EnvironmentHelper
    {
        private static void SetupCommonEnvironment()
        {
            DebuggerManager.AddDebugger(new ControlPanelDebugger());
            DebuggerManager.AddDebugger(new InputControllerDebugger());
        }

        private static void SetupDebugEnvironment()
        {
            Log.AbleDebugLog=true;

            SetupCommonEnvironment();

            DebuggerManager.AddDebugger(new ObjectVisualizerDebugger());
            DebuggerManager.AddDebugger(new ObjectsSequenceViewerDebugger());
            DebuggerManager.AddDebugger(new TriggerConditionViewerDebugger());
        }

        private static void SetupReleaseEnvironment()
        {
            SetupCommonEnvironment();
        }

        private static void SetupMiniEnvironment()
        {
            DebuggerManager.AddDebugger(new CLIControllerDebugger());
        }

        public static void SetupEnvironment()
        {
            if (PlayerSetting.MiniMode)
            {
                SetupMiniEnvironment();
            }
            else
            {
                if (Setting.DebugMode)
                    SetupDebugEnvironment();
                else
                    SetupReleaseEnvironment();
            }
        }
    }
}