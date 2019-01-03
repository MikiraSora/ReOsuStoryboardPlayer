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
        HashSet<TriggerCommand> register_triggers = new HashSet<TriggerCommand>();

        public void Add(TriggerCommand command)
        {
            Debug.Assert(!register_triggers.Contains(command),"Not allow to add trigger repeatly.");

            register_triggers.Add(command);
        }

        public void Remove(TriggerCommand trigger_command)
        {
            register_triggers.Remove(trigger_command);
        }

        public void Trig(HitSoundInfo hit_sound,float current_time)
        {
            foreach (var command in register_triggers
                .Where(x=> x.CheckTimeVaild(current_time)
                    &&x.Condition is HitSoundTriggerCondition condition
                    &&condition.CheckCondition(hit_sound)))
            {
                command.Trig();
            }
        }

        public void Trig(GameState state, float current_time)
        {
            foreach (var command in register_triggers
                .Where(x => x.CheckTimeVaild(current_time)
                    &&x.Condition is GameStateTriggerCondition condition
                    &&condition.CheckCondition(state)))
            {
                command.Trig();
            }
        }

        public static TriggerListener DefaultListener { get; private set; } = new TriggerListener();
    }
}
