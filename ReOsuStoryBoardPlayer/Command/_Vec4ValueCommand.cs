using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Commands
{
    class _ColorCommand : _ValueCommand<Vec4>
    {
        public _ColorCommand() => Event = Event.Color;

        public override void ApplyValue(StoryBoardObject @object, Vec4 value)
        {
            @object.Color.x = value.x;
            @object.Color.y = value.y;
            @object.Color.z = value.z;
        }

        public override Vec4 CalculateValue(float normalize_value)
        {
            Vec4 Distance = EndValue - StartValue;

            Vec4 temp = new Vec4();
            temp.x = Math.Max(0, Math.Min((StartValue.x + Distance.x * normalize_value), 1));
            temp.y = Math.Max(0, Math.Min((StartValue.y + Distance.y * normalize_value), 1));
            temp.z = Math.Max(0, Math.Min((StartValue.z + Distance.z * normalize_value), 1));

            return temp;
        }
    }
}
