using IrrKlang;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer
{
    public class MusicPlayer
    {
        static ISoundEngine engine;

        static MusicPlayer()
        {
            engine = new ISoundEngine();
        }

        string audioFilePath;

        public event Action<uint> OnJumpCurrentPlayingTime;

        public string AudioFilePath { get => audioFilePath; }

        ISound sound;

        public uint Length { get => sound.PlayLength; }

        public uint CurrentPlayback { get => sound.PlayPosition; }

        public float PlaybackSpeed { get => sound.PlaybackSpeed; set => sound.PlaybackSpeed=value; }

        public bool IsPlaying { get => !sound.Paused; }
        
        public float CurrentFixedTime { get; private set; }

        public float Volume { get => sound.Volume; set => sound.Volume = value; }

        public Stopwatch offset_watch = new Stopwatch();
        public uint prev_mp3_time=0;
        
        public MusicPlayer(string file_path)
        {
            audioFilePath = file_path;

            sound = engine.Play2D(file_path, false, true, StreamMode.AutoDetect, false);
            
            CurrentFixedTime = 0;
            offset_watch.Reset();
        }

        public void Tick()
        {
            if (prev_mp3_time!=CurrentPlayback && !sound.Paused)
                offset_watch.Restart();

            prev_mp3_time = CurrentPlayback;

            CurrentFixedTime = prev_mp3_time + offset_watch.ElapsedMilliseconds;

            //Debug.Assert(offset_watch.ElapsedMilliseconds > 26);
        }

        public void Play()
        {
            sound.Paused = false;
            offset_watch.Start();
        }

        public void Pause()
        {
            sound.Paused = true;
            offset_watch.Stop();
        }

        public void Jump(uint pos)
        {
            Pause();
            sound.PlayPosition = pos;
            offset_watch.Reset();

            OnJumpCurrentPlayingTime?.Invoke((uint)CurrentFixedTime);
        }

        public void Term()
        {
            //fix_thread.Abort();
        }
    }
}
