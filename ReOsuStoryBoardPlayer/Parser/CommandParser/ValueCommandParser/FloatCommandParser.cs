using ReOsuStoryBoardPlayer.Commands;
using ReOsuStoryBoardPlayer.Parser.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser.CommandParser.ValueCommandParser
{
    public class FloatCommandParser<CMD> : IValueCommandParser<float, CMD> where CMD : ValueCommand<float>, new()
    {
        public override int ParamCount => 1;

        public override float ConvertValue(IEnumerable<string> p) => p.First().ToSigle();
    }
}
