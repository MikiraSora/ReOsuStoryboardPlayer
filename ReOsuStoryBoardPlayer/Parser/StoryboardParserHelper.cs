using ReOsuStoryBoardPlayer.Parser.Collection;
using ReOsuStoryBoardPlayer.Parser.Reader;
using ReOsuStoryBoardPlayer.Parser.Stream;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser
{
    public static class StoryboardParserHelper
    {
        public static List<StoryBoardObject> GetStoryBoardObjects(string path)
        {
            try
            {
                var chars = new ReadOnlyMemory<char>(File.ReadAllText(path).ToCharArray());

                OsuFileReader reader = new OsuFileReader(chars);

                SectionReader sr = reader.GetSectionReader(Section.Variables);

                VariableCollection collection = new VariableCollection(new VariableReader(sr).GetValues());

                EventReader er = new EventReader(reader.ReadSectionContent(Section.Events), collection);

                StoryboardReader storyboardReader = new StoryboardReader(er);

                var list = storyboardReader.GetValues().ToList();

                list.RemoveAll(c => c == null);

                return list;
            }
            catch (Exception e)
            {
                Log.Error($"Parse error! "+e.Message);
                return null;
            }
        }
    }
}
