﻿using ReOsuStoryBoardPlayer.Parser.Collection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser.Rubbish
{
    //我他妈撸这个玩意干嘛?
    public class EnumParser<T>
    {
        private static EnumParser<T> _instance;

        public static EnumParser<T> Instance
        {
            get
            {
                if (_instance == null)
                {
                    if (!typeof(T).IsEnum)
                        throw new InvalidOperationException($"{typeof(T).Name} isn't enum");

                    _instance = new EnumParser<T>();
                }

                return _instance;
            }
            private set { }
        }

        CharPatternCollectionBase<EnumWrapper> map;

        readonly struct EnumWrapper:IComparable<EnumWrapper>
        {
            public readonly T val;
            public readonly int iv;

            public EnumWrapper(T val,int iv)
            {
                this.val = val;
                this.iv = iv;
            }

            public int CompareTo(EnumWrapper other) => val.ToString().CompareTo(other.val.ToString());
        }

        public EnumParser()
        {
            map = new CharPatternCollectionBase<EnumWrapper>(e => e.ToString(), default, new EnumWrapper[0]);

            foreach (T val in Enum.GetValues(typeof(T)))
            {
                int iv = CalculateEnumValue(val);
                EnumWrapper enumWrapper = new EnumWrapper(val,iv);

                map[iv.ToString()] = enumWrapper;
                map[val.ToString()] = enumWrapper;
            }
        }

        public static int CalculateEnumValue(T value)
        {
            return 0;
        }

        public T Parse(int value) => Parse(value.ToString());

        public T Parse(string value)
        {
            if (map.TryGetValue(value.ToString(), out var val))
                return val.val;
            throw new Exception($"Not found value {value} to {typeof(T).Name}");
        }
    }
}