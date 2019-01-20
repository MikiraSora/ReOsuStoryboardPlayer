using ReOsuStoryBoardPlayer.Commands.Group.Trigger.TriggerCondition;
using System;
using System.Collections.Generic;
using System.Linq;

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

        private static TriggerConditionBase Generate(string condition_expr)
        {
            if (condition_expr.StartsWith("HitSound"))
                return new HitSoundTriggerCondition(condition_expr);
            if (Enum.GetNames(typeof(GameState)).Any(x => condition_expr.StartsWith(x)))
                return new GameStateTriggerCondition(condition_expr);
            /* todo
            if (condition_expr=="HitObjectHit")
                return new HitSoundTriggerCondition("HitSound");
                */

            throw new FormatException($"\"{condition_expr}\" not a vaild trigger type value.");
        }

        #endregion Parse
    }
}