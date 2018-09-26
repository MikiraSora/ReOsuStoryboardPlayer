using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser
{
    interface IReader<T>
    {
        bool IsEnd { get; }

        IEnumerable<T> GetValues();
    }
}
