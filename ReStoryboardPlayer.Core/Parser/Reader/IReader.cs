using System.Collections.Generic;

namespace ReOsuStoryBoardPlayer.Core.Parser.Reader
{
    internal interface IReader<T>
    {
        IEnumerable<T> EnumValues();
    }
}