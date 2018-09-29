using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Commands
{
    public class _HorizonFlipCommand : _StateCommand
    {
        public _HorizonFlipCommand() => Event = Event.HorizonFlip;

        public override void ApplyValue(StoryBoardObject @object, bool value) => @object.IsHorizonFlip = value;
    }

    public class _VerticalFlipCommand : _StateCommand
    {
        public _VerticalFlipCommand() => Event = Event.VerticalFlip;

        public override void ApplyValue(StoryBoardObject @object, bool value) => @object.IsVerticalFlip = value;
    }

    public class _AdditiveBlendCommand : _StateCommand
    {
        public _AdditiveBlendCommand() => Event = Event.AdditiveBlend;

        public override void ApplyValue(StoryBoardObject @object, bool value) => @object.IsAdditive = value;
    }
}
