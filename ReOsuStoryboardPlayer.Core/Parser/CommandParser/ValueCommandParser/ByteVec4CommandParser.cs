﻿using ReOsuStoryboardPlayer.Core.Commands;
using ReOsuStoryboardPlayer.Core.PrimitiveValue;
using ReOsuStoryboardPlayer.Core.Utils;
using System.Collections.Generic;
using System.Linq;

namespace ReOsuStoryboardPlayer.Core.Parser.CommandParser.ValueCommandParser
{
    public class ByteVec4CommandParser<CMD> : IValueCommandParser<ByteVec4, CMD> where CMD : ValueCommand<ByteVec4>, new()
    {
        public override int ParamCount => 3;

        public override ByteVec4 ConvertValue(IEnumerable<string> p) => new ByteVec4((byte)p.ElementAt(0).ToInt(), (byte)p.ElementAt(1).ToInt(), (byte)p.ElementAt(2).ToInt(), 0);
    }
}