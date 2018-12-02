using ReOsuStoryBoardPlayer.Commands;
using ReOsuStoryBoardPlayer.Parser.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser.CommandParser.ValueCommandParser
{
    public class VectorCommandParser<CMD> : IValueCommandParser<Vector, CMD> where CMD : ValueCommand<Vector>, new()
    {
        public override int ParamCount => 2;

        public override Vector ConvertValue(IEnumerable<string> p) => new Vector(p.First().ToSigle(), p.Last().ToSigle());
    }
}
