using ReOsuStoryboardPlayer.Core.Commands;
using ReOsuStoryboardPlayer.Core.Commands.Group;
using ReOsuStoryboardPlayer.Core.Commands.Group.Trigger;
using ReOsuStoryboardPlayer.Core.Parser.CommandParser.ValueCommandParser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReOsuStoryboardPlayer.Core.Parser.CommandParser
{
    public class CommandParserIntance
    {
        private Dictionary<string, ICommandParser> ext_map;

        public IEnumerable<Command> Parse(string command_line) => Parse(command_line.Split(','));

        public CommandParserIntance(Dictionary<string, ICommandParser> ext_command_parser_creator = null)
        {
            ext_map = ext_command_parser_creator?.ToDictionary(x => x.Key, y => y.Value) ?? new Dictionary<string, ICommandParser>();
        }

        public IEnumerable<Command> Parse(IEnumerable<string> data_arr)
        {
            var command_event = data_arr.First().TrimStart(' ', '_');

            if (!TryGetCommandParaser(command_event, data_arr, out var result))
                throw new Exception("Unknown command event:" + command_event);

            foreach (var cmd in result)
                yield return cmd;
        }

        private bool TryGetCommandParaser(string command_event, IEnumerable<string> data_arr, out IEnumerable<Command> result)
        {
            result = null;

            if (ext_map.TryGetValue(command_event,out var parser))
            {
                result = parser.Parse(data_arr);
                return true;
            }

            return false;
        }
    }
}