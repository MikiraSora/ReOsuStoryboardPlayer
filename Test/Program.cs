using ReOsuStoryBoardPlayer.Commands;
using ReOsuStoryBoardPlayer.Parser;
using ReOsuStoryBoardPlayer.Parser.Collection;
using ReOsuStoryBoardPlayer.Parser.Extension;
using ReOsuStoryBoardPlayer.Parser.Reader;
using ReOsuStoryBoardPlayer.Parser.SimpleOsuParser;
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
            var x = HitObjectParserHelper.ParseHitObjects(@"G:\SBTest\144171 Nekomata Master - Far east nightbird (kors k Remix)\Nekomata Master - Far east nightbird (kors k Remix) (jonathanlfj) [EruJazz's Beginner].osu");
        }
    }
}
