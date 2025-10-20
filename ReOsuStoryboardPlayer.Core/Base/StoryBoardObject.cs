using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommunityToolkit.HighPerformance.Buffers;
using ReOsuStoryboardPlayer.Core.Commands;
using ReOsuStoryboardPlayer.Core.Commands.Group;
using ReOsuStoryboardPlayer.Core.Commands.Group.Trigger;
using ReOsuStoryboardPlayer.Core.Kernel;
using ReOsuStoryboardPlayer.Core.PrimitiveValue;
using ReOsuStoryboardPlayer.Core.Serialization;
using ReOsuStoryboardPlayer.Core.Serialization.DeserializationFactory;

namespace ReOsuStoryboardPlayer.Core.Base
{
    public class StoryboardObject : IStoryboardSerializable, IEquatable<StoryboardObject>
    {
        /// <summary>
        ///     钦定这个物件的最初变换值，通过委托链可以覆盖初始值
        /// </summary>
        public Action<StoryboardObject> BaseTransformResetAction;

        public SortedDictionary<Event, CommandTimeline> CommandMap = new SortedDictionary<Event, CommandTimeline>();

        public long FileLine;

        public int FrameStartTime = int.MinValue, FrameEndTime;

        public bool FromOsbFile;

        public string ImageFilePath;

        public Layer layer;

        //表示此物件拥有的Trigger集合，Key为GroupID
        public Dictionary<int, HashSet<TriggerCommand>> Triggers = new Dictionary<int, HashSet<TriggerCommand>>();

        public int Z = -1;

        public StoryboardObject()
        {
            BaseTransformResetAction = obj =>
            {
                obj.Postion = new Vector(320, 240);
                obj.Scale = new Vector(1, 1);

                obj.Color = new ByteVec4(255, 255, 255, 255);

                obj.Rotate = 0;

                obj.IsAdditive = false;
                obj.IsHorizonFlip = false;
                obj.IsVerticalFlip = false;
            };
        }

        //表示此时驱动更新物件的Update
        public StoryboardUpdater CurrentUpdater { get; internal set; }

        public bool IsVisible { get; private set; }

        public bool ContainTrigger => CommandMap.ContainsKey(Event.Trigger);

        public bool ContainLoop => CommandMap.ContainsKey(Event.Loop);

        public bool ContainNonValueCommand => ContainLoop || ContainTrigger;

        public void ResetTransform()
        {
            BaseTransformResetAction(this);
        }

        public virtual void Update(float current_time)
        {
#if DEBUG
            ExecutedCommands.ForEach(c => c.IsExecuted = false);
            ExecutedCommands.Clear();
#endif
            foreach (var pair in CommandMap)
            {
                var timeline = pair.Value;
                var command = timeline.PickCommand(current_time);

                if (command == null)
                    continue;

                command.Execute(this, current_time);
#if DEBUG
                MarkCommandExecuted(command);
#endif
            }

            IsVisible = Color.W != 0;
        }

        /// <summary>
        ///     计算物件的FrameTime
        ///     (此方法必须确保计算出来的物件时间是基于命令的真实的有效时间，不能因为Trigger而提前计算，FrameStartTime必须是一次性算好固定的值(否则Scan炸了，理论上也没什么玩意可以变更此参数))
        /// </summary>
        public void CalculateAndApplyBaseFrameTime()
        {
            var commands = CommandMap.SelectMany(l => l.Value);

            if (commands.Count() == 0)
                return;

            var start = commands.Min(p => p.StartTime);
            var end = commands.Max(p => p.EndTime);

            //Debug.Assert(FrameStartTime==int.MinValue||FrameStartTime==start||this is StoryboardBackgroundObject||Z<0, "目前实现不能再次更变FrameStartTime");

            var need_resort = start != FrameStartTime;

            FrameStartTime = start;
            FrameEndTime = end;

            if (need_resort)
                CurrentUpdater?.AddNeedResortObject(this);
        }

        public override string ToString()
        {
            return
                $"line {(FromOsbFile ? "osb" : "osu")}:{FileLine} ({layer.ToString()} {Z}): {ImageFilePath} : {FrameStartTime}~{FrameEndTime}";
        }

        #region Transform

        public Vector Postion, Scale;

        public ByteVec4 Color;

        public float Rotate;

        public HalfVector OriginOffset = new HalfVector(0f, 0f);

        public bool IsAdditive, IsHorizonFlip, IsVerticalFlip;

        #endregion Transform

        #region Add/Remove Command

        public void AddCommand(Command command)
        {
            switch (command.Event)
            {
                case Event.Loop:
                    AddLoopCommand((LoopCommand) command);
                    break;

                case Event.Trigger:
                    AddTriggerCommand((TriggerCommand) command);
                    break;
            }

            if (!CommandMap.TryGetValue(command.Event, out var timeline))
                timeline = CommandMap[command.Event] = new CommandTimeline();

            timeline.Add(command);
        }

        public void AddCommandRange(IEnumerable<Command> commands)
        {
            foreach (var pair in commands.GroupBy(x => x.Event))
            {
                switch (pair.Key)
                {
                    case Event.Loop:
                    case Event.Trigger:
                        foreach (var command in pair)
                            AddCommand(command);
                        continue;
                }

                if (!CommandMap.TryGetValue(pair.Key, out var timeline))
                    timeline = CommandMap[pair.Key] = new CommandTimeline();

                timeline.AddRange(pair);
            }
        }

        private void AddLoopCommand(LoopCommand loop_command)
        {
            if (Setting.EnableLoopCommandUnrolling)
                //将Loop命令各个类型的子命令时间轴封装成一个命令，并添加到物件本体各个时间轴上
                foreach (var cmd in loop_command.SubCommandExpand())
                    AddCommand(cmd);
            else
                //将Loop命令里面的子命令展开
                foreach (var @event in loop_command.SubCommands.Keys)
                {
                    var sub_command_wrapper = new LoopSubTimelineCommand(loop_command, @event);
                    AddCommand(sub_command_wrapper);
                }
        }

        private void AddTriggerCommand(TriggerCommand trigger_command, bool insert = false)
        {
            if (!Triggers.TryGetValue(trigger_command.GroupID, out var list))
                Triggers[trigger_command.GroupID] = new HashSet<TriggerCommand>();

            Triggers[trigger_command.GroupID].Add(trigger_command);
            trigger_command.BindObject(this);
            TriggerListener.DefaultListener.Add(this);

            if (!CommandMap.TryGetValue(Event.Trigger, out var x) || x.Count == 0)
                BaseTransformResetAction += TriggerCommand.OverrideDefaultValue;
        }

        public void RemoveCommand(Command command)
        {
            switch (command)
            {
                case LoopCommand loop_command:
                    foreach (var t in CommandMap.Values)
                    {
                        var result = t /*.OfType<LoopSubTimelineCommand>()*/
                            .Where(x => x.RelativeLine == loop_command.RelativeLine).ToArray();

                        foreach (var c in result)
                            t.Remove(c);
                    }

                    break;

                case TriggerCommand trigger_command:
                    Triggers[trigger_command.GroupID].Remove(trigger_command);

                    if (!Triggers.Values.SelectMany(l => l).Any())
                        TriggerListener.DefaultListener.Remove(this);
                    break;
            }

            //删除无用的时间轴
            if (CommandMap.TryGetValue(command.Event, out var timeline))
            {
                timeline.Remove(command);

                if (timeline.Count == 0)
                {
                    CommandMap.Remove(command.Event);

                    if (command.Event == Event.Trigger)
                        BaseTransformResetAction -= TriggerCommand.OverrideDefaultValue;
                }
            }
        }

        #endregion Add/Remove Command

#if DEBUG
        public List<Command> ExecutedCommands = new List<Command>();

        public bool DebugShow = true;

        internal void MarkCommandExecuted(Command command, bool is_exec = true)
        {
            if (is_exec)
                ExecutedCommands.Add(command);
            else
                ExecutedCommands.Remove(command);

            command.IsExecuted = is_exec;
        }
#endif

        #region Serialization

        /* Binary Layout:
         * int: Addable command count
         * ~~~~ : command binary data
         * ....
         */

        public virtual void OnSerialize(BinaryWriter stream, StringCacheTable cache_table)
        {
            /*
             因为BaseTransformResetAction无法被序列化，因此只能先计算出来然后再将物件的各个变换值作为反序列化时生成新的初始化回调
             */
            ResetTransform();

            //normal commands
            var commands = CommandMap.Values.SelectMany(l => l)
                .Where(x => !(x is LoopSubTimelineCommand || x is TriggerSubTimelineCommand));

            stream.Write(commands.Count());

            foreach (var command in commands)
                command.OnSerialize(stream, cache_table);

            //ImageFilePath.OnSerialize(stream);
            var image_string_id = cache_table[ImageFilePath];
            stream.Write(image_string_id);

            FromOsbFile.OnSerialize(stream);
            FrameStartTime.OnSerialize(stream);
            FrameEndTime.OnSerialize(stream);
            ((byte) layer).OnSerialize(stream);
            Z.OnSerialize(stream);

            Postion.OnSerialize(stream, cache_table);
            Scale.OnSerialize(stream, cache_table);
            Color.OnSerialize(stream, cache_table);
            OriginOffset.OnSerialize(stream, cache_table);
            Rotate.OnSerialize(stream);

            IsAdditive.OnSerialize(stream);
            IsHorizonFlip.OnSerialize(stream);
            IsVerticalFlip.OnSerialize(stream);

            FileLine.OnSerialize(stream);
        }

        public virtual void OnDeserialize(BinaryReader stream, StringCacheTable cache_table)
        {
            var count = stream.ReadInt32();

            for (var i = 0; i < count; i++)
            {
                var command = CommandDeserializtionFactory.Create(stream, cache_table);
                AddCommand(command); //todo: use AddCommandRange()
            }

            //ImageFilePath=stream.ReadString();
            ImageFilePath = StringPool.Shared.GetOrAdd(cache_table[stream.ReadUInt32()]);

            FromOsbFile.OnDeserialize(stream);
            FrameStartTime.OnDeserialize(stream);
            FrameEndTime.OnDeserialize(stream);
            layer = (Layer) stream.ReadByte();
            Z.OnDeserialize(stream);

            Postion.OnDeserialize(stream, cache_table);
            Scale.OnDeserialize(stream, cache_table);
            Color.OnDeserialize(stream, cache_table);
            OriginOffset.OnDeserialize(stream, cache_table);
            Rotate.OnDeserialize(stream);

            IsAdditive.OnDeserialize(stream);
            IsHorizonFlip.OnDeserialize(stream);
            IsVerticalFlip.OnDeserialize(stream);

            FileLine.OnDeserialize(stream);

            //try to rebuild base trasform reset action
            var pos = Postion;
            var start = FrameStartTime;
            var end = FrameEndTime;
            var color = Color;
            var scale = Scale;
            var additive = IsAdditive;
            var rotate = Rotate;
            var horizon = IsHorizonFlip;
            var vertical = IsVerticalFlip;

            BaseTransformResetAction += obj =>
            {
                obj.Postion = pos;
                obj.FrameStartTime = start;
                obj.FrameEndTime = end;
                obj.Color = color;
                obj.Scale = scale;
                obj.IsAdditive = additive;
                obj.IsHorizonFlip = horizon;
                obj.IsVerticalFlip = vertical;
                obj.Rotate = rotate;
            };
        }

        public virtual bool Equals(StoryboardObject other)
        {
            if (!(other.ImageFilePath == ImageFilePath
                  && other.FromOsbFile == FromOsbFile
                  && other.FrameStartTime == FrameStartTime
                  && other.FrameEndTime == FrameEndTime
                  && other.layer == layer
                  && other.Z == Z
                  && other.Postion == Postion
                  && other.Scale == Scale
                  && other.Color == Color
                  && other.Rotate == Rotate
                  && other.OriginOffset == OriginOffset
                  && other.IsAdditive == IsAdditive
                  && other.IsHorizonFlip == IsHorizonFlip
                  && other.IsVerticalFlip == IsVerticalFlip
                  && other.FileLine == FileLine))
                return false;

            //var r = other.CommandMap.Values.SelectMany(l => l).All(x => CommandMap.Values.SelectMany(l => l).Any(y => y.Equals(x)));

            var a_commands = other.CommandMap.Values.SelectMany(l => l).ToList();
            var b_commands = CommandMap.Values.SelectMany(l => l).ToList();

            while (a_commands.Count != 0)
            {
                var cmd = a_commands.FirstOrDefault();
                a_commands.Remove(cmd);

                if (!b_commands.Remove(cmd))
                    return false;
            }

            return a_commands.Count + b_commands.Count == 0;
        }

        #endregion Serialization
    }
}