using ReOsuStoryBoardPlayer.Commands;
using ReOsuStoryBoardPlayer.Parser;
using ReOsuStoryBoardPlayer.Parser.Collection;
using ReOsuStoryBoardPlayer.Parser.Extension;
using ReOsuStoryBoardPlayer.Parser.Reader;
using ReOsuStoryBoardPlayer.Parser.Stream;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var chars = new ReadOnlyMemory<char>(File.ReadAllText("2d.osb").ToCharArray());

            OsuFileReader reader = new OsuFileReader(chars);

            SectionReader sr = reader.GetSectionReader(Section.Variables);

            VariableCollection collection = new VariableCollection(new VariableReader(sr).GetValues());
            
            EventReader er = new EventReader(reader.ReadSectionContent(Section.Events), collection);

            StoryboardReader storyboardReader = new StoryboardReader(er);

            VariableCollection variables = new VariableCollection();

            variables["$zz"] = new StoryboardVariable("$zz", "666");
            variables["$aa"] = new StoryboardVariable("$aa", "222");
            variables["$aab"] = new StoryboardVariable("$aab", "252");
            variables["$abb"] = new StoryboardVariable("$abb", "27");

            List<_Command> asdasd = new List<_Command>();
            
            var dd=storyboardReader.GetValues(0).ToList();
        }
    }
}
