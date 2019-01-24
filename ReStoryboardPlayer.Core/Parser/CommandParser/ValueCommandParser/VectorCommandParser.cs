using ReOsuStoryBoardPlayer.Core.Commands;
using ReOsuStoryBoardPlayer.Core.PrimitiveValue;
using ReOsuStoryBoardPlayer.Core.Utils;
using System.Collections.Generic;
using System.Linq;

namespace ReOsuStoryBoardPlayer.Core.Parser.CommandParser.ValueCommandParser
{
    public class VectorCommandParser<CMD> : IValueCommandParser<Vector, CMD> where CMD : ValueCommand<Vector>, new()
    {
        public override int ParamCount => 2;

        public override Vector ConvertValue(IEnumerable<string> p) => new Vector(p.First().ToSigle(), p.Last().ToSigle());
    }
}