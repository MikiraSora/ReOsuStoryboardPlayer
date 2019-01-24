using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ReOsuStoryboardPlayer.Core.Parser.Collection
{
    /// <summary>
    /// 基于字符串连续字符匹配的字典
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CharPatternCollectionBase<T> : IDictionary<string, T> where T : IComparable<T>
    {
        protected class CharItem
        {
            private CharItem()
            {
            }

            public CharItem(char c, T val)
            {
                Val=val;
            }

            public T Val { get; set; }
            public char Char { get; set; }

            public Dictionary<char, CharItem> NextMap { get; set; } = new Dictionary<char, CharItem>();

            public override string ToString() => $"{Char} : {Val}";
        }

        private Func<T, string> PathProjectionFunc;
        private readonly T InvaildTValue;

        protected readonly CharItem root;

        public ICollection<string> Keys => GetVariables(root).Select(p => p.Key).ToList();

        public ICollection<T> Values => GetVariables(root).Select(p => p.Value).ToList();

        public int Count => GetVariables(root).Count();

        public bool IsReadOnly => false;

        public T this[string key]
        {
            get
            {
                var val = Get(key);
                if (ValueEquals(val, InvaildTValue))
                    throw new KeyNotFoundException();
                return val;
            }

            set
            {
                Add(key, value);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="PathProjectionFunc">获取元素的字符串值，将作为key</param>
        /// <param name="InvaildTValue">钦定无效值</param>
        /// <param name="variables">初始集合</param>
        public CharPatternCollectionBase(Func<T, string> PathProjectionFunc, T InvaildTValue, IEnumerable<T> variables)
        {
            this.PathProjectionFunc=PathProjectionFunc;
            this.InvaildTValue=InvaildTValue;
            root=new CharItem('\0', InvaildTValue);
            foreach (var variable in variables)
                Add(variable);
        }

        public bool ContainsKey(string key)
        {
            return !ValueEquals(Get(key), InvaildTValue);
        }

        private T Get(string name)
        {
            var cur_item = root;

            foreach (var ch in name)
            {
                if (!cur_item.NextMap.TryGetValue(ch, out var item))
                    return InvaildTValue;

                cur_item=cur_item.NextMap[ch];
            }

            return cur_item.Val;
        }

        public void Add(T value) => Add(PathProjectionFunc(value), value);

        public void Add(string key, T value)
        {
            var cur_item = root;

            foreach (var ch in key)
            {
                if (!cur_item.NextMap.TryGetValue(ch, out var item))
                    cur_item.NextMap[ch]=new CharItem(ch, default(T));

                cur_item=cur_item.NextMap[ch];
            }

            cur_item.Val=value;
        }

        public bool Remove(string key)
        {
            var cur_item = root;

            foreach (var ch in key)
            {
                if (!cur_item.NextMap.TryGetValue(ch, out var item))
                    return false;

                cur_item=cur_item.NextMap[ch];
            }

            cur_item.Val=InvaildTValue;
            return true;
        }

        public bool TryGetValue(string key, out T value)
        {
            var val = Get(key);
            value=!ValueEquals(val, InvaildTValue) ? val : InvaildTValue;
            return !ValueEquals(val, InvaildTValue);
        }

        public void Add(KeyValuePair<string, T> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            root.NextMap.Clear();
        }

        public bool Contains(KeyValuePair<string, T> item)
        {
            var val = Get(item.Key);

            return !ValueEquals(val, InvaildTValue)&&ValueEquals(val, item.Value);
        }

        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, T> item)
        {
            return Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            foreach (var item in GetVariables(root))
                yield return item;
        }

        private bool ValueEquals(T a, T b) => a.CompareTo(b)==0;

        private IEnumerable<KeyValuePair<string, T>> GetVariables(CharItem item)
        {
            foreach (var i in item.NextMap.Values)
                foreach (var t in GetVariables(i))
                    yield return t;

            if (!ValueEquals(item.Val, InvaildTValue))
                yield return new KeyValuePair<string, T>(PathProjectionFunc(item.Val), item.Val);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetVariables(root).GetEnumerator();
        }
    }
}