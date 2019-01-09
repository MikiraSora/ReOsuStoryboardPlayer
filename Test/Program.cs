using ReOsuStoryBoardPlayer.BeatmapParser;
using ReOsuStoryBoardPlayer.Commands;
using ReOsuStoryBoardPlayer.Parser;
using ReOsuStoryBoardPlayer.Parser.Collection;
using ReOsuStoryBoardPlayer.Parser.Extension;
using ReOsuStoryBoardPlayer.Parser.Reader;
using ReOsuStoryBoardPlayer.Parser.Stream;
using StorybrewCommon.Mapset;
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
using static ReOsuStoryBoardPlayer.Commands.Group.Trigger.TriggerCondition.HitSoundTriggerCondition;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var q= HitSoundInfosHelpers.Parse(@"G:\SBTest\440423 Kushi - Yuumeikyou o Wakatsu Koto\Kushi - Yuumeikyou o Wakatsu Koto (09kami) [Yuumei].osu");
        }
    }
}
