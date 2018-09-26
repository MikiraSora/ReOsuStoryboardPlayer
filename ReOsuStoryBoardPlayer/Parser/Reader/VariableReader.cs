using ReOsuStoryBoardPlayer.Parser.Stream;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser.Reader
{
    public class VariableReader : IReader<StoryboardVariable>
    {
        private readonly SectionReader reader;

        public bool IsEnd => reader.EndOfStream;

        public VariableReader(SectionReader reader)
        {
            this.reader = reader;
        }

        public IEnumerable<StoryboardVariable> GetValues()
        {
            foreach (var line in reader.GetAllLines())
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
