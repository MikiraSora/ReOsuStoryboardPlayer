using IrrKlang;
using ReOsuStoryboardPlayer.Core.Utils;
using System;
using System.Diagnostics;

namespace ReOsuStoryboardPlayer.Player
{
    public class MusicPlayer : PlayerBase, ISoundStopEventReceiver , IDisposable
    {
        private static ISoundEngine engine;

        private ISound sound;

        private string loaded_path;

        public override uint Length { get => sound.PlayLength; }

        public override float CurrentTime { get => (uint)GetTime(); }

        public override float PlaybackSpeed { get => sound.PlaybackSpeed; set => sound.PlaybackSpeed=value; }

        public override bool IsPlaying { get => !sound.Paused; }

        public override float Volume { get => sound.Volume; set => sound.Volume=value; }

        public Stopwatch offset_watch = new Stopwatch();

        private uint prev_mp3_time = 0;

        /// <summary>
        /// 表示播放器播放完当前音频文件，bool返回值表示是否默认自动重新加载
        /// </summary>
        public event Func<bool> FinishedPlay;

        static MusicPlayer()
        {
            engine=new ISoundEngine();
        }

        public void Load(string audio_file)
        {
            Dispose();

            sound=engine.Play2D(audio_file, false, true, StreamMode.AutoDetect, false);
            sound.setSoundStopEventReceiver(this);

            sound.Volume=Volume;
            sound.PlaybackSpeed=PlaybackSpeed;

            loaded_path=audio_file;

            Stop();
        }

        public override void Jump(float time, bool pause)
        {
            CheckIfDisposed();

            time =Math.Max(0, Math.Min(time, Length));

            Pause();
            sound.PlayPosition=(uint)time;
            offset_watch.Reset();

            prev_mp3_time=(uint)(time-sound.PlayPosition);

            if (!pause)
                Play();
        }

        public override void Play()
        {
            CheckIfDisposed();

            if (sound.Paused)
            {
                sound.Paused=false;
                offset_watch.Start();
            }
        }

        private long GetTime()
        {
            CheckIfDisposed();

            var playback = sound.PlayPosition;

            if (prev_mp3_time!=playback&&!sound.Paused)
                offset_watch.Restart();

            prev_mp3_time=playback;

            return (long)(prev_mp3_time+offset_watch.ElapsedMilliseconds*PlaybackSpeed);
        }

        public override void Stop()
        {
            CheckIfDisposed();

            Jump(0, true);
        }

        public override void Pause()
        {
            CheckIfDisposed();

            if (!sound.Paused)
            {
                sound.Paused=true;
                offset_watch.Stop();
            }
        }

        public void OnSoundStopped(ISound sound, StopEventCause reason, object userData)
        {
            Log.Debug($"MusicPlayer is stop,reason :{reason.ToString()}");

            if (FinishedPlay?.Invoke()??true)
            {
                Load(loaded_path);
                Jump(0, true);
            }
        }

        private void CheckIfDisposed()
        {
            Debug.Assert(sound != null, "sound is null,maybe this object has been disposed.");
        }

        public void Dispose()
        {
            sound?.Dispose();
            sound = null;
        }
    }
}