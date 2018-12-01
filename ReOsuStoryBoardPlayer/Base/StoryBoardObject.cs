using ReOsuStoryBoardPlayer.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReOsuStoryBoardPlayer
{
    public class StoryBoardObject
    {
        internal Dictionary<Event, CommandTimeline> CommandMap = new Dictionary<Event, CommandTimeline>();

        public string ImageFilePath;

        public int FrameStartTime, FrameEndTime;

        public bool markDone = false;

        public SpriteInstanceGroup RenderGroup;

        public Layout layout;

        public int Z = -1;

        #region Transform

        public Vector Postion = new Vector(320, 240), Scale = new Vector(1, 1);

        public Vec4 Color = new Vec4(1, 1, 1, 1);

        public float Rotate = 0;

        public Vector Anchor = new Vector(0.5f, 0.5f);

        public bool IsAdditive = false, IsHorizonFlip = false, IsVerticalFlip = false;

        #endregion Transform

        public void AddCommand(Command command)
        {
            if (command is LoopCommand loop)
            {
                AddCommand(loop);
                //这里不用return是因为还要再Visualizer显示这个Loop命令，方便调试，Loop::Execute(...)已被架空
            }

            /*
             这里因为Move/MoveX/MoveY是不同时间轴的执行，会导致MoveX执行后又被执行MoveY之类导致命令冲突，
             所以干脆直接将几个命令和其变种都放置在同一个时间轴上

             cnm,脑力跑不了:https://puu.sh/CaHLu/4654dad23f.png
             */

            var cmd_event = command.Event;

            switch (cmd_event)
            {
                case Event.MoveX:
                case Event.MoveY:
                case Event.Move:
                    cmd_event=Event.Move;
                    break;
                case Event.Scale:
                case Event.VectorScale:
                    cmd_event=Event.VectorScale;
                    break;
                default:
                    break;
            }

            if (!CommandMap.TryGetValue(cmd_event, out var timeline))
                timeline = CommandMap[cmd_event] = new CommandTimeline();
            timeline.Add(command);
        }

        private void AddCommand(LoopCommand loop_command)
        {
            //将Loop命令各个类型的子命令时间轴封装成一个命令，并添加到物件本体各个时间轴上

            foreach (var @event in loop_command.SubCommands.Keys)
            {
                var sub_command_wrapper = new LoopSubTimelineCommand(loop_command, @event);
                AddCommand(sub_command_wrapper);
            }
        }

        public void SortCommands()
        {
            foreach (var time in CommandMap.Values)
                time.Sort();
        }

        public virtual void Update(float current_time)
        {
#if DEBUG
            ExecutedCommands.ForEach(c => c.IsExecuted = false);
            ExecutedCommands.Clear();
#endif

            foreach (var timeline in CommandMap.Values)
            {
                foreach (var command in timeline.PickCommands(current_time))
                {
                    command.Execute(this, current_time);

#if DEBUG
                    MarkCommandExecuted(command);
#endif
                }
            }
        }

        public override string ToString() => $"line {FileLine} (index {Z}): {ImageFilePath} : {FrameStartTime}~{FrameEndTime}";

#if DEBUG
        internal List<Command> ExecutedCommands = new List<Command>();

        internal void MarkCommandExecuted(Command command, bool is_exec = true)
        {
            if (is_exec)
                ExecutedCommands.Add(command);
            else
                ExecutedCommands.Remove(command);

            command.IsExecuted = is_exec;
        }

#endif

        public void UpdateObjectFrameTime()
        {
            var commands = CommandMap.Where(v => !SkipEvent.Contains(v.Key)).SelectMany(l => l.Value);

            if (commands.Count() == 0)
                return;

            FrameStartTime = commands.Where(p => !(p is GroupCommand)).Min(p => p.StartTime);
            FrameEndTime = commands.Where(p => !(p is GroupCommand)).Max(p => p.EndTime);
        }

        private readonly static Event[] SkipEvent = new[]
        {
            Event.Parameter,
            Event.HorizonFlip,
            Event.AdditiveBlend,
            Event.VerticalFlip
        };

        public long FileLine { get; set; }
    }
}