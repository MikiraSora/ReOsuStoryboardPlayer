using System.Collections.Generic;

namespace ReOsuStoryBoardPlayer.Commands.Group
{
    public abstract class GroupCommand : Command
    {
        public Dictionary<Event, CommandTimeline> SubCommands { get; set; } = new Dictionary<Event, CommandTimeline>();

        public virtual void AddSubCommand(Command command)
        {
            if (!SubCommands.ContainsKey(command.Event))
                SubCommands[command.Event]=new CommandTimeline();
            SubCommands[command.Event].Add(command);
        }

        public void AddSubCommand(IEnumerable<Command> commands)
        {
            foreach (var command in commands)
                AddSubCommand(command);
        }

        public virtual void UpdateSubCommand()
        {
            foreach (var list in SubCommands.Values)
                list.Sort();
        }
    }
}