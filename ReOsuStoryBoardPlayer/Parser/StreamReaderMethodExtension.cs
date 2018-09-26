using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser
{
    public static class StreamReaderMethodExtension
    {
        /// <summary>
        /// 将流跳到指定的Section上
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static bool JumpToSection(this StreamReader reader, Section section)
        {
            reader.BaseStream.Seek(0, SeekOrigin.Begin);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                //[Events] or [32]
                if (line.FirstOrDefault()=='['&&line.LastOrDefault()==']')
                {
                    var section_name = line.Substring(1, line.Length - 2);

                    if (Enum.TryParse(section_name,out Section s)&& s == section)
                        return true;

                    if (int.TryParse(section_name, out var si)&&(Section)si == section)
                        return true;
                }
            }

            return false;
        }
    }
}
