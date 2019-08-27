using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Kernel;
using ReOsuStoryboardPlayer.Core.Optimzer;
using ReOsuStoryboardPlayer.Core.Optimzer.DefaultOptimzer;
using ReOsuStoryboardPlayer.Core.Parser.Collection;
using ReOsuStoryboardPlayer.Core.Parser.Reader;
using ReOsuStoryboardPlayer.Core.Parser.Stream;
using ReOsuStoryboardPlayer.Core.PrimitiveValue;
using ReOsuStoryboardPlayer.Core.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryboardPlayer.Core.Example
{
    public class Example1
    {
        static void Main(string[] args)
        {
            //path of standard beatmap folder which contains storyboard.
            Console.Write("Input beatmap folder path. > ");
            var beatmap_folder = Console.ReadLine();

            //time we need to update.
            Console.Write("Input time value. > ");
            var update_time = int.Parse(Console.ReadLine());

            //not allow lib output anything.
            Setting.AllowLog = false;

            #region Get beatmap info about osb file and osu file.

            //for convenience
            var beatmap_info = BeatmapFolderInfo.Parse(beatmap_folder);

            #endregion

            #region Get storyboard objects and create updater for controling objects in timeline.

            //use optimzer
            StoryboardOptimzerManager.AddOptimzer<RuntimeOptimzer>(); // or StoryboardOptimzerManager.AddOptimzer(new RuntimeOptimzer());
            StoryboardOptimzerManager.AddOptimzer<ParserStaticOptimzer>();

            //get storyboard objects from .osb file
            var osb_object_list = GetStoryboardObjectsFromFile(beatmap_info.osb_file_path);
            osb_object_list.Sort((a, b) => (int)(a.FileLine - b.FileLine));

            //get storyboard objects from first .osu file in folder
            var osu_object_list = GetStoryboardObjectsFromFile(beatmap_info.DifficultFiles.First().Value);
            osu_object_list.Sort((a, b) => (int)(a.FileLine - b.FileLine));

            //combine .osu objects and .osb objects
            var storyboard_objects = CombineStoryboardObjects(osb_object_list, osu_object_list);

            //create updater
            var updater = new StoryboardUpdater(storyboard_objects);

            #endregion

            //Update storyboard to the specified time , and then you could take UpdatingStoryboardObjects and render them.
            updater.Update(update_time);

            //Maybe collection element is different if you call Update() within difference time.
            var updating_objects = updater.UpdatingStoryboardObjects;

            //draw them.
            foreach (var obj in updating_objects)
            {
                Draw(obj);
            }

            Console.ReadLine();
        }

        private static void Draw(StoryboardObject obj)
        {
            //todo . gugu
            Console.WriteLine($"draw {obj.ToString()}");
        }

        public static List<StoryboardObject> CombineStoryboardObjects(List<StoryboardObject> osb_list, List<StoryboardObject> osu_list)
        {
            List<StoryboardObject> result = new List<StoryboardObject>();

            Add(Layout.Background);
            Add(Layout.Fail);
            Add(Layout.Pass);
            Add(Layout.Foreground);

            int z = 0;

            foreach (var obj in result)
            {
                obj.Z = z++;
            }

            return result;

            void Add(Layout layout)
            {
                result.AddRange(osu_list.Where(x => x.layout == layout));//先加osu
                result.AddRange(osb_list.Where(x => x.layout == layout).Select(x =>
                {
                    x.FromOsbFile = true;
                    return x;
                }));//后加osb覆盖
            }
        }

        public static List<StoryboardObject> GetStoryboardObjectsFromFile(string path)
        {
            OsuFileReader reader = new OsuFileReader(path);

            VariableCollection collection = new VariableCollection(new VariableReader(reader).EnumValues());

            EventReader er = new EventReader(reader, collection);

            StoryboardReader StoryboardReader = new StoryboardReader(er);

            var list = StoryboardReader.EnumValues().OfType<StoryboardObject>().ToList();

            //计算每个物件的FrameStartTime
            foreach (var obj in list)
                obj.CalculateAndApplyBaseFrameTime();

            return list;
        }
    }
}
