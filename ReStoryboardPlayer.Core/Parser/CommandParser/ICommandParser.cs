using ReOsuStoryboardPlayer.Core.Commands;
using System.Collections.Generic;

namespace ReOsuStoryboardPlayer.Core.Parser.CommandParser
{
    public interface ICommandParser
    {
        IEnumerable<Command> Parse(IEnumerable<string> data_arr);
    }
}