using ReOsuStoryBoardPlayer.Parser.Stream;
using System.Collections.Generic;

namespace ReOsuStoryBoardPlayer.Parser.Reader
{
    internal class SectionReader : IReader<string>
    {
        private readonly Section section;
        private readonly OsuFileReader reader;

        public int FileLine => reader.FileLine;

        public SectionReader(Section section, OsuFileReader reader)
        {
            this.section = section;
            this.reader = reader;
        }

        public IEnumerable<string> EnumValues()
        {
            if (!reader.JumpSectionContent(section))
                yield break;

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                if (line.Length > 2 && line[0] == '[' && line[line.Length - 1] == ']')
                    break;

                yield return line;
            }
        }
    }
}