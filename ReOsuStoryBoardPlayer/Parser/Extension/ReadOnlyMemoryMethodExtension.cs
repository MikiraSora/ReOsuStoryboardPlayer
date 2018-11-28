using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser.Extension
{
    public static class ReadOnlyMemoryMethodExtension
    {
        public static ReadOnlyMemory<char>[] Split(this ReadOnlyMemory<char> line, char split_ch, StringSplitOptions option = StringSplitOptions.None)
        {
            List<ReadOnlyMemory<char>> list = new List<ReadOnlyMemory<char>>();
            var span = line.Span;
            int position = 0;

            for (int i = 0; i < line.Length; i++)
            {
                char ch = span[i];

                if (split_ch == ch)
                {
                    if (position != i || option == StringSplitOptions.None)
                        list.Add(line.Slice(position, i - position));
                    position = i + 1;
                }
            }

            if (position < line.Length || (position == line.Length && option == StringSplitOptions.None))
                list.Add(line.Slice(position, line.Length - position));

            return list.ToArray();
        }

        public static ReadOnlyMemory<char> TrimStart(this ReadOnlyMemory<char> line)
        {
            int index = 0;

            foreach (var item in line.Span)
            {
                if (item != ' ')
                    break;
                index++;
            }

            return line.Slice(index);
        }

        public static ReadOnlyMemory<char> TrimEnd(this ReadOnlyMemory<char> line)
        {
            int index = line.Length;
            var span = line.Span;

            for (int i = line.Length-1; i >= 0; i--)
            {
                if (span[i] != ' ')
                    break;
                index--;
            }

            return line.Slice(0,index);
        }

        public static ReadOnlyMemory<char> Trim(this ReadOnlyMemory<char> line) => TrimEnd(TrimStart(line));

        public static unsafe double ToDouble(this ReadOnlyMemory<char> chars)
        {
            using (var owner = MemoryPool<byte>.Shared.Rent(chars.Length))
            {
                var buffer = owner.Memory.Span;
                var tmp = MemoryMarshal.AsBytes(chars.Span);

                for (int i = 0; i < chars.Length; i++)
                    buffer[i] = tmp[i * 2];

                if (!Utf8Parser.TryParse(buffer, out double val, out int consumed))
                    throw new FormatException();
                return val;
            } 
        }
    }
}
