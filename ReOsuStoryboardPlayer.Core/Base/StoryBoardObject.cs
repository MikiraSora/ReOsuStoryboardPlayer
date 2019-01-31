using ReOsuStoryboardPlayer.Core.Commands;
using ReOsuStoryboardPlayer.Core.Commands.Group;
using ReOsuStoryboardPlayer.Core.Commands.Group.Trigger;
using ReOsuStoryboardPlayer.Core.PrimitiveValue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ReOsuStoryboardPlayer.Core.Base
{
    public class StoryboardObject
    {
        public Dictionary<Event, CommandTimeline> CommandMap = new Dictionary<Event, CommandTimeline>();

        //表示此物件拥有的Trigger集合，Key为GroupID
        public Dictionary<int, HashSet<TriggerCommand>> Triggers = new Dictionary<int, HashSet<TriggerCommand>>();

        public string ImageFilePath;

        public bool FromOsbFile;

        /// <summary>
        /// 钦定这个物件的最初变换值，通过委托链可以覆盖初始值
        /// </summary>
        public Action<StoryboardObject> BaseTransformResetAction;

        public int FrameStartTime = int.MinValue, FrameEndTime;

        public Layout layout;

        public int Z = -1;

        public bool IsVisible { get; private set; }

        public bool ContainTrigger => CommandMap.ContainsKey(Event.Trigger);

        public bool ContainLoop => CommandMap.ContainsKey(Event.Loop);

        public bool ContainNonValueCommand => ContainLoop||ContainTrigger;

        #region Transform

        public Vector Postion, Scale;

        public ByteVec4 Color;

        public float Rotate;

        public HalfVector Anchor = new HalfVector(0f, 0f);

        public bool IsAdditive, IsHorizonFlip, IsVerticalFlip;

        #endregion Transform

        #region Add/Remove Command

        public void AddCommand(Command command)
        {
            switch (command)
            {
                case LoopCommand loop:
                    AddLoopCommand(loop);
                    break;

                case TriggerCommand trigger:
                    AddTriggerCommand(trigger);
                    break;

                default:
                    break;
            }

            if (!CommandMap.TryGetValue(command.Event, out var timeline))
                timeline=CommandMap[command.Event]=new CommandTimeline();

            timeline.Add(command);
        }

        private void AddLoopCommand(LoopCommand loop_command)
        {
            if (Setting.EnableLoopCommandExpand)
            {
                //将Loop命令各个类型的子命令时间轴封装成一个命令，并添加到物件本体各个时间轴上
                foreach (var cmd in loop_command.SubCommandExpand())
                    AddCommand(cmd);
            }
            else
            {
                //将Loop命令里面的子命令展开
                foreach (var @event in loop_command.SubCommands.Keys)
                {
                    var sub_command_wrapper = new LoopSubTimelineCommand(loop_command, @event);
                    AddCommand(sub_command_wrapper);
                }
            }
        }

        private void AddTriggerCommand(TriggerCommand trigger_command, bool insert = false)
        {
            if (!Triggers.TryGetValue(trigger_command.GroupID, out var list))
                Triggers[trigger_command.GroupID]=new HashSet<TriggerCommand>();

            Triggers[trigger_command.GroupID].Add(trigger_command);
            trigger_command.BindObject(this);
            TriggerListener.DefaultListener.Add(this);

            if (!CommandMap.TryGetValue(Event.Trigger, out var x)||x.Count==0)
                BaseTransformResetAction+=TriggerCommand.OverrideDefaultValue;
        }

        public void RemoveCommand(Command command)
        {
            switch (command)
            {
                case LoopCommand loop_command:
                    foreach (var t in CommandMap.Values)
                    {
                        var result = t/*.OfType<LoopSubTimelineCommand>()*/.Where(x => x.RelativeLine==loop_command.RelativeLine).ToArray();

                        foreach (var c in result)
                            t.Remove(c);
                    }
                    break;

                case TriggerCommand trigger_command:
                    Triggers[trigger_command.GroupID].Remove(trigger_command);

                    if (!Triggers.Values.SelectMany(l => l).Any())
                        TriggerListener.DefaultListener.Remove(this);
                    break;

                default:
                    break;
            }

            //删除无用的时间轴
            if (CommandMap.TryGetValue(command.Event, out var timeline))
            {
                timeline.Remove(command);

                if (timeline.Count==0)
                {
                    CommandMap.Remove(command.Event);

                    if (command.Event==Event.Trigger)
                        BaseTransformResetAction-=TriggerCommand.OverrideDefaultValue;
                }
            }
        }

        #endregion Add/Remove Command

        public StoryboardObject()
        {
            BaseTransformResetAction=(obj) =>
         {
             obj.Postion=new Vector(320, 240);
             obj.Scale=new Vector(1, 1);

             obj.Color=new ByteVec4(255, 255, 255, 255);

             obj.Rotate=0;

             obj.IsAdditive=false;
             obj.IsHorizonFlip=false;
             obj.IsVerticalFlip=false;
         };
        }

        public void ResetTransform() => BaseTransformResetAction(this);

        public virtual void Update(float current_time)
        {
#if DEBUG
            ExecutedCommands.ForEach(c => c.IsExecuted=false);
            ExecutedCommands.Clear();
#endif  

            foreach (var pair in CommandMap)
            {
                var timeline = pair.Value;
                var command = timeline.PickCommand(current_time);

                if (command==null)
                    continue;

                command.Execute(this, current_time);
#if DEBUG
                MarkCommandExecuted(command);
#endif
            }

            IsVisible=(Color.W!=0);
        }

#if DEBUG
        internal List<Command> ExecutedCommands = new List<Command>();

        public bool DebugShow = true;

        internal void MarkCommandExecuted(Command command, bool is_exec = true)
        {
            if (is_exec)
                ExecutedCommands.Add(command);
            else
                ExecutedCommands.Remove(command);

            command.IsExecuted=is_exec;
        }

#endif

        /// <summary>
        /// 计算物件的FrameTime
        /// (此方法必须确保计算出来的物件时间是基于命令的真实的有效时间，不能因为Trigger而提前计算，FrameStartTime必须是一次性算好固定的值(否则Scan炸了，理论上也没什么玩意可以变更此参数))
        /// </summary>
        public void CalculateAndApplyBaseFrameTime()
        {
            var commands = CommandMap.SelectMany(l => l.Value);

            if (commands.Count()==0)
                return;

            var start = commands.Min(p => p.StartTime);
            var end = commands.Max(p => p.EndTime);

            Debug.Assert(FrameStartTime==int.MinValue||FrameStartTime==start||this is StoryboardBackgroundObject||Z<0, "目前实现不能再次更变FrameStartTime");

            FrameStartTime=start;
            FrameEndTime=end;
        }

        public long FileLine { get; set; }

        public override string ToString() => $"line {(FromOsbFile ? "osb" : "osu")}:{FileLine} ({layout.ToString()} {Z}): {ImageFilePath} : {FrameStartTime}~{FrameEndTime}";
    }
}