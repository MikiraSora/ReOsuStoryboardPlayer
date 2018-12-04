using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Player
{
    public static class MusicPlayerManager
    {
        public static PlayerBase ActivityPlayer { get; private set; }

        public static void ApplyPlayer(PlayerBase player)
        {
            if (ActivityPlayer!=null)
            {
                ActivityPlayer.Pause();
            }

            ActivityPlayer=player;
        }
    }
}
