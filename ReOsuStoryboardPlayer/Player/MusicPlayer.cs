using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using ReOsuStoryboardPlayer.Core.Utils;
using ReOsuStoryboardPlayer.Utils;
using System;
using System.Diagnostics;

namespace ReOsuStoryboardPlayer.Player
{
    public class MusicPlayer : PlayerBase, IDisposable
    {
        private WaveStream audioFileReader;

        private WaveOutEvent currentOut;

        public override uint Length { get => (uint)audioFileReader.TotalTime.TotalMilliseconds; }

        public override float CurrentTime { get => (uint)GetTime(); }

        public override float PlaybackSpeed { get => 1; set { } }

        public override bool IsPlaying { get => currentOut?.PlaybackState == PlaybackState.Playing; }

        public override float Volume { get => currentOut.Volume; set => currentOut.Volume = value; }

        private float baseOffset = 0;

        public void Load(string audio_file)
        {
            //release resource before loading new one.
            Dispose();

            try
            {
                currentOut = new WaveOutEvent();
                if (audio_file.EndsWith(".ogg"))
                    audioFileReader = new VorbisWaveReader(audio_file);
                else
                    audioFileReader = new AudioFileReader(audio_file);
                currentOut?.Init(audioFileReader);
            }
            catch (Exception e)
            {
                Log.Error($"Load audio file ({audio_file}) failed : {e.Message}");
                Dispose();
            }
        }

        public override void Jump(float time, bool pause)
        {
            time = Math.Max(0, Math.Min(time, Length));

            currentOut?.Stop();

            audioFileReader.Seek(0, System.IO.SeekOrigin.Begin);
            var provider = new OffsetSampleProvider(audioFileReader.ToSampleProvider())
            {
                SkipOver = TimeSpan.FromMilliseconds(time)
            };

            baseOffset = time;

            currentOut?.Init(provider);

            if (!pause)
                Play();
        }

        public override void Play()
        {
            currentOut?.Play();
        }

        private long GetTime()
        {
            var time = (currentOut is null ? 0 : (currentOut.GetPosition() * 1000.0 / currentOut.OutputWaveFormat.BitsPerSample / currentOut.OutputWaveFormat.Channels * 8 / currentOut.OutputWaveFormat.SampleRate)) + baseOffset;

            return (long)(time);
        }

        public override void Stop()
        {
            CleanCurrentOut();
            Jump(0, true);
        }

        public override void Pause()
        {
            currentOut?.Pause();
        }

        private void CleanCurrentOut()
        {
            currentOut?.Stop();
            currentOut?.Dispose();
            currentOut = null;
        }

        public void Dispose()
        {
            CleanCurrentOut();

            audioFileReader?.Dispose();
            audioFileReader = null;
        }
    }
}