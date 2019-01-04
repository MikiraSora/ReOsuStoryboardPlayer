using ReOsuStoryBoardPlayer.Commands.Group.Trigger.TriggerCondition;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ReOsuStoryBoardPlayer.Commands.Group.Trigger.TriggerCondition.HitSoundTriggerCondition;

namespace ReOsuStoryBoardPlayer.Commands.Group.Trigger
{
    public class TriggerListener
    {
        Dictionary<int, HashSet<TriggerCommand>> register_group_triggers = new Dictionary<int, HashSet<TriggerCommand>>();
       
        public void Add(TriggerCommand command)
        {
            if (!register_group_triggers.TryGetValue(command.GroupID,out var sets))
            {
                register_group_triggers[command.GroupID]=new HashSet<TriggerCommand>();
            }

            register_group_triggers[command.GroupID].Add(command);
            command.Reset();
        }

        public void Remove(TriggerCommand trigger_command)
        {
            foreach (var sets in register_group_triggers.Values)
                sets.Remove(trigger_command);
        }

        private TriggerCommand PickVaildTrigger(IEnumerable<TriggerCommand> commands,float current_time)
        {
            return commands.FirstOrDefault(x => x.CheckTimeVaild(current_time));
        }

        public void Trig(HitSoundInfo hit_sound,float current_time)
        {
            foreach (var register_triggers in register_group_triggers.Values)
            {
                var cmd = PickVaildTrigger(register_triggers, current_time);

                if (cmd.Condition is HitSoundTriggerCondition condition
                    &&condition.CheckCondition(hit_sound))
                {
                    cmd.Trig();
                }
            }
        }

        public void Trig(GameState state, float current_time)
        {
            foreach (var register_triggers in register_group_triggers.Values)
            {
                var cmd = PickVaildTrigger(register_triggers, current_time);

                if (cmd.Condition is GameStateTriggerCondition condition
                    &&condition.CheckCondition(state))
                {
                    cmd.Trig();
                }
            }
        }

        /// <summary>
        /// 当时间轴回滚的时候就得清除触发器的子命令，然后重置状态
        /// </summary>
        public void Reset()
        {
            foreach (var trigger in register_group_triggers.Values.SelectMany(l=>l))
            {
                trigger.Reset();
            }
        }

        public static TriggerListener DefaultListener { get; private set; } = new TriggerListener();
    }
}
