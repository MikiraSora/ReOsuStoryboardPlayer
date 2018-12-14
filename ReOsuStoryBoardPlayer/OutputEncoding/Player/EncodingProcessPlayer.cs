using ReOsuStoryBoardPlayer.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.OutputEncoding.Player
{
    public class EncodingProcessPlayer : PlayerBase
    {
        #region NOT SUPPORT

        public override float Volume { get; set; }

        public override uint Length => length;

        public override bool IsPlaying => CurrentTime<=Length;

        public override float PlaybackSpeed { get => 1; set { } }

        public override void Jump(float time)
        {

        }

        public override void Pause()
        {

        }

        public override void Play()
        {

        }

        public override void Stop()
        {

        }

        #endregion

        float time_step;
        float current_time;
        uint length;

        public EncodingProcessPlayer(uint length,int fps)
        {
            time_step=1.0f/fps*1000;
            current_time=0-time_step;
            this.length=length;
        }

        public float GetNextFrameTime() => current_time+=time_step;

        public override float CurrentTime => current_time;
    }
}
