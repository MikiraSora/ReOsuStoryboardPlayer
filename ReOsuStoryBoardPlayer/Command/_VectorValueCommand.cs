using ReOsuStoryBoardPlayer.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Commands
{
    public class _MoveCommand : _ValueCommand<Vector>
    {
        public _MoveCommand() => Event = Event.Move;

        public override void ApplyValue(StoryBoardObject @object, Vector value) => @object.Postion = value;
    }

    public class _VectorScaleCommand : _ValueCommand<Vector>
    {
        public _VectorScaleCommand() => Event = Event.VectorScale;

        public override void ApplyValue(StoryBoardObject @object, Vector value) => @object.Scale = value;
    }
}
