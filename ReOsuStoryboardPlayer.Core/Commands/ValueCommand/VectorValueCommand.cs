using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.PrimitiveValue;
using System.Collections.Generic;
using System.IO;

namespace ReOsuStoryboardPlayer.Core.Commands
{
    public abstract class VectorValueCommand : ValueCommand<Vector>
    {
        public override Vector CalculateValue(float normalize_value) => StartValue+(EndValue-StartValue)*normalize_value;

        public override void OnSerialize(BinaryWriter stream, Dictionary<string,uint> map)
        {
            base.OnSerialize(stream,map);

            StartValue.OnSerialize(stream, map);
            EndValue.OnSerialize(stream, map);
        }

        public override void OnDeserialize(BinaryReader stream, Dictionary<uint, string> map)
        {
            base.OnDeserialize(stream,map);

            StartValue.OnDeserialize(stream, map);
            EndValue.OnDeserialize(stream, map);
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