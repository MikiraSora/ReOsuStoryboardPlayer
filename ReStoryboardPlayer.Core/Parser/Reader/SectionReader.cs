using ReOsuStoryBoardPlayer.Core.Parser.Stream;
using System.Collections.Generic;
using System.Linq;

namespace ReOsuStoryBoardPlayer.Core.Parser.Reader
{
    public class SectionReader : IReader<string>
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

        readonly static char[] split = new[] { ':' };
        public string ReadProperty(string name)
        {
            foreach (var line in EnumValues().Where(x=>x.StartsWith(name)))
            {
                var data = line.Split(split, 2).Select(x=> {
                    x.Trim();
                    return x;
                });

                if (data.Count()==2&&data.ElementAt(0)==name)
                    return data.ElementAt(1).Trim();
            }

            return string.Empty;
        }
    }
}