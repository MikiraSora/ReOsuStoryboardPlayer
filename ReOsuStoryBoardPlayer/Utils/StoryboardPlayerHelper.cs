using ReOsuStoryBoardPlayer.Kernel;
using ReOsuStoryBoardPlayer.Parser;
using ReOsuStoryBoardPlayer.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Utils
{
    public static class StoryboardPlayerHelper
    {
        public static void PlayStoryboard(BeatmapFolderInfo info)
        {
            //init audio
            var player = new MusicPlayer();
            player.Load(info.audio_file_path);

            MusicPlayerManager.ActivityPlayer?.Stop();
            MusicPlayerManager.ApplyPlayer(player);

            //load storyboard objects
            var instance = new StoryBoardInstance(info);

            ExecutorSync.PostTask(()=>StoryboardWindow.CurrentWindow.LoadStoryboardInstance(instance)).Wait();

            MusicPlayerManager.ActivityPlayer?.Play();
        }
    }
}
