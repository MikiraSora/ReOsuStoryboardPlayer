using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Commands
{
    public abstract class _GroupCommand : _Command
    {
        protected Dictionary<Event,CommandTimeline> sub_commands = new Dictionary<Event, CommandTimeline>();

        public IEnumerable<_Command> SubCommands => sub_commands.Values.SelectMany(l => l).OrderBy(c => c.StartTime);

        public virtual void AddSubCommand(_Command command)
        {
            if (!sub_commands.ContainsKey(command.Event))
                sub_commands[command.Event] = new CommandTimeline();
            sub_commands[command.Event].Add(command);
        }

        public void AddSubCommand(IEnumerable<_Command> commands)
        {
            foreach (var command in commands)
                AddSubCommand(command);
        }
    }
}
