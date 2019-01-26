namespace ReOsuStoryboardPlayer.Player
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