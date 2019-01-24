using ReOsuStoryBoardPlayer.Core.Base;
using ReOsuStoryBoardPlayer.Core.Kernel;
using ReOsuStoryBoardPlayer.Core.Utils;
using ReOsuStoryBoardPlayer.Graphics;
using ReOsuStoryBoardPlayer.Parser;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReOsuStoryBoardPlayer.Kernel
{
    /// <summary>
    /// 表示一个完整的Storyboard实例，包括SB物件和SB相关信息等
    /// </summary>
    public class StoryboardInstance
    {
        public StoryboardUpdater Updater { get; private set; }
        public BeatmapFolderInfo Info { get; private set; }
        public StoryboardResource Resource { get; set; }

        private StoryboardInstance()
        {
        }

        public static StoryboardInstance Load(BeatmapFolderInfo info)
        {
            StoryboardInstance instance = new StoryboardInstance();

            instance.Info=info;

            using (StopwatchRun.Count("Load and Parse osb/osu file"))
            {
                List<StoryBoardObject> temp_objs_list = new List<StoryBoardObject>(), parse_osb_storyboard_objs = new List<StoryBoardObject>();

                //get objs from osu file
                List<StoryBoardObject> parse_osu_storyboard_objs = string.IsNullOrWhiteSpace(info.osu_file_path) ? new List<StoryBoardObject>() : StoryboardParserHelper.GetStoryBoardObjects(info.osu_file_path);
                AdjustZ(parse_osu_storyboard_objs, 0);

                if ((!string.IsNullOrWhiteSpace(info.osb_file_path))&&File.Exists(info.osb_file_path))
                {
                    parse_osb_storyboard_objs=StoryboardParserHelper.GetStoryBoardObjects(info.osb_file_path);
                    AdjustZ(parse_osb_storyboard_objs, 0);
                }

                temp_objs_list=CombineStoryBoardObjects(parse_osb_storyboard_objs, parse_osu_storyboard_objs);

                void AdjustZ(List<StoryBoardObject> list, int base_z)
                {
                    list.Sort((a, b) => (int)(a.FileLine-b.FileLine));
                }

                List<StoryBoardObject> CombineStoryBoardObjects(List<StoryBoardObject> osb_list, List<StoryBoardObject> osu_list)
                {
                    List<StoryBoardObject> result = new List<StoryBoardObject>();

                    Add(Layout.Background);
                    Add(Layout.Fail);
                    Add(Layout.Pass);
                    Add(Layout.Foreground);

                    int z = 0;
                    foreach (var obj in result)
                    {
                        obj.Z=z++;
                    }

                    return result;

                    void Add(Layout layout)
                    {
                        result.AddRange(osu_list.Where(x => x.layout==layout));//先加osu
                        result.AddRange(osb_list.Where(x => x.layout==layout).Select(x =>
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