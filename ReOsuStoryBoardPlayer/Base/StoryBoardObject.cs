using ReOsuStoryBoardPlayer.Commands;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ReOsuStoryBoardPlayer.Graphics;

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

        public ByteVec4 Color = new ByteVec4(255,255,255,255);

        public float Rotate = 0;

        public HalfVector Anchor = new HalfVector(0f, 0f);

        public bool IsAdditive = false, IsHorizonFlip = false, IsVerticalFlip = false;

        #endregion Transform

        public void AddCommand(Command command)
        {
            if (command is LoopCommand loop)
            {
                AddLoopCommand(loop);
                //这里不用return是因为还要再Visualizer显示这个Loop命令，方便调试，Loop::Execute(...)已被架空
            }

            if (!CommandMap.TryGetValue(command.Event, out var timeline))
                timeline = CommandMap[command.Event] = new CommandTimeline();
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

        public void SortCommands()
        {
            foreach (var time in CommandMap.Values)
                time.Sort((x,y) => 
                {
                    var z = x.StartTime-y.StartTime;

                    if (z!=0)
                        return z;

                    return x.EndTime-y.EndTime;
                });
        }

        public virtual void Update(float current_time)
        {
#if DEBUG
            ExecutedCommands.ForEach(c => c.IsExecuted = false);
            ExecutedCommands.Clear();
#endif
            foreach (var timeline in CommandMap.Values)
            {
                var command = timeline.PickCommand(current_time);

                command.Execute(this, current_time);
#if DEBUG
                MarkCommandExecuted(command);
#endif
            }
        }


#if DEBUG
        internal List<Command> ExecutedCommands = new List<Command>();

        internal bool DebugShow = true;

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
            var commands = CommandMap.SelectMany(l => l.Value);

            if (commands.Count() == 0)
                return;

            FrameStartTime = commands.Min(p => p.StartTime);
            FrameEndTime = commands.Max(p => p.EndTime);
        }

        public long FileLine { get; set; }

        public override string ToString() => $"line {FileLine} ({layout.ToString()} {Z}): {ImageFilePath} : {FrameStartTime}~{FrameEndTime}";
    }
}