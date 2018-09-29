using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Commands
{
    public class CommandTimeline:List<_Command>
    {
        public int StartTime => this.Min(c => c.StartTime);
        public int EndTime => this.Max(c => c.EndTime);

        public virtual List<_Command> PickCommands(float time, List<_Command> result)
        {
            var command = PickCommand(time);
            result.Add(command);
            return result;
        }

        public new void Add(_Command command)
        {
            base.Add(command);
            Sort();
        }


        public _Command PickCommand(float current_time)
        {
            _Command command = null;
            if (current_time < this.First().StartTime)
            {
                //早于开始前
                return this.First();
            }
            else if (current_time > this.Last().EndTime)
            {
                //迟于结束后
                return this.Last();
            }

            //尝试选取在时间范围内的命令
            if (command == null)
            {
                foreach (var cmd in this)
                {
                    if (current_time >= cmd.StartTime && current_time <= cmd.EndTime)
                    {
                        return cmd;
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
                        return cur_cmd;
                    }
                }
            }

            return null;
        }
    }

    public class LoopCommandTimeline : CommandTimeline
    {
        public override List<_Command> PickCommands(float time, List<_Command> result)
        {
            //每个物件可能有多个Loop,全都执行了，至于命令先后循序，交给DispatchCommandExecute()判断
            result.AddRange(this);
            return result;
        }
    }
}
