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
    }
}