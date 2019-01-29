﻿using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.PrimitiveValue;
using System;

namespace ReOsuStoryboardPlayer.Core.Commands
{
    internal class ColorCommand : ValueCommand<ByteVec4>
    {
        public ColorCommand() => Event=Event.Color;

        public override void ApplyValue(StoryboardObject @object, ByteVec4 value)
        {
            @object.Color.X=value.X;
            @object.Color.Y=value.Y;
            @object.Color.Z=value.Z;
        }

        public override ByteVec4 CalculateValue(float normalize_value)
        {
            //ByteVec4 Distance = EndValue-StartValue;

            var dx = EndValue.X-StartValue.X;
            var dy = EndValue.Y-StartValue.Y;
            var dz = EndValue.Z-StartValue.Z;
            //var dw = EndValue.w-StartValue.w;

            ByteVec4 temp = new ByteVec4
            {
                X=(byte)Math.Max(0, Math.Min((StartValue.X+dx*normalize_value), 255)),
                Y=(byte)Math.Max(0, Math.Min((StartValue.Y+dy*normalize_value), 255)),
                Z=(byte)Math.Max(0, Math.Min((StartValue.Z+dz*normalize_value), 255))
            };

            return temp;
        }
    }
}