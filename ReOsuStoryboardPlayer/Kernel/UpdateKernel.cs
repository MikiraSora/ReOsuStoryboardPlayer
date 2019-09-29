using ReOsuStoryboardPlayer;
using ReOsuStoryboardPlayer.Kernel;
using ReOsuStoryboardPlayer.Player;
using ReOsuStoryboardPlayer.Tools;
using ReOsuStoryBoardPlayer.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Kernel
{
    public static class UpdateKernel
    {
        private const double SYNC_THRESHOLD_MIN = 17;// 1/60fps

        private static double _timestamp = 0;
        private static Stopwatch _timestamp_stopwatch = new Stopwatch();

        public const float THOUSANDTH = 1.0f / 1000.0f;
        private static Stopwatch _update_stopwatch = new Stopwatch();

        public static StoryboardInstance Instance { get; private set; }

        public static void LoadStoryboardInstance(StoryboardInstance instance)
        {
            Instance = instance;
            _timestamp = 0;
        }

        private static double GetSyncTime()
        {
            var audioTime = MusicPlayerManager.ActivityPlayer.CurrentTime;
            var playbackRate = MusicPlayerManager.ActivityPlayer.PlaybackSpeed;

            double step = _timestamp_stopwatch.ElapsedMilliseconds * playbackRate;
            _timestamp_stopwatch.Restart();

            if (MusicPlayerManager.ActivityPlayer.IsPlaying && PlayerSetting.EnableTimestamp)
            {
                double nextTime = _timestamp + step;

                double diffAbs = Math.Abs(nextTime - audioTime) * playbackRate;
                if (diffAbs > SYNC_THRESHOLD_MIN * playbackRate)//不同步
                {
                    if (audioTime > _timestamp)//音频快
                    {
                        nextTime += diffAbs * 0.6;//SB快速接近音频
                    }
                    else//SB快
                    {
                        nextTime = _timestamp;//SB不动
                    }
                }

                return _timestamp = nextTime;
            }
            else
            {
                return _timestamp = MusicPlayerManager.ActivityPlayer.CurrentTime;
            }
        }

        public static void Update()
        {
            if (Instance==null)
                return;

            _update_stopwatch.Restart();

            ExecutorSync.ClearTask();
            var time = GetSyncTime();

            Instance.Updater.Update((float)time);
            ToolManager.FrameUpdate();

            _update_stopwatch.Stop();
        }

        public static void FrameRateLimit()
        {
            if (PlayerSetting.EnableHighPrecisionFPSLimit)
            {
                /*//todo
                if (Math.Abs(TargetUpdateFrequency - PlayerSetting.MaxFPS) > 10e-5)
                {
                    TargetUpdateFrequency = PlayerSetting.MaxFPS;
                    TargetRenderFrequency = PlayerSetting.MaxFPS;
                }
                */
            }
            else
            {
                float time = (_update_stopwatch.ElapsedMilliseconds + RenderKernel.RenderCostTime) * THOUSANDTH;
                if (PlayerSetting.MaxFPS != 0)
                {
                    float period = 1.0f / PlayerSetting.MaxFPS;
                    if (period > time)
                    {
                        int sleep = (int)((period - time) * 1000);
                        sleep = Math.Max(0, sleep - 1);
                        Thread.Sleep(sleep);
                    }
                }
            }
        }

        public static long UpdateCostTime => _update_stopwatch.ElapsedMilliseconds;
    }
}
