using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ReOsuStoryboardPlayer.Core.Commands
{
    public abstract class ValueCommand : Command
    {
        public EasingTypes Easing { get; set; }

        public abstract IEqualityComparer EqualityComparer { get; }

        #region Evil Methods

        public object GetStartValue() => GetType().GetField("StartValue").GetValue(this);

        public object GetEndValue() => GetType().GetField("EndValue").GetValue(this);

        public T GetEndValue<T>() => (T)GetEndValue();

        public T GetStartValue<T>() => (T)GetStartValue();

        #endregion Evil Methods

        public override void OnSerialize(BinaryWriter stream, StringCacheTable cache)
        {
            base.OnSerialize(stream,cache);

            ((byte)Easing).OnSerialize(stream);
        }

        public override void OnDeserialize(BinaryReader stream, StringCacheTable cache)
        {
            base.OnDeserialize(stream,cache);

            Easing=(EasingTypes)stream.ReadByte();
        }

        public override bool Equals(Command command)
        {
            return base.Equals(command)&&Easing==((ValueCommand)command).Easing;
        }
    }

    public abstract class ValueCommand<VALUE_TYPE> : ValueCommand
    {
        public abstract void ApplyValue(StoryboardObject @object, VALUE_TYPE value);

        public override IEqualityComparer EqualityComparer => EqualityComparer<VALUE_TYPE>.Default;

        public VALUE_TYPE StartValue;

        public VALUE_TYPE EndValue;

        public abstract VALUE_TYPE CalculateValue(float normalize_value);

        private float CalculateNormalizeValue(float time)
        {
            if (time<=StartTime)
                return 0;
            else if (time>=EndTime)
                return 1;
            else
                return (float)Interpolation.ApplyEasing(Easing, (time-StartTime)/(EndTime-StartTime));
        }

        public override void Execute(StoryboardObject @object, float time)
        {
            var val = CalculateValue(CalculateNormalizeValue(time));

            ApplyValue(@object, val);
        }

        public override string ToString() => $"{base.ToString()} {Easing.ToString()} ({StartValue}~{EndValue})";

        public override bool Equals(Command command)
        {
            return base.Equals(command)
                &&command is ValueCommand<VALUE_TYPE> v
                &&EqualityComparer.Equals(v.StartValue,StartValue)
                &&EqualityComparer.Equals(v.EndValue, EndValue);
        }
    }
}