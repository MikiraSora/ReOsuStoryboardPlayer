using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Optimzer;
using ReOsuStoryboardPlayer.Core.Parser.Collection;
using ReOsuStoryboardPlayer.Core.Parser.Reader;
using ReOsuStoryboardPlayer.Core.Parser.Stream;
using ReOsuStoryboardPlayer.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryboardPlayer.Core.UnitTest.Optimzer
{
    [TestClass]
    public class SimpleRuntimeCompareTest
    {
        private static bool optimzer_add = false;

        [TestMethod]
        public void MainTest()
        {
            foreach (var file in Directory.EnumerateFiles("TestData","*.osb"))
            {
                for (int i = 0; i<=3; i++)
                {
                    TestStoryboard(file, i);
                }
            }
        }

        public void TestStoryboard(string file,int level)
        {
            Console.WriteLine($"Start test {level}:{file}");

            //get objects without any optimzer processing.
            var raw_objects=GetStoryboardObjectsFromOsb(file, 0);

            //get objects with level optimzer.
            var optimzed_objects = GetStoryboardObjectsFromOsb(file, level);

            var couple = (from a in raw_objects
                          join b in optimzed_objects on a.FileLine equals b.FileLine select (a, b)).ToList();

            Assert.AreEqual(raw_objects.Count, couple.Count);
            Assert.AreEqual(optimzed_objects.Count, couple.Count);

            couple.AsParallel().ForAll(x => CompareStoryboardObjects(x.a, x.b));
        }
        
        private static void InitOptimzerManager()
        {
            var base_type = typeof(OptimzerBase);

            var need_load_optimzer = AppDomain.CurrentDomain.GetAssemblies()
                .Select(x => x.GetTypes())
                .SelectMany(l => l)
                .Where(x => x.IsClass&&!x.IsAbstract&&x.IsSubclassOf(base_type)).Select(x => {
                    try
                    {
                        return Activator.CreateInstance(x);
                    }
                    catch (Exception e)
                    {
                        Log.Warn($"Can't load optimzer \"{x.Name}\" :"+e.Message);
                        return null;
                    }
                }).OfType<OptimzerBase>();

            foreach (var optimzer in need_load_optimzer)
                StoryboardOptimzerManager.AddOptimzer(optimzer);

            optimzer_add=true;
        }

        private static List<StoryboardObject> GetStoryboardObjectsFromOsb(string path,int level)
        {
            OsuFileReader reader = new OsuFileReader(path);

            VariableCollection collection = new VariableCollection(new VariableReader(reader).EnumValues());

            EventReader er = new EventReader(reader, collection);

            StoryboardReader StoryboardReader = new StoryboardReader(er);

            List<StoryboardObject> list;

            list=StoryboardReader.EnumValues().ToList();
            list.RemoveAll(c => c==null);

            foreach (var obj in list)
                obj.CalculateAndApplyBaseFrameTime();

            if (PlayerSetting.StoryboardObjectOptimzeLevel>0)
            {
                if (!optimzer_add)
                    InitOptimzerManager();

                StoryboardOptimzerManager.Optimze(level, list);
            }

            return list;
        }

        public void CompareStoryboardObjects(StoryboardObject raw,StoryboardObject optimzed)
        {
            raw.ResetTransform();
            optimzed.ResetTransform();

            //选取optimzed范围内的raw物件的命令作为时间参照，否则optimzer时间范围外面的命令/时间,因为有优化器提前对optimzer计算导致对比失败
            foreach (var command in optimzed.CommandMap.Values.SelectMany(l=>l).Where(x=>optimzed.FrameStartTime<=x.StartTime&&x.EndTime<=optimzed.FrameEndTime))
            {
                var time = command.StartTime!=command.EndTime?(command.StartTime+command.EndTime)*0.5f : command.EndTime+1;

                Update(time);

                CompareStoryboardObjectTransform(raw, optimzed);
            }

            void Update(float t)
            {
                raw.Update(t);
                optimzed.Update(t);
            }
        }

        public void CompareStoryboardObjectTransform(StoryboardObject raw, StoryboardObject optimzed)
        {
            Assert.AreEqual(raw.Color.ToString(), optimzed.Color.ToString());
            Assert.AreEqual(raw.IsAdditive, optimzed.IsAdditive);
            Assert.AreEqual(raw.IsHorizonFlip, optimzed.IsHorizonFlip);
            Assert.AreEqual(raw.IsVerticalFlip, optimzed.IsVerticalFlip);
            Assert.AreEqual(raw.Rotate.ToString(), optimzed.Rotate.ToString());
            Assert.AreEqual(raw.Scale.ToString(), optimzed.Scale.ToString());
            Assert.AreEqual(raw.Postion.ToString(), optimzed.Postion.ToString());
        }
    }
}
