using ReOsuStoryboardPlayer.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryboardPlayer.Player
{
    public class LoopPlayer : PlayerBase
    {
        Stopwatch watch = new Stopwatch(); 

        public override float CurrentTime => GetCurrentTime();

        public override float Volume { get { return 0; } set { } }

        public long LoopStartTime { get; }
        public long LoopEndTime { get; }

        public override uint Length => (uint)Math.Abs(LoopStartTime-LoopEndTime);

        private bool playing = false;

        public float current_time { get; private set; }
        public long prev_t { get; private set; }

        public override bool IsPlaying => playing;

        public override float PlaybackSpeed { get; set; }

        public LoopPlayer(uint start_time,uint end_time)
        {
            LoopStartTime = start_time;
            LoopEndTime = end_time;

            current_time = LoopStartTime;
        }

        public override void Jump(float time, bool pause)
        {
            if (pause)
                Pause();

            var min = Math.Min(LoopEndTime, LoopStartTime);
            var max = Math.Max(LoopEndTime, LoopStartTime);

            current_time = Math.Min(max, Math.Max(min, time));
        }

        public override void Pause()
        {
            playing = false;
            watch.Stop();
        }

        public override void Play()
        {
            playing = true;
            watch.Restart();
        }

        public override void Stop()
        {
            Jump(LoopStartTime, true);
            watch.Stop();
        }

        private float GetCurrentTime()
        {
            if (!playing)
                return current_time;

            var now_t = watch.ElapsedMilliseconds;
            var pass_t = now_t - prev_t;
            prev_t = now_t;

            if (LoopStartTime<LoopEndTime)
            {
                current_time += pass_t;
                if (current_time>LoopEndTime)
                    while (current_time>LoopEndTime)
                        current_time -= Length;
            }
            else
            {
                current_time -= pass_t;
                if (current_time < LoopEndTime)
                    while (current_time < LoopEndTime)
                        current_time += Length;
            }

            return current_time;
        }

        public override string ToString() => $"loop {LoopStartTime}~{LoopEndTime}";
    }
}
