using ReOsuStoryboardPlayer.Core.Base;

namespace ReOsuStoryboardPlayer.Core.Commands
{
    public abstract class StateCommand : Command
    {
        public abstract void ApplyValue(StoryboardObject @object, bool value);

        public override void Execute(StoryboardObject @object, float time)
        {
            if (StartTime==EndTime||(StartTime<time&&time<EndTime))
                ApplyValue(@object, true);
            else
                ApplyValue(@object, false);
        }
    }
}