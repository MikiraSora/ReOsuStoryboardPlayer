using ReOsuStoryBoardPlayer.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Commands
{
    class LoopSubTimelineCommand : Command
    {
        private readonly Event bind_event;

        LoopCommand loop_command { get; }

        CommandTimeline timeline => loop_command.SubCommands[bind_event];

        public LoopSubTimelineCommand(LoopCommand loop_command, Event bind_event)
        {
            this.loop_command = loop_command;
            this.bind_event = bind_event;
            Event = bind_event;

            var start_time_offset = loop_command.SubCommands[bind_event].Min(c => c.StartTime);

            StartTime = loop_command.StartTime + start_time_offset;
            EndTime = StartTime+loop_command.SubCommands[bind_event].Max(c => c.EndTime) * loop_command.LoopCount - start_time_offset;
        }

        public override void Execute(StoryBoardObject @object, float current_value)
        {
            int recovery_time = (int)(current_value - loop_command.StartTime);

            int timeline_cost_time = timeline.EndTime - timeline.StartTime;

            int current_time = timeline_cost_time == 0 ? recovery_time : recovery_time % timeline_cost_time;

            int current_loop_index = timeline_cost_time == 0 ? 0 : (recovery_time - timeline.StartTime) / timeline_cost_time;

            var command = timeline.PickCommand((current_value - StartTime) % timeline_cost_time + timeline.StartTime);

            if (command != null)
            {
                //store command start/end time
                var offset_time = loop_command.StartTime + current_loop_index * timeline_cost_time;
                command.StartTime += offset_time;
                command.EndTime += offset_time;

                command.Execute(@object, current_value);
                @object.MarkCommandExecuted(command);

                //restore command
                command.StartTime -= offset_time;
                command.EndTime -= offset_time;
            }
        }

        public override string ToString() => $"{base.ToString()} --> ({loop_command.ToString()})";
    }
}
