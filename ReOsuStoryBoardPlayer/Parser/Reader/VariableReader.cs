using ReOsuStoryBoardPlayer.Parser.Stream;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReOsuStoryBoardPlayer.Parser.Reader
{
    public class VariableReader : IReader<StoryboardVariable>
    {
        private readonly SectionReader reader;

        public VariableReader(OsuFileReader reader)
        {
            this.reader = new SectionReader(Section.Variables, reader);
        }

        public IEnumerable<StoryboardVariable> EnumValues()
        {
            foreach (var line in reader.EnumValues())
            {
                var arr = line.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length != 2)
                    continue;

                var variable = new StoryboardVariable();
                variable.Name = arr.First();
                variable.Value = arr.Last();

                yield return variable;
            }
        }
    }
}