using ReOsuStoryBoardPlayer.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser.CommandParser
{
    public interface ICommandParser
    {
        IEnumerable<Command> Parse(IEnumerable<string> data_arr);
    }
}
