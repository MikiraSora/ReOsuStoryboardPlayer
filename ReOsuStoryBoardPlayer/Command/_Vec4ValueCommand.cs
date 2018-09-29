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
    }
}
