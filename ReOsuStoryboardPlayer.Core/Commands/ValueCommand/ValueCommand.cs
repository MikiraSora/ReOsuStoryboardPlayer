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

        public object GetStartValue() => GetType().GetProperty("StartValue").GetValue(this);

        public object GetEndValue() => GetType().GetProperty("EndValue").GetValue(this);

        public T GetEndValue<T>() => (T)GetEndValue();

        public T GetStartValue<T>() => (T)GetStartValue();

        #endregion Evil Methods

        public override void OnSerialize(BinaryWriter stream, Dictionary<string,uint> map)
        {
            base.OnSerialize(stream,map);

            ((byte)Easing).OnSerialize(stream);
        }

        public override void OnDeserialize(BinaryReader stream, Dictionary<uint, string> map)
        {
            base.OnDeserialize(stream,map);

            Easing=(EasingTypes)stream.ReadByte();
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
    }
}