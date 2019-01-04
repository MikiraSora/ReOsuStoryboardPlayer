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
        HashSet<StoryBoardObject> register_trigger_objects = new HashSet<StoryBoardObject>();
       
        public void Add(StoryBoardObject @object)
        {
            register_trigger_objects.Add(@object);
        }

        public void Remove(StoryBoardObject @object)
        {
            register_trigger_objects.Remove(@object);
        }

        private TriggerCommand PickVaildTrigger(IEnumerable<TriggerCommand> commands,float current_time)
        {
            return commands.FirstOrDefault(x => x.CheckTimeVaild(current_time));
        }

        public void Trig(HitSoundInfo hit_sound, float current_time)
        {
            foreach (var obj in register_trigger_objects)
            {
                foreach (var pair in obj.Triggers)
                {
                    IEnumerable<TriggerCommand> commands = 
                        pair.Key==0 ? //0 Group里面的Trigger相互不冲突，除此之外的Group都会只能执行一个Trigger(后者优先）
                        pair.Value.Where(x => x.CheckTimeVaild(current_time)) 
                        : pair.Value.Reverse().Where(x => x.CheckTimeVaild(current_time)).Take(1);

                    foreach (var cmd in commands)
                    {
                        if (cmd?.Condition is HitSoundTriggerCondition condition
                            &&condition.CheckCondition(hit_sound))
                        {
                            cmd.Trig(current_time);
                        }
                    }
                }
            }
        }

        public void Trig(GameState state, float current_time)
        {
            foreach (var obj in register_trigger_objects)
            {
                foreach (var register_triggers in obj.Triggers.Values)
                {
                    var cmd = PickVaildTrigger(register_triggers, current_time);

                    if (cmd.Condition is GameStateTriggerCondition condition
                        &&condition.CheckCondition(state))
                    {
                        cmd.Trig(current_time);
                    }
                }
            }
        }

        /// <summary>
        /// 当时间轴回滚的时候就得清除触发器的子命令，然后重置状态
        /// </summary>
        public void Reset()
        {
            foreach (var trigger in register_trigger_objects.SelectMany(l=>l.Triggers).Select(l=>l.Value).SelectMany(l=>l))
            {
                trigger.Reset();
            }
        }

        public static TriggerListener DefaultListener { get; private set; } = new TriggerListener();
    }
}
