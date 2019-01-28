using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

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
            //todo fix logic
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

            CleanCacheAndRecalculateTime();
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
            
            CleanCacheAndRecalculateTime();
        }

        public new void Remove(Command command)
        {
            base.Remove(command);
            CleanCacheAndRecalculateTime();
        }

        public new void Insert(int index, Command command)
        {
            base.Insert(index, command);
            CleanCacheAndRecalculateTime();
        }

        private void CleanCacheAndRecalculateTime()
        {
            //update StartTime/EndTime and command caches.
            first_command=this.FirstOrDefault();
            last_command=this.LastOrDefault();
            StartTime=first_command?.StartTime??0;
            EndTime=Overlay ? this.Max(x => x.EndTime) : Math.Max(EndTime, last_command?.EndTime??EndTime);

            //clear cache
            pick_command_cache=default;
        }

        private (int cache_start_time, int cache_end_time, Command cache_selected_command) pick_command_cache = default;

        public Command PickCommand(float current_time) => Overlay ? PickCommandOverlay(current_time) : PickCommandNormal(current_time);

        public Command PickCommandNormal(float current_time)
        {
            //cache
            if (pick_command_cache.cache_selected_command!=null
                &&pick_command_cache.cache_start_time<=current_time&&current_time<=pick_command_cache.cache_end_time)
                return pick_command_cache.cache_selected_command;

            int min = 0, max = Count-2;

            while (min<=max)
            {
                int i = (max+min)/2;

                var cmd = this[i];
                var next_cmd = this[i+1];

                if (cmd.StartTime<=current_time&&current_time<=next_cmd.StartTime)
                    return UpdatePickCache(cmd.StartTime, next_cmd.StartTime, cmd);

                if (cmd.StartTime>=current_time)
                    max=i-1;
                else
                    min=i+1;
            }

            if (current_time<=first_command.StartTime)
                return UpdatePickCache(int.MinValue, first_command.StartTime, first_command);

            if (current_time>=last_command.StartTime)
                return UpdatePickCache(last_command.StartTime, int.MaxValue, last_command);

            return UpdatePickCache(0, 0, null);

            Command UpdatePickCache(int start_time, int end_time, Command command) => (pick_command_cache=(start_time, end_time, command)).cache_selected_command;
        }

        public Command PickCommandOverlay(float current_time)
        {
            if (current_time<StartTime)
                return first_command;

            if (current_time>EndTime)
                return last_command;

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

                    return cmd;
                }
                //时间夹在两个命令之间，那就选择前者
                else if (next_cmd!=null&&current_time>=cmd.EndTime&&current_time<=next_cmd.StartTime)
                    return cmd;
            }

            return null;

            bool TimeInCommand(Command c) => c.StartTime<=current_time&&current_time<=c.EndTime;
        }

        public override string ToString() => $"{Event} Timeline({StartTime} ~ {EndTime}) Count:{Count} {(Overlay ? "Overlay" : string.Empty)}";
    }
}