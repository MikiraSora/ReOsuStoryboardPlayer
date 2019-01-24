using ReOsuStoryboardPlayer.Core.Utils;
using ReOsuStoryboardPlayer.DebugTool;
using ReOsuStoryboardPlayer.DebugTool.Debugger.CLIController;
using ReOsuStoryboardPlayer.DebugTool.Debugger.ControlPanel;
using ReOsuStoryboardPlayer.DebugTool.Debugger.InputController;
using ReOsuStoryboardPlayer.DebugTool.Debugger.ObjectInfoVisualizer;
using ReOsuStoryboardPlayer.DebugTool.Debugger.ObjectsSequenceViewer;
using ReOsuStoryboardPlayer.DebugTool.Debugger.TriggerConditionViewer;

namespace ReOsuStoryboardPlayer
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