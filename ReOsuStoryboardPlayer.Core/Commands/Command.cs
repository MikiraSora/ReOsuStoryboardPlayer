using ReOsuStoryboardPlayer.Core.Base;
using System;

namespace ReOsuStoryboardPlayer.Core.Commands
{
    public abstract class Command : IComparable<Command>
    {
#if DEBUG
        public bool IsExecuted = false;
#endif

        public long RelativeLine = -1;

        public Event Event;

        public int StartTime;

        public int EndTime;

        public int CompareTo(Command other) => StartTime-other.StartTime;

        public abstract void Execute(StoryboardObject @object, float time);

        public override string ToString() => $"rline {RelativeLine}: {Event.ToString()} ({StartTime}~{EndTime})";

        public bool IsCommandConflict(Command b)
        {
            var a = this;

            /*
             |---------| a
                |------------------| b
             */
            if (b.StartTime<=a.StartTime)
            {
                var t = a;
                a=b;
                b=t;
            }

            return b.StartTime<a.EndTime;
        }
    }
}