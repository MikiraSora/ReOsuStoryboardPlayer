using System;
using System.Linq;

namespace ReOsuStoryBoardPlayer.Commands
{
    internal class LoopSubTimelineCommand : Command
    {
        private LoopCommand loop_command;

        private CommandTimeline timeline;

        private readonly int CostTime;

        public LoopSubTimelineCommand(LoopCommand loop_command, Event bind_event)
        {
            this.loop_command = loop_command;
            Event = bind_event;
            timeline = loop_command.SubCommands[bind_event];

            CostTime=loop_command.CostTime;
            StartTime = loop_command.StartTime;
            EndTime =loop_command.EndTime;
        }

        public override void Execute(StoryBoardObject @object, float current_value)
        {
            float relative_time = current_value;

            if (StartTime<=current_value&&current_value<=EndTime&&CostTime!=0)
                relative_time=(current_value-StartTime)%CostTime;
            
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