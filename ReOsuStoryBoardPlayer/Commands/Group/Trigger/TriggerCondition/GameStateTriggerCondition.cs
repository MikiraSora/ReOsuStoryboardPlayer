using System;

namespace ReOsuStoryBoardPlayer.Commands.Group.Trigger.TriggerCondition
{
    public class GameStateTriggerCondition : TriggerConditionBase
    {
        private GameState listen_state;

        public GameStateTriggerCondition(string description)
        {
            listen_state=(GameState)Enum.Parse(typeof(GameState), description, true);
        }

        public bool CheckCondition(GameState state) => listen_state==state;
    }
}