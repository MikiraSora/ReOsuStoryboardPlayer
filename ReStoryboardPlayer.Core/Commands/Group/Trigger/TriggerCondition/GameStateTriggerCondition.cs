using ReOsuStoryBoardPlayer.Core.Base;
using System;

namespace ReOsuStoryBoardPlayer.Core.Commands.Group.Trigger.TriggerCondition
{
    public class GameStateTriggerCondition : TriggerConditionBase
    {
        private readonly GameState listen_state;

        public GameStateTriggerCondition(string description)
        {
            listen_state=(GameState)Enum.Parse(typeof(GameState), description, true);
        }

        public bool CheckCondition(GameState state) => listen_state==state;
    }
}