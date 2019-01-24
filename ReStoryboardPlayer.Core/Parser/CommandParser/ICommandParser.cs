using ReOsuStoryBoardPlayer.Core.Commands;
using System.Collections.Generic;

namespace ReOsuStoryBoardPlayer.Core.Parser.CommandParser
{
    public interface ICommandParser
    {
        IEnumerable<Command> Parse(IEnumerable<string> data_arr);
    }
}