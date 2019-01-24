using ReOsuStoryBoardPlayer.DebugTool;
using ReOsuStoryBoardPlayer.DebugTool.Debugger.AutoTriggerContoller;
using ReOsuStoryBoardPlayer.Kernel;
using ReOsuStoryBoardPlayer.Core.Parser;
using ReOsuStoryBoardPlayer.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReOsuStoryBoardPlayer.Parser;
using ReOsuStoryBoardPlayer.Core.Kernel;

namespace ReOsuStoryBoardPlayer.Utils
{
    public static class StoryboardPlayerHelper
    {
        public static void PlayStoryboard(BeatmapFolderInfo info)
        {
            //init audio
            if (!(MusicPlayerManager.ActivityPlayer is MusicPlayer player))
                throw new Exception("Player must be MusicPlayer if you want to call PlayStoryboard()");

            player.Load(info.audio_file_path);

            MusicPlayerManager.ActivityPlayer?.Stop();
            MusicPlayerManager.ApplyPlayer(player);

            //load storyboard objects
            var instance = StoryboardInstance.Load(info);

            var auto_trigger=DebuggerManager.GetOrCreateDebugger<AutoTrigger>();
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
