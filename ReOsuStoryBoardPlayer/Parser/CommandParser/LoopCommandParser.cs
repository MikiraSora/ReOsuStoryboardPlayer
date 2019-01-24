using ReOsuStoryBoardPlayer.Commands;
using ReOsuStoryBoardPlayer.Commands.Group;
using ReOsuStoryBoardPlayer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser.CommandParser
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
