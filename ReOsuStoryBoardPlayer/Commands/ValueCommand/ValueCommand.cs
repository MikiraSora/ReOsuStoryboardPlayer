using System.Collections;
using System.Collections.Generic;

namespace ReOsuStoryBoardPlayer.Commands
{
    public abstract class ValueCommand : Command
    {
        public EasingTypes Easing { get; set; }

        public abstract IEqualityComparer EqualityComparer { get; }

        #region Evil Methods

        public object GetStartValue() => GetType().GetProperty("StartValue").GetValue(this);
        public object GetEndValue() => GetType().GetProperty("EndValue").GetValue(this);

        public T GetEndValue<T>() => (T)GetEndValue();
        public T GetStartValue<T>() => (T)GetStartValue();

        #endregion
    }

    public abstract class ValueCommand<VALUE_TYPE> : ValueCommand
    {
        public abstract void ApplyValue(StoryBoardObject @object, VALUE_TYPE value);

        public override IEqualityComparer EqualityComparer => EqualityComparer<VALUE_TYPE>.Default;

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
                return (float)Interpolation.ApplyEasing(Easing,(time-StartTime)/(EndTime-StartTime));
        }

        public override void Execute(StoryBoardObject @object, float time)
        {
            var val = CalculateValue(CalculateNormalizeValue(time));

            ApplyValue(@object, val);
        }

        public override string ToString() => $"{base.ToString()} {Easing.ToString()} ({StartValue}~{EndValue})";
    }
}