﻿using System.Linq;

namespace ReOsuStoryboardPlayer.ProgramCommandParser
{
    internal class CommandParser
    {
        private readonly ParamParserV2 _parser;

        public CommandParser(ParamParserV2 parser)
        {
            this._parser=parser;
        }

        public Parameters Parse(string args, out string cmdName)
        {
            var array = args.Split(' ');
            cmdName=array.First();
            var arg = string.Join(" ", array.Skip(1));
            if (_parser.TryDivide(arg, out var p))
            {
                return new Parameters(p);
            }

            return null;
        }

        public Parameters Parse(string[] args, out string cmdName)
        {
            for (var i = 0; i<args.Length; i++)
            {
                var s = args[i];
                if (s.Any(k => k==' '))
                {
                    args[i]=$"{_parser.Quotes.First()}{s}{_parser.Quotes.First()}";
                }
            }

            return Parse(string.Join(" ", args), out cmdName);
        }
    }
}