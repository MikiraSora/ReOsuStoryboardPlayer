using ReOsuStoryboardPlayer.Core.Base;

namespace ReOsuStoryboardPlayer.Core.Commands
{
    public class HorizonFlipCommand : StateCommand
    {
        public HorizonFlipCommand() => Event=Event.HorizonFlip;

        public override void ApplyValue(StoryboardObject @object, bool value) => @object.IsHorizonFlip=value;
    }

    public class VerticalFlipCommand : StateCommand
    {
        public VerticalFlipCommand() => Event=Event.VerticalFlip;

        public override void ApplyValue(StoryboardObject @object, bool value) => @object.IsVerticalFlip=value;
    }

    public class AdditiveBlendCommand : StateCommand
    {
        public AdditiveBlendCommand() => Event=Event.AdditiveBlend;

        public override void ApplyValue(StoryboardObject @object, bool value) => @object.IsAdditive=value;
    }
}