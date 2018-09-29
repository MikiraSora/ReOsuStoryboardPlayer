using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Commands
{
    public class _FadeCommand : _ValueCommand<float>
    {
        public _FadeCommand() => Event = Event.Fade;

        public override void ApplyValue(StoryBoardObject @object, float value) => @object.Color.w = value;
    }

    public class _MoveXCommand : _ValueCommand<float>
    {
        public _MoveXCommand() => Event = Event.MoveX;

        public override void ApplyValue(StoryBoardObject @object, float value) => @object.Postion.x = value;
    }

    public class _MoveYCommand : _ValueCommand<float>
    {
        public _MoveYCommand() => Event = Event.MoveY;

        public override void ApplyValue(StoryBoardObject @object, float value) => @object.Postion.y = value;
    }

    public class _RotateCommand : _ValueCommand<float>
    {
        public _RotateCommand() => Event = Event.Rotate;

        public override void ApplyValue(StoryBoardObject @object, float value) => @object.Rotate = value;
    }

    public class _ScaleCommand : _ValueCommand<float>
    {
        public _ScaleCommand() => Event = Event.Scale;

        public override void ApplyValue(StoryBoardObject @object, float value) => @object.Scale.x = @object.Scale.y = value;
    }
}
