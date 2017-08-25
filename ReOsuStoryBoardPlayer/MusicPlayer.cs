using IrrKlang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public MusicPlayer(string file_path)
        {
            audioFilePath = file_path;

            sound = engine.Play2D(file_path, false, true, StreamMode.AutoDetect, false);
        }

        public void Play() => sound.Paused = false;

        public void Pause() => sound.Paused = true;

        public void Jump(uint pos)
        {
            sound.PlayPosition = pos;
            OnJumpCurrentPlayingTime?.Invoke(pos);
        }
    }
}
