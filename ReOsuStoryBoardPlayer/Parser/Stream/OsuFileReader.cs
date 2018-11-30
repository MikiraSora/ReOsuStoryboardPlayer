using ReOsuStoryBoardPlayer.Parser.Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser.Stream
{
    public class OsuFileReader
    {
        StreamReader reader;

        public OsuFileReader(string file_path)
        {
            reader = new StreamReader(file_path);
        }

        ~OsuFileReader()
        {
            reader.Dispose();
        }

        public bool JumpSectionContent(Section section)
        {
            reader.BaseStream.Seek(0,SeekOrigin.Begin);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                if (line.Length > 2 && line[0] == '[' && line[line.Length - 1] == ']' && IsSection(line.Substring(1, line.Length-2),section))
                {
                    return true;
                }
            }

            return false;
        }

        public string ReadLine() => reader.ReadLine();

        public bool EndOfStream => reader.EndOfStream;

        private static bool IsSection(string name,Section section)
        {
            if (Enum.TryParse(name, out Section s) && s == section)
                return true;

            if (int.TryParse(name, out var si) && (Section)si == section)
                return true;

            return false;
        }
    }
}
