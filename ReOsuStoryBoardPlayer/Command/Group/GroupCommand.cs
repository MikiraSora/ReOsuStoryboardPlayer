using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Commands
{
    public abstract class GroupCommand : Command
    {
        public Dictionary<Event, CommandTimeline> SubCommands { get; set; } = new Dictionary<Event, CommandTimeline>();

        public virtual void AddSubCommand(Command command)
        {
            if (!SubCommands.ContainsKey(command.Event))
                SubCommands[command.Event] = new CommandTimeline();
            SubCommands[command.Event].Add(command);
        }

        public void AddSubCommand(IEnumerable<Command> commands)
        {
            foreach (var command in commands)
                AddSubCommand(command);
        }
    }
}
