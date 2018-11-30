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
    public static class BufferMethodExtension
    {
        public static ReadOnlyMemory<byte>[] Split(this ReadOnlyMemory<byte> line, byte[] split_ch, StringSplitOptions option = StringSplitOptions.None)
        {
            List<ReadOnlyMemory<byte>> list = new List<ReadOnlyMemory<byte>>();
            var span = line.Span;
            int position = 0;

            for (int i = 0; i < line.Length; i++)
            {
                int capture_count = 0;

                while (i + capture_count < line.Length && capture_count < split_ch.Length && split_ch[capture_count] == span[i + capture_count])
                    capture_count++;

                if (capture_count == split_ch.Length)
                {
                    if (position != i || option == StringSplitOptions.None)
                        list.Add(line.Slice(position, i - position));
                    position = i + capture_count;
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

        public static ReadOnlyMemory<byte> TrimStart(this ReadOnlyMemory<byte> line,params byte[] filter)
        {
            int index = 0;

            foreach (var item in line.Span)
            {
                if (!filter.Contains(item))
                    break;
                index++;
            }

            return line.Slice(index);
        }

        public static ReadOnlyMemory<char> TrimEnd(this ReadOnlyMemory<char> line)
        {
            int index = line.Length;
            var span = line.Span;

            for (int i = line.Length - 1; i >= 0; i--)
            {
                if (span[i] != ' ')
                    break;
                index--;
            }

            return line.Slice(0, index);
        }

        public static ReadOnlyMemory<char> Trim(this ReadOnlyMemory<char> line) => TrimEnd(TrimStart(line));

        public static double ToDouble(this string chars) => double.Parse(chars);

        public static float ToSigle(this string chars) => float.Parse(chars);

        public static int ToInt(this string chars) => int.Parse(chars);

        public static bool StartsWith(this ReadOnlyMemory<byte> buffer,byte[] content)
        {
            if (buffer.Length < content.Length)
                return false;

            for (int i = 0; i < content.Length; i++)
                if (buffer.Span[i]!=content[i])
                    return false;

            return true;
        }

        public static unsafe IMemoryOwner<char> GetTempReadonlyCharSpan(this ReadOnlyMemory<byte> buffer, out ReadOnlySpan<char> chars)
        {
            var src_ptr = (byte*)buffer.Pin().Pointer;

            var dist_len = Encoding.UTF8.GetCharCount(src_ptr, buffer.Length);
            var ctemp = MemoryPool<char>.Shared.Rent(dist_len);

            var dist_ptr = (char*)ctemp.Memory.Pin().Pointer;

            var encode_len = Encoding.UTF8.GetChars(src_ptr, buffer.Length, dist_ptr, dist_len);

            chars = new ReadOnlySpan<char>(dist_ptr, encode_len);

            return ctemp;
        }

        private const byte BYTE_BLANK = 47; //"/"
        private const byte BYTE_UNDERLINE = 95; //"_"
        private const byte BYTE_BACKSPACE = 0x20; //" "

        public static bool CheckLineValid(this string buffer)
        {
            if (buffer.Length == 0 || buffer.Length >= 2 && buffer[0] == '/' && buffer[1] == '/')
                return false;

            return !string.IsNullOrWhiteSpace(buffer);
        }

        public static bool IsEmptyOrWhiteSpace(this ReadOnlyMemory<byte> buffer)
        {
            if (buffer.IsEmpty)
                return true;

            foreach (var x in buffer.Span)
                if (x != BYTE_BACKSPACE)
                    return false;

            return true;
        }

        public static string GetContentString(this ReadOnlyMemory<byte> buffer) => Encoding.UTF8.GetString(buffer.ToArray());
    }
}
