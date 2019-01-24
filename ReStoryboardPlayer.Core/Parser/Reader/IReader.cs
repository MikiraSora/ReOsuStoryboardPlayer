using System.Collections.Generic;

namespace ReOsuStoryboardPlayer.Core.Parser.Reader
{
    internal interface IReader<T>
    {
        IEnumerable<T> EnumValues();
    }
}