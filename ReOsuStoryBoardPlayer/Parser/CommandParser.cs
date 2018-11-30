using ReOsuStoryBoardPlayer.Commands;
using ReOsuStoryBoardPlayer.Parser.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser
{
    public interface ICommandParser
    {
        List<Command> Parse(IEnumerable<string> data_arr,List<Command> result);
    }

    #region Value Command Parsers

    public abstract class IValueCommandParser<T,CMD> : ICommandParser where CMD:ValueCommand<T>,new()
    {
        public abstract int ParamCount { get; }

        public virtual Command Parse(IEnumerable<string> data_arr, int StartTime, int EndTime, T StartValue, T EndValue)
        {
            CMD command = new CMD();

            Easing easingID = (Easing)data_arr.ElementAt(1).ToInt();

            if (!EasingConverter.CacheEasingInterpolatorMap.ContainsKey(easingID))
            {
                Log.Warn($"Cant found the easing = {easingID.ToString()} , will be set Linear");
                command.Easing = EasingConverter.DefaultInterpolator;
            }
            else
                command.Easing = EasingConverter.CacheEasingInterpolatorMap[easingID];

            command.StartTime = StartTime;
            command.EndTime = EndTime;

            command.StartValue = StartValue;
            command.EndValue = EndValue;

            return command;
        }

        public abstract T ConvertValue(IEnumerable<string> p);

        public List<Command> Parse(IEnumerable<string> data_arr, List<Command> result)
        {
            int start_time = data_arr.ElementAt(2).ToInt();
            int end_time =string.IsNullOrWhiteSpace(data_arr.ElementAt(3)) ? start_time : data_arr.ElementAt(3).ToInt();
            int time_duration = end_time - start_time;

            int loop_count = (data_arr.Count() - 4 - ParamCount)/ ParamCount;
            loop_count = loop_count == 0 ? 1 : loop_count;

            for (int i = 0; i < loop_count; i++)
            {
                end_time = start_time + time_duration;

                int pick_base = 4 + ParamCount * i;
                T start_value = ConvertValue(data_arr.Skip(pick_base).Take(ParamCount));
                pick_base += ParamCount;
                T end_value = pick_base >= data_arr.Count() ? start_value : ConvertValue(data_arr.Skip(pick_base).Take(ParamCount));

                var command = Parse(data_arr, start_time, end_time, start_value, end_value);
                result.Add(command);

                start_time = end_time;
            }

            return result;
        }
    }

    public class FloatCommandParser<CMD> : IValueCommandParser<float,CMD> where CMD : ValueCommand<float>, new()
    {
        public override int ParamCount => 1;

        public override float ConvertValue(IEnumerable<string> p) => p.First().ToSigle();
    }

    public class VectorCommandParser<CMD> : IValueCommandParser<Vector, CMD> where CMD : ValueCommand<Vector>, new()
    {
        public override int ParamCount => 2;

        public override Vector ConvertValue(IEnumerable<string> p) => new Vector(p.First().ToSigle(),p.Last().ToSigle());
    }

    public class Vec4CommandParser<CMD> : IValueCommandParser<Vec4, CMD> where CMD : ValueCommand<Vec4>, new()
    {
        public override int ParamCount => 3;

        public override Vec4 ConvertValue(IEnumerable<string> p) => new Vec4(p.ElementAt(0).ToSigle(), p.ElementAt(1).ToSigle(), p.ElementAt(2).ToSigle(), 0);
    }

    #endregion

    public class ParameterCommandParser : ICommandParser
    {
        public List<Command> Parse(IEnumerable<string> data_arr, List<Command> result)
        {
            StateCommand command = null;

            switch (data_arr.ElementAt(4))
            {
                case "A":
                    command = new AdditiveBlendCommand();
                    break;
                case "H":
                    command = new HorizonFlipCommand();
                    break;
                case "V":
                    command = new VerticalFlipCommand();
                    break;
                default:
                    throw new Exception($"unknown Parameter command type:" + data_arr.ElementAt(3));
            }

            command.StartTime = data_arr.ElementAt(2).ToInt();
            command.EndTime = string.IsNullOrWhiteSpace(data_arr.ElementAt(3)) ? command.StartTime : data_arr.ElementAt(3).ToInt();

            result.Add(command);
            return result;
        }
    }

    public class LoopCommandParser : ICommandParser
    {
        public List<Command> Parse(IEnumerable<string> data_arr, List<Command> result)
        {
            LoopCommand command = new LoopCommand();

            command.StartTime = data_arr.ElementAt(1).ToInt();
            command.LoopCount = data_arr.ElementAt(2).ToInt();

            result.Add(command);
            return result;
        }
    }

    public class CommandParserIntance
    {
        public static List<Command> Parse(IEnumerable<string> data_arr, List<Command> result)
        {
            var command_event = data_arr.First().TrimStart(' ', '_');

            switch (command_event)
            {
                case "M":
                    return CommandParserIntance<MoveCommand>.Instance.Parse(data_arr, result);
                case "S":
                    return CommandParserIntance<ScaleCommand>.Instance.Parse(data_arr, result);
                case "V":
                    return CommandParserIntance<VectorScaleCommand>.Instance.Parse(data_arr, result);
                case "MX":
                    return CommandParserIntance<MoveXCommand>.Instance.Parse(data_arr, result);
                case "MY":
                    return CommandParserIntance<MoveYCommand>.Instance.Parse(data_arr, result);
                case "R":
                    return CommandParserIntance<RotateCommand>.Instance.Parse(data_arr, result);
                case "F":
                    return CommandParserIntance<FadeCommand>.Instance.Parse(data_arr, result);
                case "P":
                    return CommandParserIntance<StateCommand>.Instance.Parse(data_arr, result);
                case "L":
                    return CommandParserIntance<LoopCommand>.Instance.Parse(data_arr, result);
                case "C":
                    return CommandParserIntance<ColorCommand>.Instance.Parse(data_arr, result);
                default:
                    throw new Exception("Unknown command event:" + command_event);
            }
        }
    }

    public class CommandParserIntance<COMMAND> where COMMAND:Command
    {
        public static ICommandParser _instance;

        public static ICommandParser Instance
        {
            get{
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
                    return new FloatCommandParser<ScaleCommand>();
                case "MoveCommand":
                    return new VectorCommandParser<MoveCommand>();
                case "VectorScaleCommand":
                    return new VectorCommandParser<VectorScaleCommand>();
                case "ColorCommand":
                    return new Vec4CommandParser<ColorCommand>();
                case "LoopCommand":
                    return new LoopCommandParser();
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
