using System;
using System.IO;

namespace ReOsuStoryboardPlayer.Core.Parser.Stream
{
    public class OsuFileReader : IDisposable
    {
        private StreamReader reader;

        public bool EndOfStream => reader.EndOfStream;

        public int FileLine = 0;

        public OsuFileReader(System.IO.Stream base_stream)
        {
            reader=new StreamReader(base_stream);
        }

        public OsuFileReader(string file_path)
        {
            reader=new StreamReader(file_path);
        }

        ~OsuFileReader()
        {
            Dispose();
        }

        public bool JumpSectionContent(Section section)
        {
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            FileLine=0;

            while (!reader.EndOfStream)
            {
                var line = ReadLine();

                if (line.Length>2&&line[0]=='['&&line[line.Length-1]==']'&&IsSection(line.Substring(1, line.Length-2), section))
                {
                    return true;
                }
            }

            return false;
        }

        public string ReadLine()
        {
            FileLine++;
            return reader.ReadLine();
        }

        private static bool IsSection(string name, Section section)
        {
            if (Enum.TryParse(name, out Section s)&&s==section)
                return true;

            if (int.TryParse(name, out var si)&&(Section)si==section)
                return true;

            return false;
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}