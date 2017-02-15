using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryBroadParser
{
    public enum Events
    {
        Move=0,
        Fade=1,
        Scale=2,
        VectorScale=3,
        Rotate=4,
        Color=5,
        Parameter=6,
        MoveX=7,
        MoveY=8,
        Loop=9,
        Trigger=10
    }
}
