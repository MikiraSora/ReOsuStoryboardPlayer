namespace ReOsuStoryBoardPlayer.ProgramCommandParser
{
    public interface IParamParser
    {
        bool TryDivide(string args, out IParameters p);
    }
}
