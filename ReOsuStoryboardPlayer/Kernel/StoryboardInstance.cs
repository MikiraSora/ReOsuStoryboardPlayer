using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Kernel;
using ReOsuStoryboardPlayer.Core.Utils;
using ReOsuStoryboardPlayer.Graphics;
using ReOsuStoryboardPlayer.Parser;
using ReOsuStoryBoardPlayer.Parser;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ReOsuStoryboardPlayer.Kernel
{
    /// <summary>
    /// 表示一个完整的Storyboard实例，包括SB物件和SB相关信息等
    /// </summary>
    public class StoryboardInstance
    {
        public StoryboardUpdater Updater { get; private set; }
        public BeatmapFolderInfoEx Info { get; private set; }
        public StoryboardResource Resource { get; set; }

        private StoryboardInstance()
        {
        }

        public static StoryboardInstance Load(BeatmapFolderInfoEx info)
        {
            StoryboardInstance instance = new StoryboardInstance();

            instance.Info=info;

            using (StopwatchRun.Count("Load and Parse osb/osu file"))
            {
                List<StoryboardObject> temp_objs_list = new List<StoryboardObject>(), parse_osb_Storyboard_objs = new List<StoryboardObject>();

                //get objs from osu file
                List<StoryboardObject> parse_osu_Storyboard_objs = string.IsNullOrWhiteSpace(info.osu_file_path) ? new List<StoryboardObject>() : StoryboardParserHelper.GetStoryboardObjects(info.osu_file_path);
                AdjustZ(parse_osu_Storyboard_objs);

                if ((!string.IsNullOrWhiteSpace(info.osb_file_path))&&File.Exists(info.osb_file_path))
                {
                    parse_osb_Storyboard_objs=StoryboardParserHelper.GetStoryboardObjects(info.osb_file_path);
                    AdjustZ(parse_osb_Storyboard_objs);
                }

                temp_objs_list=CombineStoryboardObjects(parse_osb_Storyboard_objs, parse_osu_Storyboard_objs);

                void AdjustZ(List<StoryboardObject> list)
                {
                    list.Sort((a, b) => (int)(a.FileLine-b.FileLine));
                }

                List<StoryboardObject> CombineStoryboardObjects(List<StoryboardObject> osb_list, List<StoryboardObject> osu_list)
                {
                    List<StoryboardObject> result = new List<StoryboardObject>();

                    Add(Layer.Background);
                    Add(Layer.Fail);
                    Add(Layer.Pass);
                    Add(Layer.Foreground);
                    Add(Layer.Overlay);

                    int z = 0;
                    foreach (var obj in result)
                    {
                        obj.Z=z++;
                    }

                    return result;

                    void Add(Layer layout)
                    {
                        result.AddRange(osu_list.Where(x => x.layer==layout));//先加osu
                        result.AddRange(osb_list.Where(x => x.layer==layout).Select(x =>
                        {
                            x.FromOsbFile=true;
                            return x;
                        }));//后加osb覆盖
                    }
                }

                instance.Updater=new StoryboardUpdater(temp_objs_list);
            }

            return instance;
        }
    }
}