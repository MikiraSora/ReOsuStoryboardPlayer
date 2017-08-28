using IrrKlang;
using System;
using System.Collections.Generic;
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

        private object locker = new object();

        public uint FixCurrentPlayback {
            get
            {
                lock (locker)
                {
                    return _fixCurrentPlayback;
                }
            }
        }

        public uint _fixCurrentPlayback = 0;

        Thread fix_thread;

        public MusicPlayer(string file_path)
        {
            audioFilePath = file_path;

            sound = engine.Play2D(file_path, false, true, StreamMode.AutoDetect, false);

            sound.Volume = 0;

            fix_thread = new Thread(() =>
            {
                while (true)
                {
                    lock (locker)
                    {
                        if (!sound.Paused)
                        {
                            _fixCurrentPlayback += 8;
                        }

                        if (Math.Abs(_fixCurrentPlayback - CurrentPlayback) >= 22)
                        {
                            _fixCurrentPlayback = CurrentPlayback;
                        }

                    }

                    Thread.Sleep(8);
                }
            });

            fix_thread.Start();
        }

        public void Play()
        {
            sound.Paused = false;
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
            fix_thread.Abort();
        }
    }
}
