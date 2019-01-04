using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser.SimpleOsuParser
{
    [Flags]
    public enum HitObjectType
    {
        Normal = 1,
        Slider = 2,
        NewCombo = 4,
        NormalNewCombo = 5,
        SliderNewCombo = 6,
        Spinner = 8,
        ColourHax = 112,
        Hold = 128,
        ManiaLong = 128
    };
}
