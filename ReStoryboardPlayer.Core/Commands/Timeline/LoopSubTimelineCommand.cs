using ReOsuStoryBoardPlayer.Core.Base;
using ReOsuStoryBoardPlayer.Core.Commands.Group;
using System;
using System.Linq;

namespace ReOsuStoryBoardPlayer.Core.Commands
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

            var offset = loop_command.SubCommands.SelectMany(l=>l.Value).Min(x=>x.StartTime);

            CostTime=loop_command.CostTime-offset;
            StartTime = loop_command.StartTime+offset;
            EndTime=StartTime+CostTime*loop_command.LoopCount;

            RelativeLine=loop_command.RelativeLine;
        }
        
        public override void Execute(StoryBoardObject @object, float current_value)
        {
            float relative_time = current_value;
            
            if (current_value<StartTime)
                relative_time=timeline.StartTime;
            else if (current_value>EndTime)
                relative_time=timeline.EndTime;
            else if(CostTime!=0)
                relative_time=(current_value-StartTime)%CostTime;

            //if (StartTime<=current_value&&current_value<=EndTime&&)
            
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