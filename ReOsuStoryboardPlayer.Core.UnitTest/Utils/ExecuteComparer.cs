using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReOsuStoryboardPlayer.Core.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryboardPlayer.Core.UnitTest.Utils
{
    public static class ExecuteComparer
    {
        private static Random rand = new Random();

        public static void CompareStoryboardObjects(StoryboardObject a, StoryboardObject b)
        {
            a.ResetTransform();
            b.ResetTransform();

            //选取optimzed范围内的raw物件的命令作为时间参照，否则optimzer时间范围外面的命令/时间,因为有优化器提前对optimzer计算导致对比失败
            foreach (var command in b.CommandMap.Values.SelectMany(l => l).Where(x => b.FrameStartTime<=x.StartTime&&x.EndTime<=b.FrameEndTime))
            {
                var time = (float)(command.StartTime!=command.EndTime ? (command.StartTime + (command.EndTime-command.StartTime)*Math.Max(0.1,rand.NextDouble())) : command.EndTime+1);

                Update(time);

                CompareStoryboardObjectTransform(a, b);
            }

            void Update(float t)
            {
                a.Update(t);
                b.Update(t);
            }

            CompareStoryboardObjectTransform(a, b);
        }

        public static void CompareStoryboardObjectTransform(StoryboardObject a, StoryboardObject b)
        {
            Assert.AreEqual(a.Color.ToString(), b.Color.ToString());
            Assert.AreEqual(a.IsAdditive, b.IsAdditive);
            Assert.AreEqual(a.IsHorizonFlip, b.IsHorizonFlip);
            Assert.AreEqual(a.IsVerticalFlip, b.IsVerticalFlip);
            Assert.AreEqual(a.Rotate.ToString(), b.Rotate.ToString());
            Assert.AreEqual(a.Scale.ToString(), b.Scale.ToString());
            Assert.AreEqual(a.Postion.ToString(), b.Postion.ToString());
        }
    }
}
