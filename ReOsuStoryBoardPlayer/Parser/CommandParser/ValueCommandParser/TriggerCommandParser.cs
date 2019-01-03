using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReOsuStoryBoardPlayer.Commands;
using ReOsuStoryBoardPlayer.Commands.Group.Trigger;
using ReOsuStoryBoardPlayer.Parser.Extension;

namespace ReOsuStoryBoardPlayer.Parser.CommandParser.ValueCommandParser
{
    public class TriggerCommandParser : ICommandParser
    {
        public IEnumerable<Command> Parse(IEnumerable<string> data_arr)
        {
            var condition_text = data_arr.ElementAt(1);
            var condition = TriggerConditionBase.Parse(condition_text);

            TriggerCommand command = new TriggerCommand(condition);

            command.StartTime=data_arr.ElementAt(2).ToInt();
            command.EndTime=data_arr.ElementAt(3).ToInt();

            yield return command;
        }
    }
}
