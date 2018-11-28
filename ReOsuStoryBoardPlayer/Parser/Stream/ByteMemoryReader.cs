using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser.Stream
{
    public class ByteMemoryReader:System.IO.Stream
    {
        public ReadOnlyMemory<byte> Buffer { get; }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => Buffer.Length;

        public long FileLine { get; private set; }

        public override long Position { get; set; } = 0;

        private readonly static byte[] ReturnChars = new byte[] { 10, 13 };

        public virtual bool EndOfStream => Position >= Length;

        public ByteMemoryReader(ReadOnlyMemory<byte> buffer)
        {
            Buffer = buffer;
        }

        public virtual ReadOnlyMemory<byte> ReadLine()
        {
            byte prev_c = default;
            int pick_len = 0;
            var start_pos = Position;

            while (Position<Length)
            {
                pick_len++;
                var c = Buffer.Span[(int)Position];
                Position++;

                if (ReturnChars.Contains(prev_c))
                {
                    if (!ReturnChars.Contains(c))
                    {
                        Position--;
                    }

                    pick_len-=2;

                    break;
                }

                prev_c = c;
            }

            FileLine++;
            return Buffer.Slice((int)start_pos, pick_len);
        }

        public override void Flush()
        {

        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            var base_pos = 0L;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    base_pos = 0;
                    break;
                case SeekOrigin.Current:
                    base_pos = Position;
                    break;
                case SeekOrigin.End:
                    base_pos = Length;
                    break;
                default:
                    break;
            }

            base_pos += offset;

            if (base_pos<=0&&base_pos>Length)
                throw new ArgumentOutOfRangeException();

            Position = base_pos;
            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
