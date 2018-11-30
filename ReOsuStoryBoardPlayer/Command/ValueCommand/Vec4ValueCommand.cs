using System;

namespace ReOsuStoryBoardPlayer.Commands
{
    internal class ColorCommand : ValueCommand<Vec4>
    {
        public ColorCommand() => Event = Event.Color;

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
            temp.x = Math.Max(0, Math.Min((StartValue.x + Distance.x * normalize_value), 255)) / 255.0f;
            temp.y = Math.Max(0, Math.Min((StartValue.y + Distance.y * normalize_value), 255)) / 255.0f;
            temp.z = Math.Max(0, Math.Min((StartValue.z + Distance.z * normalize_value), 255)) / 255.0f;

            return temp;
        }
    }
}