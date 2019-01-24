using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Parser.Collection;
using ReOsuStoryboardPlayer.Core.Parser.Reader;
using ReOsuStoryboardPlayer.Core.Parser.Stream;
using ReOsuStoryboardPlayer.Core.Utils;
using ReOsuStoryboardPlayer.Optimzer.Runtime;
using System.Collections.Generic;
using System.Linq;

namespace ReOsuStoryboardPlayer.Parser
{
    public static class StoryboardParserHelper
    {
        public static List<StoryboardObject> GetStoryboardObjects(string path)
        {
            OsuFileReader reader = new OsuFileReader(path);

            VariableCollection collection = new VariableCollection(new VariableReader(reader).EnumValues());

            EventReader er = new EventReader(reader, collection);

            StoryboardReader StoryboardReader = new StoryboardReader(er);

            List<StoryboardObject> list;

            using (StopwatchRun.Count($"Parse&Optimze Storyboard Objects/Commands from {path}"))
            {
                list=StoryboardReader.EnumValues().ToList();
                list.RemoveAll(c => c==null);

                foreach (var obj in list)
                {
                    obj.CalculateAndApplyBaseFrameTime();
#if DEBUG
                    //objects are all ready to execute. so block ::AddCommand()/::RemoveCommand() for safe.
                    obj.BlockCommandsChange();
#endif
                }

                if (PlayerSetting.EnableRuntimeOptimzeObjects)
                {
                    var optimzer = new RuntimeStoryboardOptimzer();
                    optimzer.Optimze(list);
                }
            }

            return list;
        }
    }
}