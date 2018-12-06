namespace ReOsuStoryBoardPlayer.Commands
{
    public interface IParamParser
    {
        bool TryDivide(string args, out IParameters p);
    }
}
