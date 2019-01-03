using ReOsuStoryBoardPlayer.Commands.Group.Trigger.TriggerCondition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Commands.Group.Trigger
{
    //implement from osu!
    public abstract class TriggerConditionBase
    {
        #region Parse

        private static Dictionary<string, TriggerConditionBase> cache_triggers = new Dictionary<string, TriggerConditionBase>();

        public static TriggerConditionBase Parse(string description)
        {
            if (!cache_triggers.TryGetValue(description, out var trigger_condition))
                cache_triggers[description]=Generate(description);

            return cache_triggers[description];
        }

        private static TriggerConditionBase Generate(string description)
        {
            if (description.StartsWith("HitSound"))
                return new HitSoundTriggerCondition(description);

            throw new FormatException($"\"{description}\" not a vaild trigger type value.");
        }

        #endregion
    }
}
