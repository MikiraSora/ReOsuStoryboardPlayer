using ReOsuStoryboardPlayer.Core.Commands.Group.Trigger;
using ReOsuStoryboardPlayer.Core.Commands.Group.Trigger.TriggerCondition;
using System.IO;

namespace ReOsuStoryboardPlayer.Core.Serialization.DeserializationFactory
{
    public static class TriggerConditionDeserializationFactory
    {
        public static TriggerConditionBase Create(BinaryReader reader, StringCacheTable cache)
        {
            int i = 0;
            i.OnDeserialize(reader);
            TriggerConditionBase condition = null;

            switch (i)
            {
                case 0: //GameStateTriggerCondition
                    condition=new GameStateTriggerCondition();
                    break;

                case 1:
                    condition=new HitSoundTriggerCondition();
                    break;

                default:
                    break;
            }

            condition?.OnDeserialize(reader, cache);

            return condition;//todo
        }
    }
}