using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Serialization;
using System;
using System.Collections.Generic;
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

        public override void OnSerialize(BinaryWriter stream, StringCacheTable _)
        {
            //read by TriggerConditionDeserializationFactory::Create()
            0.OnSerialize(stream);

            ((byte)listen_state).OnSerialize(stream);
        }

        public override void OnDeserialize(BinaryReader stream, StringCacheTable _)
        {
            listen_state=(GameState)stream.ReadByte();
        }

        public override bool Equals(TriggerConditionBase other)
        {
            return other is GameStateTriggerCondition game_cond && game_cond.listen_state==listen_state;
        }
    }
}