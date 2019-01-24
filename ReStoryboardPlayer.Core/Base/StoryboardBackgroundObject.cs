using ReOsuStoryBoardPlayer.Core.Commands;

namespace ReOsuStoryBoardPlayer.Core.Base
{
    public class StoryboardBackgroundObject : StoryBoardObject
    {
        public StoryboardBackgroundObject()
        {
            Z=int.MaxValue;

            AddCommand(new FadeCommand()
            {
                Easing= /*EasingConverter.GetEasingInterpolator(Easing.Linear)*/EasingTypes.None,
                StartTime=-2857,
                EndTime=-2857,
                StartValue=1,
                EndValue=1
            });

            AddCommand(new FadeCommand()
            {
                Easing= /*EasingConverter.GetEasingInterpolator(Easing.Linear)*/EasingTypes.None,
                StartTime=int.MaxValue-2857,
                EndTime=int.MaxValue-2857,
                StartValue=1,
                EndValue=1
            });

            /*todo
            AddCommand(new ScaleCommand()
            {
                StartTime = int.MinValue,
                EndTime = int.MinValue,
                StartValue = 1,
                EndValue = 1
            });
            */
        }
    }
}