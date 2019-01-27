using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ReOsuStoryboardPlayer.Core.Commands
{
    public class CommandTimeline : List<Command>
    {
        public int StartTime = int.MaxValue;
        public int EndTime;

        public bool Overlay { get; set; }

        public Event Event => first_command?.Event??Event.Unknown;

        private Command last_command, first_command;

        public new void Add(Command command)
        {
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

            UpdateFirstLastCommand();
        }

        public void SortCommands()
        {
            Sort((x, y) =>
            {
                var z = x.StartTime-y.StartTime;

                if (z!=0)
                    return z;

                return x.EndTime-y.EndTime;
            });
            
            UpdateFirstLastCommand();
        }

        public new void Remove(Command command)
        {
            base.Remove(command);
            UpdateFirstLastCommand();
        }

        public new void Insert(int index, Command command)
        {
            base.Insert(index, command);
            UpdateFirstLastCommand();
        }

        private void UpdateFirstLastCommand()
        {
            //update StartTime/EndTime and command caches.
            first_command=this.FirstOrDefault();
            last_command=this.LastOrDefault();
            StartTime=first_command?.StartTime??0;
            EndTime=Overlay ? this.Max(x => x.EndTime) : Math.Max(EndTime, last_command?.EndTime??EndTime);
        }

        private Command selected_command;

        public Command PickCommand(float current_time)
        {
            //Debug.Assert(!float.IsNaN(current_time), $"current_time is not a number");

            if (Count==0)
                return null;

            //cache
            if (selected_command!=null
                &&TimeInCommand(selected_command)
                &&(!Overlay))
                return selected_command;

            if (current_time<StartTime)
                return selected_command=first_command;

            if (current_time>EndTime)
                return selected_command=last_command;

            for (int i = 0; i<Count; i++)
            {
                var cmd = this[i];
                var next_cmd = (i<Count-1) ? this[i+1] : null;

                //尝试选取在时间范围内的命令
                if (TimeInCommand(cmd))
                {
                    if (Overlay&&next_cmd!=null)
                    {
                        //判断下一个命令(可能是overlay command)是否也是在范围内，是的话就跳到下一个命令判断
                        if (TimeInCommand(next_cmd))
                            continue;
                    }

                    return selected_command=cmd;
                }
                //时间夹在两个命令之间，那就选择前者
                else if (next_cmd!=null&&current_time>=cmd.EndTime&&current_time<=next_cmd.StartTime)
                    return selected_command=cmd;
            }

            return selected_command=null;

            bool TimeInCommand(Command c) => (c.StartTime<=current_time&&current_time<=c.EndTime);
        }

        public override string ToString() => $"{Event} Timeline({StartTime} ~ {EndTime}) Count:{Count} {(Overlay ? "Overlay" : string.Empty)}";
    }
}