using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Commands
{
    class LoopCommand : GroupCommand
    {
        public LoopCommand() => Event = Event.Loop;

        public int CostTime { get; private set; }

        public int LoopCount { get; set; }
        
        public void AddSubCommandsAndUpdate(IEnumerable<Command> commands)
        {
            AddSubCommand(commands);
            UpdateParam();
        }

        public void UpdateParam()
        {
            /*
            //整理自己子命令的时间轴
            foreach (var timeline in this.SubCommands)
                timeline.Value.Sort();
            */

            /*
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

            CostTime = SubCommands.SelectMany(l=>l.Value).Max(c=>c.EndTime) - SubCommands.SelectMany(l => l.Value).Min(c => c.StartTime);
            var total_cast_time = CostTime * LoopCount;
            
            EndTime = StartTime + total_cast_time;

            foreach (var list in SubCommands.Values)
                list.Sort();
        }

        public override void Execute(StoryBoardObject @object, float current_value)
        {
            //throw new NotImplementedException("此处已经迁移到_GroupSubTimelineCommand");
            /*
            int recovery_time = (int)(current_value - StartTime);

            int current_time = recovery_time % CostTime;

            int current_loop_index = recovery_time / CostTime;

            foreach (var command_list in SubCommands.Values)
            {
                var command = command_list.PickCommand(current_time);

                if (command != null)
                {
                    //store command start/end time
                    var offset_time = StartTime + current_loop_index * CostTime;
                    command.StartTime += offset_time;
                    command.EndTime += offset_time;

                    command.Execute(@object, current_time);
                    @object.MarkCommandExecuted(command);

                    //restore command
                    command.StartTime -= offset_time;
                    command.EndTime -= offset_time;
                }
            }
            */
        }
    }
}
