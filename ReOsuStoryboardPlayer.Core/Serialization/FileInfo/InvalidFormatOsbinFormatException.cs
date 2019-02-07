using System;
using System.Collections.Generic;
using System.Text;

namespace ReOsuStoryboardPlayer.Core.Serialization.FileInfo
{
    public class InvalidFormatOsbinFormatException:Exception
    {
        public InvalidFormatOsbinFormatException() : base("This file isn't standard .osbin file format.")
        {

        }
    }
}
