using ReOsuStoryBoardPlayer.Commands;
using ReOsuStoryBoardPlayer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser.CommandParser
{
    public abstract class IValueCommandParser<T, CMD> : ICommandParser where CMD : ValueCommand<T>, new()
    {
        public abstract int ParamCount { get; }

        public Command Parse(IEnumerable<string> data_arr, int StartTime, int EndTime, T StartValue, T EndValue)
        {
            CMD command = new CMD();

            command.Easing = (EasingTypes)data_arr.ElementAt(1).ToInt();
            
            
            if (Setting.FunReverseEasing)
                command.Easing=Interpolation.GetReverseEasing(command.Easing);
                

            command.StartTime=StartTime;
            command.EndTime=EndTime;

            command.StartValue=StartValue;
            command.EndValue=EndValue;

            return command;
        }

        public abstract T ConvertValue(IEnumerable<string> p);

        public virtual IEnumerable<Command> Parse(IEnumerable<string> data_arr)
        {
            int start_time = data_arr.ElementAt(2).ToInt();
            int end_time = string.IsNullOrWhiteSpace(data_arr.ElementAt(3)) ? start_time : data_arr.ElementAt(3).ToInt();
            int time_duration = end_time-start_time;

            int loop_count = (data_arr.Count()-4-ParamCount)/ParamCount;
            loop_count=loop_count==0 ? 1 : loop_count;

            for (int i = 0; i<loop_count; i++)
            {
                end_time=start_time+time_duration;

                int pick_base = 4+ParamCount*i;
                T start_value = ConvertValue(data_arr.Skip(pick_base).Take(ParamCount));
                pick_base+=ParamCount;
                T end_value = pick_base>=data_arr.Count() ? start_value : ConvertValue(data_arr.Skip(pick_base).Take(ParamCount));

                var command = Parse(data_arr, start_time, end_time, start_value, end_value);
                yield return command;

                start_time=end_time;
            }
        }
    }
}
