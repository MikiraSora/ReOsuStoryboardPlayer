using ReOsuStoryBoardPlayer.Parser.Collection;
using ReOsuStoryBoardPlayer.Parser.Extension;
using System;
using System.Collections.Generic;

namespace ReOsuStoryBoardPlayer.Parser.Reader
{
    public struct StoryboardPacket : IComparable<StoryboardPacket>
    {
        public List<string> CommandLines;

        public string ObjectLine;
        public long ObjectFileLine;

        public static readonly StoryboardPacket Empty = new StoryboardPacket() { ObjectFileLine = -2857 };

        public int CompareTo(StoryboardPacket other)
        {
            return Math.Sign(ObjectFileLine - other.ObjectFileLine);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is StoryboardPacket))
            {
                return false;
            }

            var packet = (StoryboardPacket)obj;
            return EqualityComparer<List<string>>.Default.Equals(CommandLines, packet.CommandLines)&&
                   ObjectLine==packet.ObjectLine&&
                   ObjectFileLine==packet.ObjectFileLine;
        }

        public override int GetHashCode()
        {
            var hashCode = -1274036625;
            hashCode=hashCode*-1521134295+EqualityComparer<List<string>>.Default.GetHashCode(CommandLines);
            hashCode=hashCode*-1521134295+EqualityComparer<string>.Default.GetHashCode(ObjectLine);
            hashCode=hashCode*-1521134295+ObjectFileLine.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(StoryboardPacket a, StoryboardPacket b) => a.CompareTo(b) == 0;

        public static bool operator !=(StoryboardPacket a, StoryboardPacket b) => !(a == b);
    }

    public class EventReader : IReader<StoryboardPacket>
    {
        public VariableCollection Variables { get; }

        private SectionReader reader;

        public enum LineType
        {
            Object,
            Command,
            Others
        }

        public EventReader(OsuFileReader reader, VariableCollection variables)
        {
            Variables = variables;
            this.reader = new SectionReader(Section.Events, reader);
        }

        private StoryboardPacket packet = StoryboardPacket.Empty;

        public IEnumerable<StoryboardPacket> EnumValues()
        {
            var FileLine = 0;

            foreach (var line in reader.EnumValues())
            {
                switch (CheckLineType(line))
                {
                    case LineType.Object:
                        //return current object
                        var t = packet;

                        packet = new StoryboardPacket();
                        packet.CommandLines = new List<string>();
                        packet.ObjectFileLine = FileLine - 1;
                        packet.ObjectLine = line;

                        if (t != StoryboardPacket.Empty)
                            yield return t;

                        break;

                    case LineType.Command:
                        packet.CommandLines.Add(line);
                        break;

                    //ignore comment/space only
                    case LineType.Others:
                    default:
                        break;
                }
            }

            if (packet != StoryboardPacket.Empty)
            {
                yield return packet;
                packet = StoryboardPacket.Empty;
            }
        }

        public LineType CheckLineType(string line)
        {
            if (!line.CheckLineValid())
                return LineType.Others;

            if (line[0] == '_' || line[0] == ' ')
                return LineType.Command;

            return LineType.Object;
        }

        public void ReturnPacket(ref StoryboardPacket packet)
        {
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

            return chars;
        }
    }
}