using ReOsuStoryboardPlayer.Core.Commands;
using ReOsuStoryboardPlayer.Core.Commands.Group;
using ReOsuStoryboardPlayer.Core.Utils;
using System.Collections.Generic;
using System.Linq;

namespace ReOsuStoryboardPlayer.Core.Parser.CommandParser
{
    public class LoopCommandParser : ICommandParser
    {
        public IEnumerable<Command> Parse(IEnumerable<string> data_arr)
        {
            LoopCommand command = new LoopCommand();

            command.StartTime=data_arr.ElementAt(1).ToInt();
            command.LoopCount=data_arr.ElementAt(2).ToInt();

            yield return command;
        }
    }
}