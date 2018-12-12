using ReOsuStoryBoardPlayer.Optimzer.Runtime;
using ReOsuStoryBoardPlayer.Parser.Collection;
using ReOsuStoryBoardPlayer.Parser.Reader;
using ReOsuStoryBoardPlayer.Parser.Stream;
using ReOsuStoryBoardPlayer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReOsuStoryBoardPlayer.Parser
{
    public static class StoryboardParserHelper
    {
        public static List<StoryBoardObject> GetStoryBoardObjects(string path)
        {
            //try
            {
                OsuFileReader reader = new OsuFileReader(path);

                VariableCollection collection = new VariableCollection(new VariableReader(reader).EnumValues());

                EventReader er = new EventReader(reader, collection);

                StoryboardReader storyboardReader = new StoryboardReader(er);

                List<StoryBoardObject> list;

                using (StopwatchRun.Count($"Parse&Optimze Storyboard Objects/Commands from {path}"))
                {
                    list = storyboardReader.EnumValues().ToList();

                    if (Setting.EnableRuntimeOptimzeObjects)
                    {
                        var optimzer = new RuntimeStoryboardOptimzer();
                        optimzer.Optimze(list); 
                    }
                }

                list.RemoveAll(c => c == null);

                return list;
            }
            /*
            catch (Exception e)
            {
                Log.Error($"Parse error! " + e.Message);
                return null;
            }
            */
        }
    }
}