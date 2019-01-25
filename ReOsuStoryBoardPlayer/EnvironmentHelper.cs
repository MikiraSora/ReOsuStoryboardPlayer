using ReOsuStoryboardPlayer.Core.Utils;
using ReOsuStoryboardPlayer.Tools;
using ReOsuStoryboardPlayer.Tools.DefaultTools.CLIController;
using ReOsuStoryboardPlayer.Tools.DefaultTools.ControlPanel;
using ReOsuStoryboardPlayer.Tools.DefaultTools.InputController;
using ReOsuStoryboardPlayer.Tools.DefaultTools.ObjectInfoVisualizer;
using ReOsuStoryboardPlayer.Tools.DefaultTools.ObjectsSequenceViewer;
using ReOsuStoryboardPlayer.Tools.DefaultTools.TriggerConditionViewer;

namespace ReOsuStoryboardPlayer
{
    public static class EnvironmentHelper
    {
        private static void SetupCommonEnvironment()
        {
            ToolManager.AddTool(new ControlPanelDebugger());
            ToolManager.AddTool(new InputControllerDebugger());
        }

        private static void SetupDebugEnvironment()
        {
            Log.AbleDebugLog=true;

            SetupCommonEnvironment();

            ToolManager.AddTool(new ObjectVisualizerDebugger());
            ToolManager.AddTool(new ObjectsSequenceViewerDebugger());
            ToolManager.AddTool(new TriggerConditionViewerDebugger());
        }

        private static void SetupReleaseEnvironment()
        {
            SetupCommonEnvironment();
        }

        private static void SetupMiniEnvironment()
        {
            ToolManager.AddTool(new CLIControllerDebugger());
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