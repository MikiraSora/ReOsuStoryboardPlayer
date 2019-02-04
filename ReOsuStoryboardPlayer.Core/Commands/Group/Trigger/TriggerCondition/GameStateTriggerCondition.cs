using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Serialization;
using System;
using System.IO;

namespace ReOsuStoryboardPlayer.Core.Commands.Group.Trigger.TriggerCondition
{
    public class GameStateTriggerCondition : TriggerConditionBase
    {
        private GameState listen_state;

        public GameStateTriggerCondition()
        {

        }

        public GameStateTriggerCondition(string description)
        {
            listen_state=(GameState)Enum.Parse(typeof(GameState), description, true);
        }

        public bool CheckCondition(GameState state) => listen_state==state;

        public override void OnSerialize(BinaryWriter stream)
        {
            //read by TriggerConditionDeserializationFactory::Create()
            0.OnSerialize(stream);

            ((byte)listen_state).OnSerialize(stream);
        }

        public override void OnDeserialize(BinaryReader stream)
        {
            listen_state=(GameState)stream.ReadByte();
        }
    }
}