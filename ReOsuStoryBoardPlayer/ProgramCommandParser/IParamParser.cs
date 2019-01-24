namespace ReOsuStoryboardPlayer.ProgramCommandParser
{
    public interface IParamParser
    {
        bool TryDivide(string args, out IParameters p);
    }
}