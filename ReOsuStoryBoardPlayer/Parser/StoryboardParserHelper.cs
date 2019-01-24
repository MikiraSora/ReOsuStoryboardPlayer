using ReOsuStoryBoardPlayer.Core.Base;
using ReOsuStoryBoardPlayer.Optimzer.Runtime;
using ReOsuStoryBoardPlayer.Core.Parser.Collection;
using ReOsuStoryBoardPlayer.Core.Parser.Reader;
using ReOsuStoryBoardPlayer.Core.Parser.Stream;
using ReOsuStoryBoardPlayer.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReOsuStoryBoardPlayer.Parser
{
    public static class StoryboardParserHelper
    {
        public static List<StoryBoardObject> GetStoryBoardObjects(string path)
        {
            OsuFileReader reader = new OsuFileReader(path);

            VariableCollection collection = new VariableCollection(new VariableReader(reader).EnumValues());

            EventReader er = new EventReader(reader, collection);

            StoryboardReader storyboardReader = new StoryboardReader(er);

            List<StoryBoardObject> list;

            using (StopwatchRun.Count($"Parse&Optimze Storyboard Objects/Commands from {path}"))
            {
                list=storyboardReader.EnumValues().ToList();
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