﻿using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Serialization;
using ReOsuStoryboardPlayer.Core.Serialization.DeserializationFactory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ReOsuStoryboardPlayer.Core.Commands.Group.Trigger
{
    public class TriggerCommand : GroupCommand
    {
        public TriggerConditionBase Condition { get; set; }

        private Dictionary<Event, TriggerSubTimelineCommand> cache_timline_wrapper = new Dictionary<Event, TriggerSubTimelineCommand>();

        private StoryboardObject bind_object;

        public int GroupID;

        public bool Trigged { get; private set; }

        private float last_trigged_time = 0;

        public TriggerCommand()
        {
            Event=Event.Trigger;
        }

        public override void Execute(StoryboardObject @object, float time)
        {
            if (Trigged)
            {
                //executed,recovery status and reset
                if (last_trigged_time+CostTime<=time)
                {
                    Trigged=false;
                    Reset(true);
                }
            }
        }

        /// <summary>
        /// 钦定触发器绑定的物件，一次性的
        /// </summary>
        /// <param name="obj"></param>
        public void BindObject(StoryboardObject obj)
        {
            Debug.Assert(bind_object==null, "Not allow trigger command bind more Storyboard objects");

            bind_object=obj??throw new ArgumentNullException(nameof(obj));
        }

        /// <summary>
        /// 钦定触发器激活，并将子命令塞到物件里面去
        /// </summary>
        /// <param name="time"></param>
        public void Trig(float time)
        {
            Reset(true);
            last_trigged_time=time;

            AttachSubCommands(time);

            bind_object.CalculateAndApplyBaseFrameTime();

            Trigged=true;
        }

        /// <summary>
        /// 将子命令塞到物件那里执行
        /// </summary>
        /// <param name="time"></param>
        private void AttachSubCommands(float time)
        {
            foreach (var wrapper in cache_timline_wrapper)
            {
                bind_object.RemoveCommand(wrapper.Value);
                wrapper.Value.UpdateOffset((int)time);
                bind_object.AddCommand(wrapper.Value);
            }
        }

        /// <summary>
        /// 清除物件内自己的子命令
        /// </summary>
        /// <param name="magic"></param>
        private void DetachSubCommands(bool magic = false)
        {
            foreach (var wrapper in cache_timline_wrapper.Where(x => !magic||(x.Value.StartTime==x.Value.EndTime&&x.Value.StartTime==0)))
                bind_object.RemoveCommand(wrapper.Value);
        }

        /// <summary>
        /// 清除物件内自己的子命令，重置触发器状态
        /// </summary>
        /// <param name="magic">是否要也要清除物件内，同Group组所有Trigger子命令，
        /// 为啥是magic呢，因为我看不出这有啥实际意义，而且现在屙屎的Trigger逻辑细节和以前不同，也没文档跟上，只能按照基本法膜法一下了</param>
        public void Reset(bool magic = false)
        {
            DetachSubCommands(magic);

            last_trigged_time=0;

            Trigged=false;
        }

        /// <summary>
        /// 检查时间是否处于触发器可触发时间内
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool CheckTimeVaild(float time)
        {
            return StartTime<=time&&time<=EndTime;
        }

        public override void UpdateSubCommand()
        {
            CostTime=SubCommands.Values.SelectMany(l => l).Max(p => p.EndTime);

            cache_timline_wrapper.Clear();

            foreach (var timeline in SubCommands)
                cache_timline_wrapper[timeline.Key]=new TriggerSubTimelineCommand(this, timeline.Key);
        }

        public override string ToString() => $"{base.ToString()} {Condition} {(Trigged ? $"Trigged at {last_trigged_time} ~ end:{last_trigged_time+CostTime}" : "")}";

        /// <summary>
        /// 有触发器的物件默认不显示(之前默认显示),不懂ppy想法，magic
        /// 比如440423的歌词
        /// </summary>
        public readonly static Action<StoryboardObject> OverrideDefaultValue = obj => obj.Color.W=0;

        public override void OnSerialize(BinaryWriter stream, StringCacheTable cache)
        {
            base.OnSerialize(stream, cache);

            GroupID.OnSerialize(stream);
            last_trigged_time.OnSerialize(stream);

            Condition.OnSerialize(stream, cache);
        }

        public override void OnDeserialize(BinaryReader stream, StringCacheTable cache)
        {
            base.OnDeserialize(stream, cache);

            GroupID.OnDeserialize(stream);
            last_trigged_time.OnDeserialize(stream);

            Condition=TriggerConditionDeserializationFactory.Create(stream, cache);

            UpdateSubCommand();
        }

        public override bool Equals(Command command)
        {
            return base.Equals(command)
                &&command is TriggerCommand triger
                &&triger.Condition.Equals(Condition)
                &&triger.GroupID==GroupID
                &&triger.CostTime==CostTime;
        }
    }
}