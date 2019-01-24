using ReOsuStoryBoardPlayer.Commands;
using ReOsuStoryBoardPlayer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser.CommandParser.ValueCommandParser
{
    public class Vec4CommandParser<CMD> : IValueCommandParser<Vec4, CMD> where CMD : ValueCommand<Vec4>, new()
    {
        public override int ParamCount => 3;

        public override Vec4 ConvertValue(IEnumerable<string> p) => new Vec4(p.ElementAt(0).ToSigle(), p.ElementAt(1).ToSigle(), p.ElementAt(2).ToSigle(), 0);
    }
}
