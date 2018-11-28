using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.CommandParser
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
            ArgString = p.ArgString;
            Args = p.Args;
            FreeArgs = p.FreeArgs;
            Switches = p.Switches;
            SimpleArgs = p.SimpleArgs;
        }

        public bool TryGetArg(string key, out string value)
        {
            if (Args.ContainsKey(key))
            {
                value = Args[key];
                return true;
            }

            value = null;
            return false;
        }
    }
}
