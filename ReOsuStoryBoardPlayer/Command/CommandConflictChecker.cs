using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Commands
{
    /// <summary>
    /// 检查命令是否和之前命令冲突的封装类
    /// </summary>
    public class CommandConflictChecker
    {
        private Dictionary<Event,(_Command command, int StartTime, int EndTime)> ExecutedCommandRegisterMap { get; } = new Dictionary<Event, (_Command command, int StartTime, int EndTime)>();

        public bool CheckIfConflict(_Command command,float current_playing_time)
        {
            if (!ExecutedCommandRegisterMap.TryGetValue(command.Event,out var reg_cmd_info))
                return true;

            /*
             如果之前有同类型命令执行了，比如Loop里面的子命令
             将之前的命令和现在的命令当做在一条时间轴上判断
                                                                      (allow)
                                                                      |
             |-----reg_cmd_a-----|    o   |------current_cmd------|   o  |-----reg_cmd_b-----|
                                      |
                                      current_playing_time(reject)
             至于考虑重叠的，不考虑了
             */
            if (
                //是否存在已执行命令
                reg_cmd_info.command != null && (
                    //已执行的命令比现在还靠后
                    command.EndTime <= reg_cmd_info.command.StartTime ||

                    //现在命令比已执行命令还靠后，但当前时间在现命令之前
                    (reg_cmd_info.command.StartTime < command.StartTime && current_playing_time < command.StartTime)
                ))
                return false;//不给执行
            
            return true;
        }

        public void ForceUpdate(_Command command)
        {
            ExecutedCommandRegisterMap[command.Event] = (command, command.StartTime, command.EndTime);
        }

        public bool CheckIfConflictThenUpdate(_Command command, float current_playing_time)
        {
            if (!CheckIfConflict(command, current_playing_time))
                return false;
            ForceUpdate(command);
            return true;
        }

        public void Reset()
        {
            ExecutedCommandRegisterMap.Clear();
        }
    }
}
