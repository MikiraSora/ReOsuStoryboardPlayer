﻿using ReOsuStoryboardPlayer.Core.Base;
using System.Linq;

namespace ReOsuStoryboardPlayer.Core.Commands.Group.Trigger
{
    public class TriggerSubTimelineCommand : Command
    {
        private TriggerCommand trigger_command;

        private CommandTimeline timeline;

        private readonly float CostTime;

        public TriggerSubTimelineCommand(TriggerCommand trigger_command, Event bind_event)
        {
            this.trigger_command=trigger_command;
            Event=bind_event;
            timeline=trigger_command.SubCommands[bind_event];

            CostTime=timeline.Count!=0 ? timeline.Max(x => x.EndTime) : 0;

            RelativeLine=trigger_command.RelativeLine;
        }

        public void UpdateOffset(int time)
        {
            StartTime=time;
            EndTime=(int)(StartTime+CostTime);
        }

        public override void Execute(StoryboardObject @object, float current_value)
        {
            current_value-=StartTime;

            var command = timeline.PickCommand(current_value);

            if (command!=null)
            {
                command.Execute(@object, current_value);
#if DEBUG
                @object.MarkCommandExecuted(command);
#endif
            }
        }

        public override string ToString() => $"{base.ToString()} --> ({trigger_command.ToString()})";
    }
}