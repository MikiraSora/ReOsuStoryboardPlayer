using ReOsuStoryBoardPlayer.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Utils
{
    public static class StoryboardUpdateLimitCalculator
    {
        /// <summary>
        /// 计算最大同时执行物件数量
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        public static int CalculateMaxUpdatingObjectsCount(this IEnumerable<StoryBoardObject> objects)
        {
            var timeline = objects.SelectMany(obj => new[] { (obj.FrameStartTime, true), (obj.FrameEndTime, false) })
                .GroupBy(x => x.Item1)
                .Select(p => (p.Key, p.Sum(x => x.Item2 ? 1 : -1)))
                .OrderBy(p => p.Key);

            int max = 0;

            foreach (var stamp in timeline)
                max=Math.Max(max, max+stamp.Item2);

            return max;
        }
    }
}
