using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser.Extension
{
    public static class ParserExtension
    {
        public static bool CheckLineValid(this string buffer)
        {
            if (buffer.Length==0||buffer.Length>=2&&buffer[0]=='/'&&buffer[1]=='/')
                return false;

            return !string.IsNullOrWhiteSpace(buffer);
        }
    }
}
