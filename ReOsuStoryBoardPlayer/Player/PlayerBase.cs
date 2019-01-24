namespace ReOsuStoryBoardPlayer.Player
{
    public abstract class PlayerBase
    {
        public abstract float CurrentTime { get; }
        public abstract float Volume { get; set; }
        public abstract uint Length { get; }
        public abstract bool IsPlaying { get; }
        public abstract float PlaybackSpeed { get; set; }

        public abstract void Play();

        public abstract void Stop();

        public abstract void Pause();

        public abstract void Jump(float time, bool pause);
    }
}