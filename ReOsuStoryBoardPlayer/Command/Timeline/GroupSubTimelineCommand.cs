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

            /*
            int relative_time = (int)(current_value - loop_command.StartTime);

            int timeline_cost_time = timeline.EndTime - timeline.StartTime;

            int current_time = timeline_cost_time == 0 ? relative_time : relative_time % timeline_cost_time;

            int current_loop_index = timeline_cost_time==0 ? 0 : Math.Min((relative_time-timeline.StartTime)/timeline_cost_time, this.loop_command.LoopCount);

            //一个时间轴上只有一个 0 duration的命令
            var mapped_time = timeline_cost_time == 0 ? timeline.StartTime : (current_value - StartTime) % timeline_cost_time + timeline.StartTime;

            var command = timeline.PickCommand(mapped_time);

            if (command != null)
            {
                //store command start/end time
                var offset_time = loop_command.StartTime + current_loop_index * timeline_cost_time;
                command.StartTime += offset_time;
                command.EndTime += offset_time;

                command.Execute(@object, current_value);

#if DEBUG
                @object.MarkCommandExecuted(command);
#endif

                //restore command
                command.StartTime -= offset_time;
                command.EndTime -= offset_time;
            }
            */
        }

        public override string ToString() => $"{base.ToString()} --> ({loop_command.ToString()})";
    }
}