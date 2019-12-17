using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using ReOsuStoryboardPlayer.Player;

namespace ReOsuStoryboardPlayer.WPFControl.Example
{
    internal sealed class NAudioMusicPlayer : PlayerBase, IDisposable
    {
        public enum PlayerStatusEnum
        {
            NotInitialized, Ready, Playing, Paused, Stopped, Finished
        }

        private int _playTime;
        private const int Latency = 5;
        private static bool WaitingMode => true;

        private CancellationTokenSource _cts;
        private readonly object _propertiesLock = new object();
        private IWavePlayer _device;
        private AudioFileReader _reader;
        private PlayerStatusEnum _playerStatus;

        private int _progressRefreshInterval;

        private string _filePath;

        public override float CurrentTime => _playTime;
        public override float Volume { get => _reader.Volume; set => _reader.Volume = value; }
        public override uint Length => (uint)(_reader?.TotalTime.TotalMilliseconds ?? 0);
        public override bool IsPlaying => _playerStatus == PlayerStatusEnum.Playing;
        public override float PlaybackSpeed { get => 1; set => throw new NotSupportedException(); }

        public void Load(string filePath)
        {
            Dispose();
            _cts = new CancellationTokenSource();
            _filePath = filePath;

            var fi = new FileInfo(_filePath);
            if (!fi.Exists)
            {
                throw new FileNotFoundException();
            }

            _reader = new AudioFileReader(_filePath)
            {
                Volume = 1f
            };

            _device = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, Latency);
            _device.PlaybackStopped += (sender, args) =>
            {
                PlayerStatus = PlayerStatusEnum.Finished;
            };

            _device.Init(_reader);
            Task.Factory.StartNew(UpdateProgress, TaskCreationOptions.LongRunning);

            PlayerStatus = PlayerStatusEnum.Ready;
        }

        private void UpdateProgress()
        {
            while (!_cts.IsCancellationRequested)
            {
                if (_reader != null && PlayerStatus != PlayerStatusEnum.NotInitialized && PlayerStatus != PlayerStatusEnum.Finished)
                {
                    _playTime = (int)_reader?.CurrentTime.TotalMilliseconds;
                }

                Thread.Sleep(5);
            }
        }


        public override void Play()
        {
            PlayWithoutNotify();
            PlayerStatus = PlayerStatusEnum.Playing;
        }

        public override void Stop()
        {
            ResetWithoutNotify();
        }

        public override void Pause()
        {
            PauseWithoutNotify();
            PlayerStatus = PlayerStatusEnum.Paused;
        }

        public override void Jump(float ms, bool play)
        {
            if (ms < 0) ms = 0;
            var span = TimeSpan.FromMilliseconds(ms);
            if (_reader != null)
            {
                _reader.CurrentTime = span >= _reader.TotalTime ? _reader.TotalTime - new TimeSpan(0, 0, 0, 0, 1) : span;
            }

            if (!play) PauseWithoutNotify();
        }

        public void Reset()
        {
            Jump(0, true);
            Play();
        }

        private void PlayWithoutNotify()
        {
            _device.Play();
        }

        private void PauseWithoutNotify()
        {
            _device?.Pause();
        }

        internal void ResetWithoutNotify()
        {
            Jump(0, false);
            PlayerStatus = PlayerStatusEnum.Stopped;
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _device?.Dispose();
            _device = null;
            _reader?.Dispose();
            _reader = null;
            _cts?.Dispose();
        }

        #region Properties

        public int ProgressRefreshInterval
        {
            get => _progressRefreshInterval;
            set
            {
                if (value < 10)
                    _progressRefreshInterval = 10;
                _progressRefreshInterval = value;
            }
        }

        public PlayerStatusEnum PlayerStatus
        {
            get => _playerStatus;
            private set
            {
                Console.WriteLine(@"Music: " + value);
                _playerStatus = value;
            }
        }

        #endregion
    }
}
