namespace ReOsuStoryBoardPlayer.Commands
{
    public abstract class ValueCommand : Command
    {
        public EasingInterpolator Easing { get; set; }
    }

    public abstract class ValueCommand<VALUE_TYPE> : ValueCommand
    {
        public abstract void ApplyValue(StoryBoardObject @object, VALUE_TYPE value);

        public VALUE_TYPE StartValue { get; set; }

        public VALUE_TYPE EndValue { get; set; }

        public abstract VALUE_TYPE CalculateValue(float normalize_value);

        private float CalculateNormalizeValue(float time)
        {
            if (time <= StartTime)
                return 0;
            else if (time >= EndTime)
                return 1;
            else
                return Easing.calculate(time - StartTime, StartTime, EndTime);
        }

        public override void Execute(StoryBoardObject @object, float time)
        {
            var val = CalculateValue(CalculateNormalizeValue(time));

            ApplyValue(@object, val);
        }

        public override string ToString() => $"{base.ToString()} {Easing.ToString()} ({StartValue}~{EndValue})";
    }
}