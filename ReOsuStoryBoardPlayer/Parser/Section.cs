using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser
{
    public enum Section
    {
        Unknown = 0,
        General = 1,
        Colours = 2,
        Editor = 4,
        Metadata = 8,
        TimingPoints = 16,
        Events = 32,
        HitObjects = 64,
        Difficulty = 128,
        Variables = 256
    }
}
