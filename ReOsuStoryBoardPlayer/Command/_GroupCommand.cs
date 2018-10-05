using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Commands
{
    public abstract class _GroupCommand : _Command
    {
        public Dictionary<Event, _CommandTimeline> SubCommands { get; set; } = new Dictionary<Event, _CommandTimeline>();

        public virtual void AddSubCommand(_Command command)
        {
            if (!SubCommands.ContainsKey(command.Event))
                SubCommands[command.Event] = new _CommandTimeline();
            SubCommands[command.Event].Add(command);
        }

        public void AddSubCommand(IEnumerable<_Command> commands)
        {
            foreach (var command in commands)
                AddSubCommand(command);
        }
    }
}
