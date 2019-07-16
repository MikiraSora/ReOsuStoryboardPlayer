using ReOsuStoryboardPlayer.Core.Commands;
using ReOsuStoryboardPlayer.Core.Parser.CommandParser.ValueCommandParser;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReOsuStoryboardPlayer.Core.Parser.CommandParser
{
    public class CommandParserIntanceDefaultImplement<COMMAND> where COMMAND : Command
    {
        private static ICommandParser _instance;

        public static ICommandParser Instance
        {
            get
            {
                if (_instance == null)
                    _instance = CreateCommandParserIntance();
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
                    throw new Exception("Unknown command name:" + typeof(COMMAND).Name);
            }
        }
    }
}
