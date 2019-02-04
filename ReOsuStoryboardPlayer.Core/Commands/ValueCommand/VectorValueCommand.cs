using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.PrimitiveValue;
using System.IO;

namespace ReOsuStoryboardPlayer.Core.Commands
{
    public abstract class VectorValueCommand : ValueCommand<Vector>
    {
        public override Vector CalculateValue(float normalize_value) => StartValue+(EndValue-StartValue)*normalize_value;

        public override void OnSerialize(BinaryWriter stream)
        {
            base.OnSerialize(stream);

            StartValue.OnSerialize(stream);
            EndValue.OnSerialize(stream);
        }

        public override void OnDeserialize(BinaryReader stream)
        {
            base.OnDeserialize(stream);

            StartValue.OnDeserialize(stream);
            EndValue.OnDeserialize(stream);
        }
    }

    public class MoveCommand : VectorValueCommand
    {
        public MoveCommand() => Event=Event.Move;

        public override void ApplyValue(StoryboardObject @object, Vector value) => @object.Postion=value;
    }

    public class VectorScaleCommand : VectorValueCommand
    {
        public VectorScaleCommand() => Event=Event.VectorScale;

        public override void ApplyValue(StoryboardObject @object, Vector value) => @object.Scale=value;
    }
}