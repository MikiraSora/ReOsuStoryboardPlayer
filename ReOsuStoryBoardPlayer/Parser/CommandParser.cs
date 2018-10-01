using ReOsuStoryBoardPlayer.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser
{
    public interface ICommandParser
    {
        List<_Command> Parse(IEnumerable<string> data_arr,List<_Command> result);
    }

    #region Value Command Parsers

    public abstract class IValueCommandParser<T,CMD> : ICommandParser where CMD:_ValueCommand<T>,new()
    {
        public abstract int ParamCount { get; }

        public virtual _Command Parse(IEnumerable<string> data_arr, int StartTime, int EndTime, T StartValue, T EndValue)
        {
            CMD command = new CMD();

            Easing easingID = (Easing)int.Parse(data_arr.ElementAt(1).Trim());

            if (!EasingConverter.CacheEasingInterpolatorMap.ContainsKey(easingID))
            {
                Log.Warn($"Cant found the easing = {easingID.ToString()} , will be set Linear");
                command.Easing = EasingConverter.CacheEasingInterpolatorMap[Easing.Linear];
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

        public List<_Command> Parse(IEnumerable<string> data_arr, List<_Command> result)
        {
            int start_time = int.Parse(data_arr.ElementAt(2));
            int end_time = string.IsNullOrWhiteSpace(data_arr.ElementAt(3)) ? start_time : int.Parse(data_arr.ElementAt(3));
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

    public class FloatCommandParser<CMD> : IValueCommandParser<float,CMD> where CMD : _ValueCommand<float>, new()
    {
        public override int ParamCount => 1;

        public override float ConvertValue(IEnumerable<string> p) => float.Parse(p.First());
    }

    public class VectorCommandParser<CMD> : IValueCommandParser<Vector, CMD> where CMD : _ValueCommand<Vector>, new()
    {
        public override int ParamCount => 2;

        public override Vector ConvertValue(IEnumerable<string> p) => new Vector(float.Parse(p.First()), float.Parse(p.Last()));
    }

    public class Vec4CommandParser<CMD> : IValueCommandParser<Vec4, CMD> where CMD : _ValueCommand<Vec4>, new()
    {
        public override int ParamCount => 3;

        public override Vec4 ConvertValue(IEnumerable<string> p) => new Vec4(float.Parse(p.ElementAt(0)), float.Parse(p.ElementAt(1)), float.Parse(p.ElementAt(2)), 0);
    }

    #endregion

    public class ParameterCommandParser : ICommandParser
    {
        public List<_Command> Parse(IEnumerable<string> data_arr, List<_Command> result)
        {
            _StateCommand command = null;

            switch (data_arr.ElementAt(4))
            {
                case "A":
                    command = new _AdditiveBlendCommand();
                    break;
                case "H":
                    command = new _HorizonFlipCommand();
                    break;
                case "V":
                    command = new _VerticalFlipCommand();
                    break;
                default:
                    throw new Exception($"unknown Parameter command type:" + data_arr.ElementAt(3));
            }

            command.StartTime = int.Parse(data_arr.ElementAt(2));
            command.EndTime = string.IsNullOrWhiteSpace(data_arr.ElementAt(3)) ? command.StartTime : int.Parse(data_arr.ElementAt(3));

            result.Add(command);
            return result;
        }
    }

    public class LoopCommandParser : ICommandParser
    {
        public List<_Command> Parse(IEnumerable<string> data_arr, List<_Command> result)
        {
            _LoopCommand command = new _LoopCommand();

            command.StartTime = int.Parse(data_arr.ElementAt(1));
            command.LoopCount = int.Parse(data_arr.ElementAt(2));

            result.Add(command);
            return result;
        }
    }

    public class CommandParserIntance
    {
        public static List<_Command> Parse(IEnumerable<string> data_arr, List<_Command> result)
        {
            var command_event = data_arr.First().Trim('_', ' ');

            switch (command_event)
            {
                case "M":
                    return CommandParserIntance<_MoveCommand>.Instance.Parse(data_arr, result);
                case "S":
                    return CommandParserIntance<_ScaleCommand>.Instance.Parse(data_arr, result);
                case "V":
                    return CommandParserIntance<_VectorScaleCommand>.Instance.Parse(data_arr, result);
                case "MX":
                    return CommandParserIntance<_MoveXCommand>.Instance.Parse(data_arr, result);
                case "MY":
                    return CommandParserIntance<_MoveYCommand>.Instance.Parse(data_arr, result);
                case "R":
                    return CommandParserIntance<_RotateCommand>.Instance.Parse(data_arr, result);
                case "F":
                    return CommandParserIntance<_FadeCommand>.Instance.Parse(data_arr, result);
                case "P":
                    return CommandParserIntance<_StateCommand>.Instance.Parse(data_arr, result);
                case "L":
                    return CommandParserIntance<_LoopCommand>.Instance.Parse(data_arr, result);
                case "C":
                    return CommandParserIntance<_ColorCommand>.Instance.Parse(data_arr, result);
                default:
                    throw new Exception("Unknown command event:" + command_event);
            }
        }
    }

    public class CommandParserIntance<COMMAND> where COMMAND:_Command
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
                case "_FadeCommand":
                    return new FloatCommandParser<_FadeCommand>();
                case "_MoveXCommand":
                    return new FloatCommandParser<_MoveXCommand>();
                case "_MoveYCommand":
                    return new FloatCommandParser<_MoveYCommand>();
                case "_RotateCommand":
                    return new FloatCommandParser<_RotateCommand>();
                case "_ScaleCommand":
                    return new FloatCommandParser<_ScaleCommand>();
                case "_MoveCommand":
                    return new VectorCommandParser<_MoveCommand>();
                case "_VectorScaleCommand":
                    return new VectorCommandParser<_VectorScaleCommand>();
                case "_ColorCommand":
                    return new Vec4CommandParser<_ColorCommand>();
                case "_LoopCommand":
                    return new LoopCommandParser();
                case "_StateCommand":
                case "_AdditiveBlendCommand":
                case "_HorizonFlipCommand":
                case "_VerticalFlipCommand":
                    return new ParameterCommandParser();
                default:
                    throw new Exception("Unknown command name:" + typeof(COMMAND).Name);
            }
        }
    }
}
