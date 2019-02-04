using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Serialization;
using System;
using System.IO;

namespace ReOsuStoryboardPlayer.Core.Commands
{
    public abstract class Command : IComparable<Command>,IStoryboardSerializable
    {
#if DEBUG
        public bool IsExecuted = false;
#endif

        public long RelativeLine = -1;

        public Event Event;

        public int StartTime;

        public int EndTime;

        public int CompareTo(Command b)
        {
            var cmp = StartTime.CompareTo(b.StartTime);
            return cmp==0 ? RelativeLine.CompareTo(b.RelativeLine) : cmp;
        }

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

        public virtual void OnSerialize(BinaryWriter stream)
        {
            //this is read by CommandDeserializtionFactory::Create()
            ((int)Event).OnSerialize(stream);

            RelativeLine.OnSerialize(stream);
            StartTime.OnSerialize(stream);
            EndTime.OnSerialize(stream);
        }

        public virtual void OnDeserialize(BinaryReader stream)
        {
            RelativeLine.OnDeserialize(stream);
            StartTime.OnDeserialize(stream);
            EndTime.OnDeserialize(stream);
        }
    }
}