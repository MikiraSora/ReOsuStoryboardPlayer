using System;

namespace ReOsuStoryBoardPlayer.Commands
{
    internal class ColorCommand : ValueCommand<ByteVec4>
    {
        public ColorCommand() => Event = Event.Color;

        public override void ApplyValue(StoryBoardObject @object, ByteVec4 value)
        {
            @object.Color.x = value.x;
            @object.Color.y = value.y;
            @object.Color.z = value.z;
        }

        public override ByteVec4 CalculateValue(float normalize_value)
        {
            //ByteVec4 Distance = EndValue-StartValue;

            var dx = EndValue.x-StartValue.x;
            var dy = EndValue.y-StartValue.y;
            var dz = EndValue.z-StartValue.z;
            //var dw = EndValue.w-StartValue.w;

            ByteVec4 temp = new ByteVec4();
            temp.x=(byte)Math.Max(0, Math.Min((StartValue.x+dx*normalize_value), 255));
            temp.y=(byte)Math.Max(0, Math.Min((StartValue.y+dy*normalize_value), 255));
            temp.z=(byte)Math.Max(0, Math.Min((StartValue.z+dz*normalize_value), 255));

            return temp;
        }
    }
}