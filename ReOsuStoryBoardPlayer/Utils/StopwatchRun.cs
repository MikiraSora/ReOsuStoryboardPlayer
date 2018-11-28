using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Utils
{
    public class StopwatchRun : IDisposable
    {
        private readonly string message;
        private readonly Stopwatch stopwatch;

        private StopwatchRun(string message)
        {
            this.message = message;
            stopwatch=ObjectPool<Stopwatch>.Instance.GetObject();
            stopwatch.Restart();
        }

        public void Dispose()
        {
            stopwatch.Stop();
            Log.User($"[StopwatchRun] {stopwatch.ElapsedMilliseconds}ms: {message}");
            ObjectPool<Stopwatch>.Instance.PutObject(stopwatch);
        }

        public static StopwatchRun Count(string message) => new StopwatchRun(message);
    }
}
