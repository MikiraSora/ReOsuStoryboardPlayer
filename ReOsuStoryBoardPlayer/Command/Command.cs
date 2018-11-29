using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Commands
{
    public abstract class Command:IComparable<Command>
    {
#if DEBUG
        internal bool IsExecuted = false;
#endif

        public Event Event;

        public int StartTime;

        public int EndTime;

        public int CompareTo(Command other) => StartTime - other.StartTime;

        public abstract void Execute(StoryBoardObject @object, float time);

        public override string ToString() => $"{Event.ToString()} ({StartTime}~{EndTime})";
    }
}
