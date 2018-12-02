using System;
using System.Linq;

namespace ReOsuStoryBoardPlayer.Commands
{
    internal class LoopSubTimelineCommand : Command
    {
        private LoopCommand loop_command;

        private CommandTimeline timeline;

        private readonly int Sub_CostTime;

        public LoopSubTimelineCommand(LoopCommand loop_command, Event bind_event)
        {
            this.loop_command = loop_command;
            Event = bind_event;
            timeline = loop_command.SubCommands[bind_event];
            timeline.Sort();

            Sub_CostTime =timeline.Max(c => c.EndTime);

            StartTime = loop_command.StartTime;
            EndTime =loop_command.EndTime;
        }

        public override void Execute(StoryBoardObject @object, float current_value)
        {
            int relative_time = (int)current_value;

            if (StartTime<=current_value && current_value<=EndTime&&Sub_CostTime!=0)
                relative_time=(int)(current_value-loop_command.StartTime)%loop_command.CostTime;
            
            var command = timeline.PickCommand(relative_time);

            if (command!=null)
            {
                command.Execute(@object, relative_time);
#if DEBUG
                @object.MarkCommandExecuted(command);
#endif
            }
        }

        public override string ToString() => $"{base.ToString()} --> ({loop_command.ToString()})";
    }
}