using ReOsuStoryBoardPlayer.Core.Commands;
using ReOsuStoryBoardPlayer.Core.Utils;
using System.Collections.Generic;
using System.Linq;

namespace ReOsuStoryBoardPlayer.Core.Parser.CommandParser.ValueCommandParser
{
    public class FloatCommandParser<CMD> : IValueCommandParser<float, CMD> where CMD : ValueCommand<float>, new()
    {
        public override int ParamCount => 1;

        public override float ConvertValue(IEnumerable<string> p) => p.First().ToSigle();
    }
}