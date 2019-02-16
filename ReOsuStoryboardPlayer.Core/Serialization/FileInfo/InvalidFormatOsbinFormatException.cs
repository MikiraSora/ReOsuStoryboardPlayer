using System;

namespace ReOsuStoryboardPlayer.Core.Serialization.FileInfo
{
    public class InvalidFormatOsbinFormatException : Exception
    {
        public InvalidFormatOsbinFormatException() : base("This file isn't standard .osbin file format.")
        {
        }
    }
}