using ReOsuStoryBoardPlayer.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Commands
{
    public abstract class _VectorValueCommand : _ValueCommand<Vector>
    {
        public override Vector CalculateValue(float normalize_value) => StartValue + (EndValue - StartValue) * normalize_value;
    }

    public class _MoveCommand : _VectorValueCommand
    {
        public _MoveCommand() => Event = Event.Move;

        public override void ApplyValue(StoryBoardObject @object, Vector value) => @object.Postion = value;
    }

    public class _VectorScaleCommand : _VectorValueCommand
    {
        public _VectorScaleCommand() => Event = Event.VectorScale;

        public override void ApplyValue(StoryBoardObject @object, Vector value) => @object.Scale = value;
    }
}
