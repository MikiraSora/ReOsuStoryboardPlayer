using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Serialization;
using System.IO;

namespace ReOsuStoryboardPlayer.Core.Commands
{
    public abstract class FloatValueCommand : ValueCommand<float>
    {
        public override float CalculateValue(float normalize_value) => StartValue+(EndValue-StartValue)*normalize_value;

        public override void OnSerialize(BinaryWriter stream)
        {
            base.OnSerialize(stream);

            StartValue.OnSerialize(stream);
            EndValue.OnSerialize(stream);
        }

        public override void OnDeserialize(BinaryReader stream)
        {
            base.OnDeserialize(stream);

            var v= StartValue;
            v.OnDeserialize(stream); StartValue=v;
            v.OnDeserialize(stream); EndValue=v;
        }
    }

    public class FadeCommand : FloatValueCommand
    {
        public FadeCommand() => Event=Event.Fade;

        public override void ApplyValue(StoryboardObject @object, float value) => @object.Color.W=(byte)(value*255);
    }

    public class MoveXCommand : FloatValueCommand
    {
        public MoveXCommand() => Event=Event.MoveX;

        public override void ApplyValue(StoryboardObject @object, float value) => @object.Postion.X=value;
    }

    public class MoveYCommand : FloatValueCommand
    {
        public MoveYCommand() => Event=Event.MoveY;

        public override void ApplyValue(StoryboardObject @object, float value) => @object.Postion.Y=value;
    }

    public class RotateCommand : FloatValueCommand
    {
        public RotateCommand() => Event=Event.Rotate;

        public override void ApplyValue(StoryboardObject @object, float value) => @object.Rotate=value;
    }

    public class ScaleCommand : FloatValueCommand
    {
        public ScaleCommand() => Event=Event.Scale;

        public override void ApplyValue(StoryboardObject @object, float value) => @object.Scale.X=@object.Scale.Y=value;
    }
}