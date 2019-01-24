using ReOsuStoryBoardPlayer.Core.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ReOsuStoryBoardPlayer.Core.Commands
{
    public class CommandTimeline : List<Command>
    {
        public int StartTime=int.MaxValue;
        public int EndTime;

        public bool Overlay { get; set; }

        public Event Event=Event.Unknown;

        public new void Add(Command command)
        {
            Debug.Assert(((Event==Event.Unknown ? (Event=command.Event) : Event)==Event) && Event==command.Event,"Not allow to add different event command");

            //check overlay
            if (Count>=1)
            {
                var prev_cmd = this.Last();

                if (command.EndTime<=prev_cmd.EndTime||command.StartTime<prev_cmd.EndTime)
                {
                    Overlay=true;

                    if (Setting.ShowProfileSuggest&&(command.StartTime!=command.EndTime))
                        Log.User($"{command} is conflict with {prev_cmd}");
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

            if (Count==0)
                return null;

            //cache
            if (selected_command!=null
                &&selected_command.StartTime<=current_time&&current_time<=selected_command.EndTime
                &&(!Overlay))
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
                    if (Overlay&&next_cmd!=null)
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

        public override string ToString() => $"{Event} Timeline({StartTime} ~ {EndTime}) Count:{Count} {(Overlay ? "Overlay":string.Empty)}";
    }
}