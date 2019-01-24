using ReOsuStoryboardPlayer.Core.Commands;
using ReOsuStoryboardPlayer.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReOsuStoryboardPlayer.Core.Parser.CommandParser
{
    public class ParameterCommandParser : ICommandParser
    {
        public IEnumerable<Command> Parse(IEnumerable<string> data_arr)
        {
            StateCommand command = null;

            switch (data_arr.ElementAt(4))
            {
                case "A":
                    command=new AdditiveBlendCommand();
                    break;

                case "H":
                    command=new HorizonFlipCommand();
                    break;

                case "V":
                    command=new VerticalFlipCommand();
                    break;

                default:
                    throw new Exception($"unknown Parameter command type:"+data_arr.ElementAt(3));
            }

            command.StartTime=data_arr.ElementAt(2).ToInt();
            command.EndTime=string.IsNullOrWhiteSpace(data_arr.ElementAt(3)) ? command.StartTime : data_arr.ElementAt(3).ToInt();

            yield return command;
        }
    }
}