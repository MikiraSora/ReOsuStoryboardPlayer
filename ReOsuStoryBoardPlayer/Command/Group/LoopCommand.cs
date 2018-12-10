using System.Collections.Generic;
using System.Linq;

namespace ReOsuStoryBoardPlayer.Commands
{
    internal class LoopCommand : GroupCommand
    {
        public LoopCommand() => Event = Event.Loop;

        public int CostTime { get; private set; }

        public int LoopCount { get; set; }

        public void UpdateSubCommand()
        {
            var commands = SubCommands.SelectMany(l => l.Value);
            var offset = commands.Min(x => x.StartTime);
            StartTime+=offset;
            foreach (var command in commands)
            {
                command.StartTime-=offset;
                command.EndTime-=offset;
            }

            CostTime =commands.Max(c => c.EndTime);
            var total_cast_time = CostTime * LoopCount;

            EndTime = StartTime + total_cast_time;

            foreach (var list in SubCommands.Values)
                list.Sort();
        }

        public override void Execute(StoryBoardObject @object, float current_value)
        {
            //咕咕哒
        }

        public override string ToString() => $"{base.ToString()} (Times:{LoopCount} CostPerLoop:{CostTime})";
    }
}