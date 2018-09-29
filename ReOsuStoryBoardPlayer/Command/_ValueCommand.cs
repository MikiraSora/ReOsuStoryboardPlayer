using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Commands
{
    public abstract class _ValueCommand<VALUE_TYPE>:_Command
    {
        public abstract void ApplyValue(StoryBoardObject @object, VALUE_TYPE value);

        public EasingInterpolator Easing { get; set; }

        public VALUE_TYPE StartValue { get; set; }

        public VALUE_TYPE EndValue { get; set; }

        public VALUE_TYPE CalculateValue(float time)
        {
            //todo
            return default;
        }

        public override void Execute(StoryBoardObject @object, float time)
        {
            if (!@object.CommandConflictChecker.CheckIfConflictThenUpdate(this,time))
                return;

            var val = CalculateValue(time);

            ApplyValue(@object, val);
        }

        public override string ToString() => $"{base.ToString()} {Easing.ToString()} ({StartValue}~{EndValue})";
    }
}
