using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Commands.Group.Trigger.TriggerCondition
{
    [Flags]
    public enum HitObjectSoundType
    {
        None = 0,
        Normal = 1,
        Whistle = 2,
        Finish = 4,
        Clap = 8
    };
}
