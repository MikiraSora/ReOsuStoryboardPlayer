using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser.Stream
{
    public class EventReader : SectionReader
    {
        public VariableCollection Variables { get; }

        private ArrayPool<char> string_pool=ArrayPool<char>.Shared;

        public struct StoryboardPacket:IComparable<StoryboardPacket>
        {
            public List<ReadOnlyMemory<char>> CommandLines;

            public ReadOnlyMemory<char> ObjectLine;
            public long ObjectFileLine;

            public static readonly StoryboardPacket Empty = new StoryboardPacket() { ObjectFileLine = -2857 };

            public int CompareTo(StoryboardPacket other)
            {
                return Math.Sign(ObjectFileLine - other.ObjectFileLine);
            }

            public static bool operator ==(StoryboardPacket a, StoryboardPacket b) => a.CompareTo(b) == 0;
            public static bool operator !=(StoryboardPacket a, StoryboardPacket b) => !(a==b);
        }

        public enum LineType
        {
            Object,
            Command,
            Others
        }

        public EventReader(ReadOnlyMemory<char> buffer,VariableCollection variables) : base(buffer)
        {
            Variables = variables;
        }
        
        StoryboardPacket packet = StoryboardPacket.Empty;

        public IEnumerable<StoryboardPacket> GetStoryboardPackets()
        {
            while (true)
            {
                if (EndOfStream)
                    yield break;
                var packet = GetStoryboardPacket();
                if (packet != StoryboardPacket.Empty)
                    yield return packet;
            }
        }

        public StoryboardPacket GetStoryboardPacket()
        {
            lock (this)
            {
                while (true)
                {
                    if (EndOfStream)
                    {
                        var t = packet;
                        packet = StoryboardPacket.Empty;//set empty
                        return t;
                    }

                    var line_mem = ReadLine();

                    switch (CheckLineType(line_mem.Span))
                    {
                        case LineType.Object:
                            //return current object
                            var t = packet;

                            packet = new StoryboardPacket();
                            packet.CommandLines = new List<ReadOnlyMemory<char>>();
                            packet.ObjectFileLine = FileLine;
                            packet.ObjectLine = line_mem;

                            if (t != StoryboardPacket.Empty)
                                return t;

                            break;
                        case LineType.Command:
                            packet.CommandLines.Add(line_mem);
                            break;

                        //ignore comment/space only
                        case LineType.Others:
                        default:
                            break;
                    }
                }
            }
        }

        public LineType CheckLineType(ReadOnlySpan<char> line)
        {
            if (line.IsWhiteSpace()||line.ToString().StartsWith("//"))
                return LineType.Others;

            if (line[0] == '_' || line[0] == ' ')
                return LineType.Command;

            return LineType.Object;
        }

        public void ReturnPacket(ref StoryboardPacket packet)
        {
            string_pool.Return(packet.ObjectLine.ToArray());

            foreach (var str in packet.CommandLines)
                string_pool.Return(str.ToArray());

            //force set null and wait for GC
            packet = StoryboardPacket.Empty;
        }

        public ReadOnlyMemory<char> LineProcessVariable(ReadOnlyMemory<char> line)
        {
            //get replacable variables
            var var_list = Variables.MatchReplacableVariables(line.ToString())/*.ToDictionary(p=>p.Name)*/;

            var result = line.ToString();

            foreach (var var in var_list)
                result = result.Replace(var.Name, var.Value);

            var chars = result.ToCharArray();

            var buffer = string_pool.Rent(chars.Length);
            chars.CopyTo(buffer, 0);

            return buffer;
        }
    }
}
