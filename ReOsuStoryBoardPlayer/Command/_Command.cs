using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Commands
{
    public abstract class _Command:IComparable<_Command>
    {
#if DEBUG
        internal bool IsExecuted { get; set; } = false;
#endif

        public Event Event { get; set; }

        public int StartTime { get; set; }

        public int EndTime { get; set; }

        public int CompareTo(_Command other) => StartTime - other.StartTime;

        public abstract void Execute(StoryBoardObject @object, float time);

        public override string ToString() => $"{Event.ToString()} ({StartTime}~{EndTime})";
    }
}
