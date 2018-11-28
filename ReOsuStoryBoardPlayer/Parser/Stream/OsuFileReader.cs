using ReOsuStoryBoardPlayer.Parser.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser.Stream
{
    public class OsuFileReader : ByteMemoryReader
    {
        public OsuFileReader(ReadOnlyMemory<byte> buffer) : base(buffer)
        {

        }

        public ReadOnlyMemory<byte> ReadSectionContent(Section section)
        {
            var pos = FindSectionPostion(section);
            if (pos < 0)
                return null;

            var backup_pos = Position;
            Position = pos;

            //skip current section head
            ReadLine();
            int start_pos = (int)Position;
            int end_pos = start_pos;

            while (!EndOfStream)
            {
                var line = ReadLine();

                if (line.Length > 2 && line.Span[0] == '[' && line.Span[line.Length - 1] == ']')
                {
                    //get other section head
                    break;
                }

                end_pos = (int)Position;
            }

            Position = backup_pos;
            return Buffer.Slice(start_pos, end_pos - start_pos);
        }

        public SectionReader GetSectionReader(Section section)
        {
            var pos = this.FindSectionPostion(section);
            if (pos < 0)
                return null;

            var backup_pos = Position;
            Position = pos;

            //skip current section head
            ReadLine();
            
            pos = Position;
            Position = backup_pos;

            return new SectionReader(Buffer.Slice((int)pos));
        }
        
        public long FindSectionPostion(Section section)
        {
            var backup_pos = Position;
            long result=-1;
            Position = 0;

            while (!EndOfStream)
            {
                result = Position;
                var line = ReadLine();

                //[Events] or [32]
                if (line.Length > 2 && line.Span[0] == '[' && line.Span[line.Length - 1] == ']')
                {
                    var section_name = line.Slice(1, line.Length - 2).GetContentString();

                    if (IsSection(section_name,section))
                    {
                        break;
                    }
                }
            }

            Position = backup_pos;
            return result;
        }

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
