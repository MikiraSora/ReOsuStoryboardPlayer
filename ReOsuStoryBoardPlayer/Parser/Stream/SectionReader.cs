using ReOsuStoryBoardPlayer.Parser.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser.Stream
{
    public class SectionReader : ByteMemoryReader
    {
        private bool is_end = false;

        public override bool EndOfStream => is_end || base.EndOfStream;

        public SectionReader(ReadOnlyMemory<byte> buffer) : base(buffer)
        {

        }

        public override ReadOnlyMemory<byte> ReadLine()
        {
            while (!EndOfStream)
            {
                var mem = base.ReadLine();
                var m = mem.Span;

                if (!mem.CheckLineValid())
                {

                }
                else if (m[0] == '[' && m[m.Length - 1] == ']')
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
