using IrrKlang;
using ReOsuStoryBoardPlayer.Kernel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Player
{
    public class MusicPlayer : PlayerBase
    {
        private static ISoundEngine engine;
        private ISound sound;

        private string loaded_path;

        public override uint Length { get => sound.PlayLength; }

        public override uint CurrentTime { get => (uint)GetTime(); }

        public override float PlaybackSpeed { get => sound.PlaybackSpeed; set => sound.PlaybackSpeed=value; }

        public override bool IsPlaying { get => !sound.Paused; }

        public float CurrentFixedTime { get; private set; }

        public override float Volume { get => sound.Volume; set => sound.Volume=value; }
        
        public Stopwatch offset_watch = new Stopwatch();
        public uint prev_mp3_time = 0;

        static MusicPlayer()
        {
            engine=new ISoundEngine();
        }

        public void Load(string audio_file)
        {
            if (sound!=null)
            {
                sound.Dispose();
            }

            sound=engine.Play2D(audio_file, false, true, StreamMode.AutoDetect, false);

            CurrentFixedTime=0;
            offset_watch.Reset();

            loaded_path=audio_file;
        }

        public override void Jump(uint time)
        {
            Pause();
            sound.PlayPosition=time;
            offset_watch.Reset();

            prev_mp3_time=time-sound.PlayPosition;

            StoryboardInstanceManager.ActivityInstance.Flush();
        }

        public override void Play()
        {
            if (sound.Paused)
            {
                sound.Paused=false;
                offset_watch.Start();
            }
        }

        private long GetTime()
        {
            var playback = sound.PlayPosition;

            if (prev_mp3_time!=playback&&!sound.Paused)
                offset_watch.Restart();

            prev_mp3_time=playback;

            return prev_mp3_time+offset_watch.ElapsedMilliseconds;
        }

        public override void Stop()
        {
            Jump(0);
        }

        public override void Pause()
        {
            if (!sound.Paused)
            {
                sound.Paused=true;
                offset_watch.Stop();
            }
        }
    }
}
