using System.Collections.Generic;

namespace ReOsuStoryBoardPlayer.Commands
{
    public interface IParameters
    {
        string ArgString { get; }
        Dictionary<string, string> Args { get; }
        List<string> FreeArgs { get; }
        List<string> Switches { get; }
        List<string> SimpleArgs { get; }
    }
}
