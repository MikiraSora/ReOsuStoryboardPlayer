using ReOsuStoryBoardPlayer.Commands;
using ReOsuStoryBoardPlayer.Parser;
using ReOsuStoryBoardPlayer.Parser.Collection;
using ReOsuStoryBoardPlayer.Parser.Extension;
using ReOsuStoryBoardPlayer.Parser.Reader;
using ReOsuStoryBoardPlayer.Parser.Stream;
using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var reader = new ByteMemoryReader(File.ReadAllBytes("2d.osb"));

            while (!reader.EndOfStream)
            {
                Console.WriteLine(Encoding.UTF8.GetString(reader.ReadLine().ToArray()));
            }

            Console.ReadLine();
        }
    }
}
