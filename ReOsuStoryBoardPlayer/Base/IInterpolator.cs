namespace ReOsuStoryBoardPlayer
{
    public interface IInterpolator
    {
        float start { get; set; }
        float end { get; set; }

        float calculate(float value);

        float calculate(float current_value, float start, float end);

        IInterpolator reverse();
    }
}