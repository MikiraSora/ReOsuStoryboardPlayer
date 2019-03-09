using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryboardPlayer.Core.UnitTest.Command.LoopCommand
{
    [TestClass]
    public class LoopUnrollingTest
    {
        static string file_path = Path.Combine("TestData", "Camellia vs Akira Complex - Reality Distortion (rrtyui).osb");

        [TestMethod]
        public void Main()
        {
            Setting.EnableLoopCommandUnrolling=true;
            var loop_unrolled_objects = StoryboardParserHelper.GetStoryboardObjects(file_path).Where(x => x.ContainLoop).ToList();

            Setting.EnableLoopCommandUnrolling=false;
            var normal_objects = StoryboardParserHelper.GetStoryboardObjects(file_path).Where(x => x.ContainLoop);

            var object_pair=from a in loop_unrolled_objects
            join b in normal_objects on a.FileLine equals b.FileLine
            select (a,b);

            int count = 0;

            foreach (var (a,b) in object_pair)
            {
                Utils.ExecuteComparer.CompareStoryboardObjects(a, b);
                count++;
            }

            Assert.AreEqual(count, normal_objects.Count());
            Assert.AreEqual(count, loop_unrolled_objects.Count());
        }
    }
}
