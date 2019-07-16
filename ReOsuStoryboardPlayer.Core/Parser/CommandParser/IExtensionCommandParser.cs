using System;
using System.Collections.Generic;
using System.Text;

namespace ReOsuStoryboardPlayer.Core.Parser.CommandParser
{
    public interface IExtensionCommandParser: ICommandParser
    {
        string[] SupportPrefix { get; }
    }
}
