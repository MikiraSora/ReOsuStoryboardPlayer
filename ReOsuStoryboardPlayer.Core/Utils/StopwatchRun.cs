using System;
using System.Diagnostics;

namespace ReOsuStoryboardPlayer.Core.Utils
{
    public class StopwatchRun : IDisposable
    {
        private readonly Func<string> callback;
        private readonly Stopwatch stopwatch;

        private StopwatchRun()
        {
            stopwatch=ObjectPool<Stopwatch>.Instance.GetObject();
            stopwatch.Restart();
        }

        private StopwatchRun(string message) : this(() => message)
        {
        }

        private StopwatchRun(Func<string> callback) : this()
        {
            this.callback=callback;
        }

        public void Dispose()
        {
            stopwatch.Stop();
            Log.User($"[StopwatchRun] {stopwatch.ElapsedMilliseconds}ms: {callback()}");
            ObjectPool<Stopwatch>.Instance.PutObject(stopwatch);
        }

        public static StopwatchRun Count(string message) => new StopwatchRun(message);

        public static StopwatchRun Count(Func<string> callback) => new StopwatchRun(callback);
    }
}