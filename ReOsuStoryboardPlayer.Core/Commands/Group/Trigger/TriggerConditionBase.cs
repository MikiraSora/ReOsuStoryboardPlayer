using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Commands.Group.Trigger.TriggerCondition;
using ReOsuStoryboardPlayer.Core.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReOsuStoryboardPlayer.Core.Commands.Group.Trigger
{
    //implement from osu!
    public abstract class TriggerConditionBase:IStoryboardSerializable
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

        public abstract void OnDeserialize(BinaryReader stream);

        public abstract void OnSerialize(BinaryWriter stream);

        #endregion Parse
    }
}