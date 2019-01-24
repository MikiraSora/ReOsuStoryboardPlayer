using System.Collections.Generic;

namespace ReOsuStoryboardPlayer.ProgramCommandParser
{
    internal class ParametersWithCommand : IParameters
    {
        public string CommandName { get; }
        public string ArgString { get; }

        public Dictionary<string, string> Args { get; } = new Dictionary<string, string>();
        public List<string> FreeArgs { get; } = new List<string>();
        public List<string> Switches { get; } = new List<string>();
        public List<string> SimpleArgs { get; } = new List<string>();

        public ParametersWithCommand()
        {
        }

        public ParametersWithCommand(IParameters p)
        {
            ArgString=p.ArgString;
            Args=p.Args;
            FreeArgs=p.FreeArgs;
            Switches=p.Switches;
            SimpleArgs=p.SimpleArgs;
        }

        public bool TryGetArg(string key, out string value)
        {
            if (Args.ContainsKey(key))
            {
                value=Args[key];
                return true;
            }

            value=null;
            return false;
        }
    }
}