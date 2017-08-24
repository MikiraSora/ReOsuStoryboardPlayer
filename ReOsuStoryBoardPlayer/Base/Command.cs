using SimpleRenderFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer
{
    public class Command 
    {
        public Event CommandEventType;
        public EasingInterpolator Easing;
        public int StartTime, EndTime;
        public CommandParameters Parameters;

        public CommandExecutor.CommandFunc executor;

        public override string ToString() => $"{CommandEventType.ToString()},{Easing.ToString()},{StartTime},{EndTime}";
    }
}
