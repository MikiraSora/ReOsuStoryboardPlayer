using ReOsuStoryBoardPlayer.Parser;
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

            foreach (var obj in storyboardReader.GetValues(3))
            {
                obj.ImageFilePath = "";
            }

            Console.ReadLine();
        }
    }
}
