using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.PrimitiveValue;
using ReOsuStoryboardPlayer.Core.Serialization;
using System.IO;

namespace ReOsuStoryboardPlayer.Core.Commands
{
    public abstract class VectorValueCommand : ValueCommand<Vector>
    {
        public override Vector CalculateValue(float normalize_value) => StartValue+(EndValue-StartValue)*normalize_value;

        public override void OnSerialize(BinaryWriter stream, StringCacheTable cache)
        {
            base.OnSerialize(stream, cache);

            StartValue.OnSerialize(stream, cache);
            EndValue.OnSerialize(stream, cache);
        }

        public override void OnDeserialize(BinaryReader stream, StringCacheTable cache)
        {
            base.OnDeserialize(stream, cache);

            StartValue.OnDeserialize(stream, cache);
            EndValue.OnDeserialize(stream, cache);
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