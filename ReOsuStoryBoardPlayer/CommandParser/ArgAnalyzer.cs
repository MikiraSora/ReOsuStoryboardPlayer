using System.Collections.Generic;
using System.Linq;

namespace ReOsuStoryBoardPlayer.CommandParser
{
    public class ArgAnalyzer
    {
        private readonly ParamParserV2 _parser;

        public ArgAnalyzer(ParamParserV2 parser)
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

        public Parameters Parse(string[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                var s = args[i];
                if (s.Any(k => k == ' '))
                {
                    args[i] = $"{_parser.Quotes.First()}{s}{_parser.Quotes.First()}";
                }
            }

            if (_parser.TryDivide(string.Join(" ", args), out var p))
            {
                return new Parameters(p);
            }

            return null;
        }
    }
}
