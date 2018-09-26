using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser.Stream
{
    public class SectionReader : CharMemoryReader
    {
        private bool is_end = false;

        public override bool EndOfStream => is_end || base.EndOfStream;

        public SectionReader(ReadOnlyMemory<char> buffer) : base(buffer)
        {

        }

        public override ReadOnlyMemory<char> ReadLine()
        {
            while (!EndOfStream)
            {
                var mem = base.ReadLine();
                var m = mem.Span;

                if (m.TrimStart().StartsWith("//".ToArray()) || m.IsWhiteSpace())
                {

                }
                else if (m[0]=='['&&m[m.Length-1]==']')
                {
                    is_end = true;
                }
                else
                    return mem;
            }

            return default;
        }

        public IEnumerable<string> GetAllLines()
        {
            Seek(0, System.IO.SeekOrigin.Begin);
            is_end = false;

            while (!EndOfStream)
            {
                var line = ReadLine();

                if (line.Length!=0)
                {
                    yield return line.ToString();
                }
            }
        }


    }
}
