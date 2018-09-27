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
            var chars = new ReadOnlyMemory<char>(File.ReadAllText("m.osb").ToCharArray());

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
            
            var test = ",$zz,is,$aa$aa$zz,2,,from,$abb,";

            int i = 0;

            foreach (var sub in test.AsMemory().Split(',',StringSplitOptions.None))
                Console.WriteLine($"{i++}:"+sub+"|");

            var zz=EnumParser<Section>.Instance.Parse(Section.Metadata.ToString());
        }
    }
}
