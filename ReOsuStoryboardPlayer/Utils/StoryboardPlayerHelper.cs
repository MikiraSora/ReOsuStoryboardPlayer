using ReOsuStoryboardPlayer.Tools;
using ReOsuStoryboardPlayer.Tools.DefaultTools.AutoTriggerContoller;
using ReOsuStoryboardPlayer.Kernel;
using ReOsuStoryboardPlayer.Parser;
using ReOsuStoryboardPlayer.Player;
using System;
using ReOsuStoryBoardPlayer.Parser;

namespace ReOsuStoryboardPlayer.Utils
{
    public static class StoryboardPlayerHelper
    {
        public static void PlayStoryboard(BeatmapFolderInfoEx info)
        {
            //init audio
            if (!(MusicPlayerManager.ActivityPlayer is MusicPlayer player))
                throw new Exception("Player must be MusicPlayer if you want to call PlayStoryboard()");

            player.Load(info.audio_file_path);

            MusicPlayerManager.ActivityPlayer?.Stop();
            MusicPlayerManager.ApplyPlayer(player);

            //load Storyboard objects
            var instance = StoryboardInstance.Load(info);

            var auto_trigger = ToolManager.GetOrCreateTool<AutoTrigger>();
            auto_trigger.Load(info);
            auto_trigger.Trim();

            ExecutorSync.PostTask(() =>
            {
                StoryboardWindow.CurrentWindow.LoadStoryboardInstance(instance);
                MusicPlayerManager.ActivityPlayer?.Play();
            }
            );
        }
    }
}