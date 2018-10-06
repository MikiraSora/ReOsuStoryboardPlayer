﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Commands
{
    public class CommandTimeline:List<Command>
    {
        public int StartTime => this.Min(c => c.StartTime);
        public int EndTime => this.Max(c => c.EndTime);

        public virtual List<Command> PickCommands(float time, List<Command> result)
        {
            var command = PickCommand(time);
            result.Add(command);
            return result;
        }

        public new void Add(Command command)
        {
            Debug.Assert(Count==0 || command.Event == this.First().Event);
            base.Add(command);
            Sort();
        }

        Command selected_command;

        public Command PickCommand(float current_time)
        {
            Command command = null;

            if (selected_command != null && (selected_command.StartTime <= current_time && current_time <= selected_command.EndTime))
                return selected_command;

            if (current_time < this.First().StartTime)
                return selected_command=this.First();
            else if (current_time > this.Last().EndTime)
                return selected_command=this.Last();

            //尝试选取在时间范围内的命令
            if (command == null)
            {
                foreach (var cmd in this)
                {
                    if (current_time >= cmd.StartTime && current_time <= cmd.EndTime)
                    {
                        return selected_command=cmd;
                    }
                }
            }

            //尝试选取在命令之间的前者
            if (command == null)
            {
                for (int i = 0; i < Count - 1; i++)
                {
                    var cur_cmd = this[i];
                    var next_cmd = this[i + 1];

                    if (current_time >= cur_cmd.EndTime && current_time <= next_cmd.StartTime)
                    {
                        return selected_command=cur_cmd;
                    }
                }
            }

            return selected_command=null;
        }
    }

    public class LoopCommandTimeline : CommandTimeline
    {
        public override List<Command> PickCommands(float time, List<Command> result)
        {
            //每个物件可能有多个Loop,全都执行了，至于命令先后循序，交给DispatchCommandExecute()判断
            result.AddRange(this);
            return result;
        }
    }
}