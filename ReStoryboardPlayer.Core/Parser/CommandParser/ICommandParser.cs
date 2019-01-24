using ReOsuStoryBoardPlayer.Core.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Core.Parser.CommandParser
{
    public interface ICommandParser
    {
        IEnumerable<Command> Parse(IEnumerable<string> data_arr);
    }
}
