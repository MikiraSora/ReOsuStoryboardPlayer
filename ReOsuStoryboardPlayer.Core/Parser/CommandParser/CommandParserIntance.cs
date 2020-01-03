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
        public static IEnumerable<Command> Parse(string command_line) => Parse(command_line.Split(','));

        public static IEnumerable<Command> Parse(IEnumerable<string> data_arr)
        {
            var command_event = data_arr.First().TrimStart(' ', '_');

            IEnumerable<Command> result = null;

            switch (command_event)
            {
                case "M":
                    result=CommandParserIntance<MoveCommand>.Instance.Parse(data_arr);
                    break;

                case "S":
                    result=CommandParserIntance<ScaleCommand>.Instance.Parse(data_arr);
                    break;

                case "V":
                    result=CommandParserIntance<VectorScaleCommand>.Instance.Parse(data_arr);
                    break;

                case "MX":
                    result=CommandParserIntance<MoveXCommand>.Instance.Parse(data_arr);
                    break;

                case "MY":
                    result=CommandParserIntance<MoveYCommand>.Instance.Parse(data_arr);
                    break;

                case "R":
                    result=CommandParserIntance<RotateCommand>.Instance.Parse(data_arr);
                    break;

                case "F":
                    result=CommandParserIntance<FadeCommand>.Instance.Parse(data_arr);
                    break;

                case "P":
                    result=CommandParserIntance<StateCommand>.Instance.Parse(data_arr);
                    break;

                case "L":
                    result=CommandParserIntance<LoopCommand>.Instance.Parse(data_arr);
                    break;

                case "C":
                    result=CommandParserIntance<ColorCommand>.Instance.Parse(data_arr);
                    break;

                case "T":
                    result=CommandParserIntance<TriggerCommand>.Instance.Parse(data_arr);
                    break;

                default:
                    throw new Exception("Unknown command event:"+command_event);
            }

            foreach (var cmd in result)
            {
                yield return cmd;
            }
        }
    }

    public class CommandParserIntance<COMMAND> where COMMAND : Command
    {
        private static ICommandParser _instance;

        public static ICommandParser Instance
        {
            get
            {
                if (_instance==null)
                    _instance=CreateCommandParserIntance();
                return _instance;
            }
        }

        private static ICommandParser CreateCommandParserIntance()
        {
            switch (typeof(COMMAND).Name)
            {
                case "FadeCommand":
                    return new FloatCommandParser<FadeCommand>();

                case "MoveXCommand":
                    return new FloatCommandParser<MoveXCommand>();

                case "MoveYCommand":
                    return new FloatCommandParser<MoveYCommand>();

                case "RotateCommand":
                    return new FloatCommandParser<RotateCommand>();

                case "ScaleCommand":
                    return Setting.EnableSplitMoveScaleCommand ? new SplitableScaleCommandParser() : new FloatCommandParser<ScaleCommand>();

                case "MoveCommand":
                    return Setting.EnableSplitMoveScaleCommand ? new SplitableMoveCommandParser() : new VectorCommandParser<MoveCommand>();

                case "VectorScaleCommand":
                    return new VectorCommandParser<VectorScaleCommand>();

                case "ColorCommand":
                    return new ByteVec4CommandParser<ColorCommand>();

                case "LoopCommand":
                    return new LoopCommandParser();

                case "TriggerCommand":
                    return new TriggerCommandParser();

                case "StateCommand":
                case "AdditiveBlendCommand":
                case "HorizonFlipCommand":
                case "VerticalFlipCommand":
                    return new ParameterCommandParser();

                default:
                    throw new Exception("Unknown command name:"+typeof(COMMAND).Name);
            }
        }
    }
}