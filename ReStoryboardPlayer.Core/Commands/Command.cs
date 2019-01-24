﻿using ReOsuStoryBoardPlayer.Core.Base;
using System;

namespace ReOsuStoryBoardPlayer.Core.Commands
{
    public abstract class Command : IComparable<Command>
    {
#if DEBUG
        internal bool IsExecuted = false;
#endif

        public long RelativeLine = -1;

        public Event Event;

        public int StartTime;

        public int EndTime;

        public int CompareTo(Command other) => StartTime-other.StartTime;

        public abstract void Execute(StoryBoardObject @object, float time);

        public override string ToString() => $"rline {RelativeLine}: {Event.ToString()} ({StartTime}~{EndTime})";
    }
}