using ReOsuStoryBoardPlayer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Commands
{
    /// <summary>
    /// 检查命令是否和之前命令冲突的封装类
    /// * 暂时废弃（毕竟这货只是用来解决Loop子命令和外部命令的先后执行顺序）
    /// </summary>
    public class CommandConflictChecker
    {
        class RegisterData
        {
            public Command command { get; set; }
            public int StartTime { get; set; }
            public int EndTime { get; set; }
        }

        private RegisterData[] ExecutedCommandRegisterMap { get; } = new RegisterData[Enum.GetValues(typeof(Event)).Length];

        public bool CheckIfConflict(Command command,float current_playing_time)
        {
            var reg_cmd_info = ExecutedCommandRegisterMap[(int)command.Event];
                
            if (reg_cmd_info==null)
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

        public void ForceUpdate(Command command)
        {
            var data = ObjectPool<RegisterData>.Instance.GetObject();

            data.command = command;
            data.StartTime = command.StartTime;
            data.EndTime = command.EndTime;

            var prev_command = ExecutedCommandRegisterMap[(int)command.Event]?.command;

#if DEBUG
            if (prev_command != null)
                prev_command.IsExecuted = false;
#endif

            ExecutedCommandRegisterMap[(int)command.Event] = data;
        }

        public bool CheckIfConflictThenUpdate(Command command, float current_playing_time)
        {
            if (!CheckIfConflict(command, current_playing_time))
                return false;
            ForceUpdate(command);
            return true;
        }

        public void Reset()
        {
            foreach (var data in ExecutedCommandRegisterMap)
                ObjectPool<RegisterData>.Instance.PutObject(data);

            Array.Clear(ExecutedCommandRegisterMap,0, ExecutedCommandRegisterMap.Length);
        }
    }
}
