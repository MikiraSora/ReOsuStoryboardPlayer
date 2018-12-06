using System.Linq;

namespace ReOsuStoryBoardPlayer.Commands
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

            return Parse(string.Join(" ", args));
        }
    }
}
