using ReOsuStoryBoardPlayer.Base;

namespace ReOsuStoryBoardPlayer.Commands
{
    public abstract class StateCommand : Command
    {
        public abstract void ApplyValue(StoryBoardObject @object, bool value);

        public override void Execute(StoryBoardObject @object, float time)
        {
            if (StartTime == EndTime || (StartTime < time && time < EndTime))
                ApplyValue(@object, true);
            else
                ApplyValue(@object, false);
        }
    }
}