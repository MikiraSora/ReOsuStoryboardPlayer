using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ReOsuStoryBoardPlayer.Commands
{
    public class CommandTimeline : List<Command>
    {
        public int StartTime;
        public int EndTime;

        private bool overlay;

        public new void Add(Command command)
        {
            //check overlay
            if (Count>0)
            {
                var prev_cmd = this.Last();

                if (prev_cmd.StartTime<=command.StartTime&&command.EndTime<=prev_cmd.EndTime)
                {
                    overlay=true;
                }
            }

            base.Add(command);

            //update StartTime/EndTime
            StartTime=Math.Min(StartTime, command.StartTime);
            EndTime=Math.Max(EndTime, command.EndTime);
        }

        private Command selected_command;

        public Command PickCommand(float current_time)
        {
            Debug.Assert(!float.IsNaN(current_time), $"current_time is not a number");

            //cache
            if (selected_command!=null
                &&selected_command.StartTime<=current_time&&current_time<=selected_command.EndTime
                &&(!overlay))
                return selected_command;

            if (current_time<this.First().StartTime)
                return selected_command=this.First();
            else if (current_time>this.Last().EndTime)
                return selected_command=this.Last();

            //尝试选取在时间范围内的命令
            for (int i = 0; i<Count; i++)
            {
                var cmd = this[i];
                var next_cmd = (i<Count-1) ? this[i+1] : null;

                if (TimeInCommand(cmd))
                {
                    if (overlay&&next_cmd!=null)
                    {
                        //判断下一个命令(可能是overlay command)是否也是在范围内
                        if (TimeInCommand(next_cmd))
                            return selected_command=next_cmd;
                    }

                    return selected_command=cmd;
                }
                else if (next_cmd!=null&&current_time>=cmd.EndTime&&current_time<=next_cmd.StartTime)
                    return selected_command=cmd;
            }

            return selected_command=null;

            bool TimeInCommand(Command c) => (current_time>=c.StartTime&&current_time<=c.EndTime);
        }
    }
}