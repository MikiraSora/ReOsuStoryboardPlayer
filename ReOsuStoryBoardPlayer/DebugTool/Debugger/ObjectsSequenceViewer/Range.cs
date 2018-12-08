using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.DebugTool.Debugger.ObjectsSequenceViewer
{
    public struct Range
    {
        public int End;
        public int Start;

        public bool InRange(int cur) => cur>=Start&&cur<=End;
    }
}
