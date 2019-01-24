using ReOsuStoryboardPlayer.DebugTool;
using ReOsuStoryboardPlayer.OutputEncoding.Kernel;
using ReOsuStoryboardPlayer.Player;
using System;

namespace ReOsuStoryboardPlayer.OutputEncoding.Player
{
    public class EncodingProcessPlayer : PlayerBase
    {
        public override float Volume { get; set; }

        public override uint Length => length;

        public override bool IsPlaying => is_playing;

        public override float PlaybackSpeed { get => 1; set { } }

        private bool is_playing = false;

        public override void Jump(float time, bool pause)
        {
            current_time=Math.Min(time, Length);
        }

        public override void Pause()
        {
            is_playing=false;
        }

        public override void Play()
        {
            is_playing=true;
        }

        public override void Stop()
        {
            DebuggerManager.GetDebugger<EncodingKernel>().Abort();
        }

        private float time_step;
        private float current_time;
        private uint length;

        public EncodingProcessPlayer(uint length, int fps)
        {
            time_step=1.0f/fps*1000;
            current_time=0-time_step;
            this.length=length;
        }

        public float GetNextFrameTime() => current_time+=is_playing ? time_step : 0;

        public override float CurrentTime => current_time;
    }
}