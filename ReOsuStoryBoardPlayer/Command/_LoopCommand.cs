using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Commands
{
    class _LoopCommand : _GroupCommand
    {
        public _LoopCommand() => Event = Event.Loop;

        public int CostTime { get; private set; }

        public int LoopCount { get; set; }
        
        public void AddSubCommandsAndUpdate(IEnumerable<_Command> commands)
        {
            AddSubCommand(commands);
            UpdateParam();
        }

        public void UpdateParam()
        {
            var total_command_list = sub_commands.Values.SelectMany(l=>l).OrderBy(c => c.StartTime);

            int first_start_time = total_command_list.First().StartTime;

            int fix_start_time = total_command_list.First().StartTime;

            //因为Loop子命令是可以有offset的，所以在这就把那些子命令减去共同的offset
            foreach (var sub_cmd in total_command_list)
            {
                sub_cmd.StartTime -= fix_start_time;
                sub_cmd.EndTime -= fix_start_time;
            }

            /*
            for (int index = 0; index < total_command_list.Count(); index++)
            {
                var sub_command = total_command_list.ElementAt(index);
                current_end_time += sub_command.EndTime - sub_command.StartTime;

                var prev_sub_command_time = index==0?0: total_command_list.Last().EndTime;
                current_end_time += sub_command.StartTime - prev_sub_command_time;
            }
            */

            CostTime = total_command_list.Max(c=>c.EndTime) - total_command_list.Min(c => c.StartTime);
            var total_cast_time = CostTime * LoopCount;

            StartTime += first_start_time;
            EndTime = StartTime + total_cast_time;

            foreach (var list in sub_commands.Values)
                list.Sort();
        }

        public override void Execute(StoryBoardObject @object, float current_value)
        {
            int recovery_time = (int)((current_value - StartTime));

            int current_time = recovery_time % CostTime;

            int current_loop_index = recovery_time / CostTime;

            foreach (var command_list in sub_commands.Values)
            {
                var command = command_list.PickCommand(current_time);

                if (command != null)
                {
                    //store command start/end time
                    var offset_time = StartTime + current_loop_index * CostTime;
                    command.StartTime += offset_time;
                    command.EndTime += offset_time;

                    command.Execute(@object, current_time);

                    //restore command
                    command.StartTime -= offset_time;
                    command.EndTime -= offset_time;
                }
            }
        }
    }
}
