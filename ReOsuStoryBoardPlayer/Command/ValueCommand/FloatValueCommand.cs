namespace ReOsuStoryBoardPlayer.Commands
{
    public abstract class FloatValueCommand : ValueCommand<float>
    {
        public override float CalculateValue(float normalize_value) => StartValue + (EndValue - StartValue) * normalize_value;
    }

    public class FadeCommand : FloatValueCommand
    {
        public FadeCommand() => Event = Event.Fade;

        public override void ApplyValue(StoryBoardObject @object, float value) => @object.Color.w = value;
    }

    public class MoveXCommand : FloatValueCommand
    {
        public MoveXCommand() => Event = Event.MoveX;

        public override void ApplyValue(StoryBoardObject @object, float value) => @object.Postion.x = value;
    }

    public class MoveYCommand : FloatValueCommand
    {
        public MoveYCommand() => Event = Event.MoveY;

        public override void ApplyValue(StoryBoardObject @object, float value) => @object.Postion.y = value;
    }

    public class RotateCommand : FloatValueCommand
    {
        public RotateCommand() => Event = Event.Rotate;

        public override void ApplyValue(StoryBoardObject @object, float value) => @object.Rotate = -value;
    }

    public class ScaleCommand : FloatValueCommand
    {
        public ScaleCommand() => Event = Event.Scale;

        public override void ApplyValue(StoryBoardObject @object, float value) => @object.Scale.x = @object.Scale.y = value;
    }
}