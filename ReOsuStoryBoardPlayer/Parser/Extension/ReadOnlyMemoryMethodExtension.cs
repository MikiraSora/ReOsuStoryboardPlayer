using System;
using System.Collections.Generic;
using System.Linq;
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

    }
}
