using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryBroadParser
{
    public class LoopCommand : Command
    {
        public LoopCommand(){}

        public List<Command> _loopCommandList = new List<Command>();
    }
}
