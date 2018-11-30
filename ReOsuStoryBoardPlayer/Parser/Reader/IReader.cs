using System.Collections.Generic;

namespace ReOsuStoryBoardPlayer.Parser.Reader
{
    internal interface IReader<T>
    {
        IEnumerable<T> EnumValues();
    }
}