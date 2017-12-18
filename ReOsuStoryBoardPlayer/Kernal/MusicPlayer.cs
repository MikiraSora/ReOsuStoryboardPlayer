﻿using IrrKlang;
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

        private DateTime prev_time;
        
        public MusicPlayer(string file_path)
        {
            audioFilePath = file_path;

            sound = engine.Play2D(file_path, false, true, StreamMode.AutoDetect, false);
            
            CurrentFixedTime = 0;
        }

        public void Tick()
        {
            var now_time =DateTime.Now;
            float tick_time = IsPlaying?(float)(now_time-prev_time).TotalMilliseconds/*offset*/:0/*keep*/;
            prev_time = now_time;

            CurrentFixedTime +=tick_time;

            if (Math.Abs(CurrentFixedTime-CurrentPlayback)>26)
            {
                CurrentFixedTime = CurrentPlayback;
            }
        }

        public void Play()
        {
            sound.Paused = false;

            prev_time = DateTime.Now;
        }

        public void Pause()
        {
            sound.Paused = true;
        }

        public void Jump(uint pos)
        {
            sound.PlayPosition = pos;
            OnJumpCurrentPlayingTime?.Invoke(pos);
        }

        public void Term()
        {
            //fix_thread.Abort();
        }
    }
}