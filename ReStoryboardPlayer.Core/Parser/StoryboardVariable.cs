using System;
using System.Collections.Generic;
using System.Linq;

namespace ReOsuStoryBoardPlayer.Core.Parser
{
    public struct StoryboardVariable : IComparable<StoryboardVariable>
    {
        public string Name;
        public string Value;

        public static readonly StoryboardVariable Empty = new StoryboardVariable(string.Empty, string.Empty);

        public StoryboardVariable(string name, string value)
        {
            if (value.Length != 0 && name.FirstOrDefault() != '$')
                throw new Exception("StoryboardVariable name first char must be '$'");

            Name = name ?? string.Empty;
            Value = value ?? string.Empty;
        }

        public int CompareTo(StoryboardVariable other)
        {
            return (Name ?? string.Empty).CompareTo(other.Name ?? string.Empty);
        }

        public static bool operator ==(StoryboardVariable x, StoryboardVariable y) => x.CompareTo(y) == 0;

        public static bool operator !=(StoryboardVariable x, StoryboardVariable y) => !(x == y);

        public override string ToString() => $"{Name} = {Value}";

        public override bool Equals(object obj)
        {
            if (!(obj is StoryboardVariable))
            {
                return false;
            }

            var variable = (StoryboardVariable)obj;
            return Name==variable.Name&&
                   Value==variable.Value;
        }

        public override int GetHashCode()
        {
            var hashCode = -244751520;
            hashCode=hashCode*-1521134295+EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode=hashCode*-1521134295+EqualityComparer<string>.Default.GetHashCode(Value);
            return hashCode;
        }
    }
}