﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReOsuStoryBoardPlayer.Core.Commands;
using ReOsuStoryBoardPlayer.Core.Commands.Group.Trigger;
using ReOsuStoryBoardPlayer.Utils;

namespace ReOsuStoryBoardPlayer.Core.Parser.CommandParser
{
    public class TriggerCommandParser : ICommandParser
    {
        public IEnumerable<Command> Parse(IEnumerable<string> data_arr)
        {
            var condition_text = data_arr.ElementAt(1);
            var condition = TriggerConditionBase.Parse(condition_text);

            TriggerCommand command = new TriggerCommand(condition);

            command.StartTime=data_arr.ElementAt(2).ToInt();
            command.EndTime=string.IsNullOrWhiteSpace(data_arr.ElementAt(3)) ? command.StartTime : data_arr.ElementAt(3).ToInt();
            command.GroupID=data_arr.Count()>4 ? (string.IsNullOrWhiteSpace(data_arr.ElementAt(4)) ? 0 : data_arr.ElementAt(4).ToInt()) : 0;

            yield return command;
        }
    }
}