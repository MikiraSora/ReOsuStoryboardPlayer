using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.CommandParser
{
    public interface IParamParser
    {
        bool TryDivide(string args, out IParameters p);
    }
}
