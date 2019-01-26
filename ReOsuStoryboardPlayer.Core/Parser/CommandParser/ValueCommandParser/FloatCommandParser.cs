using ReOsuStoryboardPlayer.Core.Commands;
using ReOsuStoryboardPlayer.Core.Utils;
using System.Collections.Generic;
using System.Linq;

namespace ReOsuStoryboardPlayer.Core.Parser.CommandParser.ValueCommandParser
{
    public class FloatCommandParser<CMD> : IValueCommandParser<float, CMD> where CMD : ValueCommand<float>, new()
    {
        public override int ParamCount => 1;

        public override float ConvertValue(IEnumerable<string> p) => p.First().ToSigle();
    }
}