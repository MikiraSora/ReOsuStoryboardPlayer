using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Serialization;
using System;
using System.IO;

namespace ReOsuStoryboardPlayer.Core.Commands
{
    public abstract class Command : IComparable<Command>, IStoryboardSerializable, IEquatable<Command>
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

        public virtual void OnSerialize(BinaryWriter stream, StringCacheTable _)
        {
            //this is read by CommandDeserializtionFactory::Create()
            ((int)Event).OnSerialize(stream);

            RelativeLine.OnSerialize(stream);
            StartTime.OnSerialize(stream);
            EndTime.OnSerialize(stream);
        }

        public virtual void OnDeserialize(BinaryReader stream, StringCacheTable _)
        {
            RelativeLine.OnDeserialize(stream);
            StartTime.OnDeserialize(stream);
            EndTime.OnDeserialize(stream);
        }

        /// <summary>
        /// 指引用不同的两个命令对象是否内容完全相同
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public virtual bool Equals(Command command)
        {
            return Event==command.Event
                   &&StartTime==command.StartTime
                   &&EndTime==command.EndTime;
        }
    }
}