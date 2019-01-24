using System.Collections.Generic;
using System.Linq;

namespace ReOsuStoryboardPlayer.ProgramCommandParser
{
    public class Parameters : IParameters
    {
        public string ArgString { get; }

        public Dictionary<string, string> Args { get; } = new Dictionary<string, string>();
        public List<string> FreeArgs { get; } = new List<string>();
        public List<string> Switches { get; } = new List<string>();
        public List<string> SimpleArgs { get; } = new List<string>();

        public Parameters()
        {
        }

        public Parameters(IParameters p)
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

        public bool TryGetArg(out string value, params string[] keys)
        {
            var x = Args.FirstOrDefault(pair => keys.Any(key => key==pair.Key));

            value=x.Value;

            return !string.IsNullOrWhiteSpace(x.Key);
        }

        public bool TryGetSwitch(params string[] option_names)
        {
            return Switches.Any(x => option_names.Contains(x));
        }
    }
}