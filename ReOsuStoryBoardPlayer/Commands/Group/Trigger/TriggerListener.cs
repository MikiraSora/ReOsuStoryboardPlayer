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
    /// <summary>
    /// 用于管控SB里面所有Trigger,通过此类(默认Default)可以直接触发已经绑定的TriggerCommand.
    /// </summary>
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

        private IEnumerable<TriggerCommand> PickVaildTriggers(int group_id,IEnumerable<TriggerCommand> commands, float current_time)
        {
            /*
             0 Group里面的Trigger相互不冲突，除此之外的Group组里,都会只能执行一个Trigger(后者优先).
             不过在旧屙屎里面>0里面group_id会对应各个主要TriggerCondition类型，而且还是取负输出到.osb/.osu , 也取负读取，一脸懵逼.jpg
             */
            return group_id==0 ? 
                           commands.Where(x => x.CheckTimeVaild(current_time))
                           : commands.Reverse().Where(x => x.CheckTimeVaild(current_time)).Take(1);
        }

        public void Trig(HitSoundInfo hit_sound, float current_time)
        {
            foreach (var obj in register_trigger_objects.Where(o => o.FrameStartTime<=current_time&&current_time<=o.FrameEndTime))
            {
                foreach (var pair in obj.Triggers)
                {
                    var commands=PickVaildTriggers(pair.Key,pair.Value,current_time).ToList();
                    
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


        /// <summary>
        /// For AutoTrigger. optimze/trim HitSounds.
        /// </summary>
        /// <param name="hit_sound"></param>
        /// <returns></returns>
        internal bool CheckTrig(HitSoundInfo hit_sound)
        {
            if (hit_sound.Time==50507)
            {

            }

            foreach (var obj in register_trigger_objects.Where(o => o.FrameStartTime<=hit_sound.Time&&hit_sound.Time<=o.FrameEndTime))
            {
                foreach (var pair in obj.Triggers)
                {
                    var commands = PickVaildTriggers(pair.Key, pair.Value, (float)hit_sound.Time).ToList();

                    foreach (var cmd in commands)
                    {
                        if (cmd?.Condition is HitSoundTriggerCondition condition
                            &&condition.CheckCondition(hit_sound))
                            return true;
                    }
                }
            }

            return false;
        }

        public static TriggerListener DefaultListener { get; private set; } = new TriggerListener();
    }
}
