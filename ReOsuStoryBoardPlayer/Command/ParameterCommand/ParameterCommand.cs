﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Commands
{
    public class HorizonFlipCommand : StateCommand
    {
        public HorizonFlipCommand() => Event = Event.HorizonFlip;

        public override void ApplyValue(StoryBoardObject @object, bool value) => @object.IsHorizonFlip = value;
    }

    public class VerticalFlipCommand : StateCommand
    {
        public VerticalFlipCommand() => Event = Event.VerticalFlip;

        public override void ApplyValue(StoryBoardObject @object, bool value) => @object.IsVerticalFlip = value;
    }

    public class AdditiveBlendCommand : StateCommand
    {
        public AdditiveBlendCommand() => Event = Event.AdditiveBlend;

        public override void ApplyValue(StoryBoardObject @object, bool value) => @object.IsAdditive = value;
    }
}