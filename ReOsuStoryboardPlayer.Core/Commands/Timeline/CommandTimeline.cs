using ReOsuStoryboardPlayer.Core.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ReOsuStoryboardPlayer.Core.Commands
{
    public class CommandTimeline : IEnumerable<Command>
    {
        public List<Command> commands = new List<Command>();

        public Event Event => first_command?.Event??Event.Unknown;

        private Command last_command, first_command;

        public int StartTime = int.MaxValue;

        public int EndTime;

        public bool Overlay { get; set; }

        public int Count => commands.Count;

        public Command this[int index] => commands[index];

        public IEnumerator<Command> GetEnumerator() => ((IEnumerable<Command>)commands).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private int BinarySearchInsertableIndex(Command command)
        {
            var insert = commands.BinarySearch(command);
            insert=insert<0 ? ~insert : insert;
            return insert;
        }

        /// <summary>
        /// 检查是否存在命令冲突
        /// **必须确保此集合已排序**
        /// O(n)
        /// </summary>
        /// <param name="fast_index"></param>
        private void FastOverlayCheck()
        {
            /*
             |----------------| cmd
                              |----|  itor1
                                     |---| itor2
             */

            Overlay=false;

            for (int i = 0; i<commands.Count; i++)
            {
                var cmd = commands[i];

                var itor_i = i+1;

                while (itor_i<commands.Count)
                {
                    var itor = commands[itor_i];

                    if (itor.StartTime>=cmd.EndTime)
                        break;

                    Overlay=true;
                    return;
                }
            }
        }

        private void CleanCacheAndRecalculateTime()
        {
            //update StartTime/EndTime and command caches.
            first_command=commands.FirstOrDefault();
            last_command=commands.LastOrDefault(); StartTime=first_command?.StartTime??0;
            EndTime=Overlay ? this.Max(x => x.EndTime) : Math.Max(EndTime, last_command?.EndTime??EndTime);

            //clear cache
            pick_command_cache=default;
        }

        #region Collection Methods

        public void Add(Command command)
        {
            var insert_index = BinarySearchInsertableIndex(command);

            commands.Insert(insert_index, command);

            FastOverlayCheck();

            CleanCacheAndRecalculateTime();
        }

        public void AddRange(IEnumerable<Command> commands)
        {
            //todo:还能平衡一下
            if (commands.Count()<100)
            {
                foreach (var command in commands)
                {
                    var insert_index = BinarySearchInsertableIndex(command);
                    this.commands.Insert(insert_index, command);
                }
            }
            else
            {
                this.commands.AddRange(commands);

                this.commands.Sort();
            }

            FastOverlayCheck();

            CleanCacheAndRecalculateTime();
        }

        public void Remove(Command command)
        {
            commands.Remove(command);

            CleanCacheAndRecalculateTime();
            FastOverlayCheck();
        }

        public void RemoveRange(IEnumerable<Command> commands)
        {
            foreach (var command in commands)
                this.commands.Remove(command);

            CleanCacheAndRecalculateTime();
            FastOverlayCheck();
        }

        public int IndexOf(Command cmd) => commands.IndexOf(cmd);

        #endregion Collection Methods

        private (int cache_start_time, int cache_end_time, Command cache_selected_command) pick_command_cache = default;

        public Command PickCommand(float current_time) => Overlay ? PickCommandOverlay(current_time) : PickCommandNormal(current_time);

        public Command PickCommandNormal(float current_time)
        {
            //cache
            if (pick_command_cache.cache_selected_command!=null
                &&pick_command_cache.cache_start_time<=current_time&&current_time<=pick_command_cache.cache_end_time)
                return pick_command_cache.cache_selected_command;

            int min = 0, max = commands.Count-2;

            while (min<=max)
            {
                int i = (max+min)/2;

                var cmd = commands[i];
                var next_cmd = commands[i+1];

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
            
            for (int i = 0; i<commands.Count; i++)
            {
                var cmd = commands[i];
                var next_cmd = (i<commands.Count-1) ? commands[i+1] : null;

                //尝试选取在时间范围内的命令
                if (TimeInCommand(cmd))
                {
                    if (next_cmd!=null)
                    {
                        //判断被覆盖的命令有没有在范围内，是的话就跳到下一个命令判断
                        /* LINQ WILL TAKE YOU INTO GC HELL
                        var is_overlay_others = Enumerable
                            .Range(i+1, commands.Count)
                            .Select(x => commands[x])
                            .TakeWhile(x => x.StartTime<=cmd.EndTime)
                            .Any(x=>TimeInCommand(x));
                        */

                        bool is_overlay_others = false;

                        for (int x = i+1; x<commands.Count; x++)
                        {
                            var check_overlay_cmd = commands[x];

                            if (check_overlay_cmd.StartTime>cmd.EndTime)
                                break;

                            if (TimeInCommand(check_overlay_cmd))
                            {
                                is_overlay_others=true;
                                break;
                            }
                        }

                        if (is_overlay_others)
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

        public override string ToString() => $"{Event} Timeline({StartTime} ~ {EndTime}) Count:{commands.Count} {(Overlay ? "Overlay" : string.Empty)}";
    }
}