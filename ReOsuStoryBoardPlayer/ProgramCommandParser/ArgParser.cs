using System.Linq;

namespace ReOsuStoryBoardPlayer.ProgramCommandParser
{
    public class ArgParser
    {
        private readonly ParamParserV2 _parser;

        public ArgParser(ParamParserV2 parser)
        {
            this._parser = parser;
        }

        public Parameters Parse(string args)
        {
            if (_parser.TryDivide(args, out var p))
            {
                return new Parameters(p);
            }

            return null;
        }
    }
}
