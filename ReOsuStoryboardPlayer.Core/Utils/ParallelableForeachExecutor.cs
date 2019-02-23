using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryboardPlayer.Core.Utils
{
    public static class ParallelableForeachExecutor
    {
        private static ParallelOptions options;
        private static int prev_thread = 1;

        private static ParallelOptions Options
        {
            get
            {

                if (options==null||prev_thread!=Setting.UpdateThreadCount)
                {
                    prev_thread=Setting.UpdateThreadCount;
                    options=new ParallelOptions() { MaxDegreeOfParallelism=prev_thread };
                }

                return options;
            }
        }

        public static void Foreach<T>(bool need_parallel,IEnumerable<T> collection,Action<T> action)
        {
            var o = Options;

            if (need_parallel&&prev_thread>1)
                Parallel.ForEach(collection, o, action);
            else
                foreach (var obj in collection)
                    action(obj);
        }
    }
}
