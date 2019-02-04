using ReOsuStoryboardPlayer.Core.Commands.Group.Trigger;
using ReOsuStoryboardPlayer.Core.Commands.Group.Trigger.TriggerCondition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ReOsuStoryboardPlayer.Core.Serialization.DeserializationFactory
{
    public static class TriggerConditionDeserializationFactory
    {
        public static TriggerConditionBase Create(BinaryReader reader)
        {
            int i = 0;
            i.OnDeserialize(reader);
            TriggerConditionBase condition=null;

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

            condition?.OnDeserialize(reader);
            
            return condition;//todo
        }
    }
}
