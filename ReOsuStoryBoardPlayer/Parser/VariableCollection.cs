using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser
{
    public class VariableCollection : IDictionary<string, string>
    {
        class CharItem<T>
        {
            public CharItem(char c, T val)
            {
                Val = val;
            }

            public T Val { get; set; }
            public char Char { get; set; }

            public Dictionary<char, CharItem<T>> NextMap { get; set; } = new Dictionary<char, CharItem<T>>();

            public override string ToString() => $"{Char} : {Val}";
        }

        private CharItem<StoryboardVariable> root = new CharItem<StoryboardVariable>('$', StoryboardVariable.Empty);

        public ICollection<string> Keys => GetVariables(root).Select(p => p.Key).ToList();

        public ICollection<string> Values => GetVariables(root).Select(p => p.Value).ToList();

        public int Count => GetVariables(root).Count();

        public bool IsReadOnly => false;

        public string this[string key]
        {
            get
            {
                var val = Get(key);
                if (val == StoryboardVariable.Empty)
                    throw new KeyNotFoundException();
                return val.Value;
            }

            set
            {
                Add(new StoryboardVariable(key, value));
            }
        }

        public VariableCollection(IEnumerable<StoryboardVariable> variables = null)
        {
            if (variables != null)
                foreach (var variable in variables)
                    Add(variable);
        }

        public void Add(StoryboardVariable variable)
        {
            var cur_item = root;

            foreach (var ch in variable.Name.Skip(1))
            {
                if (!cur_item.NextMap.TryGetValue(ch, out var item))
                    cur_item.NextMap[ch] = new CharItem<StoryboardVariable>(ch, StoryboardVariable.Empty);

                cur_item = cur_item.NextMap[ch];
            }

            cur_item.Val = variable;
        }

        public bool ContainsKey(string key)
        {
            return Get(key) != StoryboardVariable.Empty;
        }

        public StoryboardVariable Get(string name)
        {
            var cur_item = root;

            foreach (var ch in name.Skip(1))
            {
                if (!cur_item.NextMap.TryGetValue(ch, out var item))
                    return StoryboardVariable.Empty;

                cur_item = cur_item.NextMap[ch];
            }

            return cur_item.Val;
        }

        public void Add(string key, string value)
        {
            Add(new StoryboardVariable()
            {
                Name = key,
                Value = value
            });
        }

        public bool Remove(string key)
        {
            var cur_item = root;

            foreach (var ch in key.Skip(1))
            {
                if (!cur_item.NextMap.TryGetValue(ch, out var item))
                    return false;

                cur_item = cur_item.NextMap[ch];
            }

            cur_item.Val = StoryboardVariable.Empty;
            return true;
        }

        public bool TryGetValue(string key, out string value)
        {
            var val = Get(key);
            value = val != StoryboardVariable.Empty ? val.Value : null;
            return val != StoryboardVariable.Empty;
        }

        public void Add(KeyValuePair<string, string> item)
        {
            Add(new StoryboardVariable(item.Key, item.Value));
        }

        public void Clear()
        {
            root.NextMap.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            var val = Get(item.Key);

            return val != StoryboardVariable.Empty && val.Value == item.Value;
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            return Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            foreach (var item in GetVariables(root))
                yield return item;
        }

        private IEnumerable<KeyValuePair<string, string>> GetVariables(CharItem<StoryboardVariable> item)
        {
            foreach (var i in item.NextMap.Values)
                foreach (var t in GetVariables(i))
                    yield return t;

            if (item.Val != StoryboardVariable.Empty)
                yield return new KeyValuePair<string, string>(item.Val.Name, item.Val.Value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region Match Variables

        public IEnumerable<StoryboardVariable> MatchReplacableVariables(string line)
        {
            int cur_pos = 0;
            var span = line.AsMemory();

            while (!(cur_pos < 0 || cur_pos >= line.Length - 1))
            {
                cur_pos = line.IndexOf('$', cur_pos);
                if (cur_pos < 0 || cur_pos >= line.Length - 1)
                    break;

                if (TryGetReplacableVariable(span.Slice(cur_pos+1), out var variable))
                    yield return variable;

                cur_pos++;
            }
        }

        private bool TryGetReplacableVariable(ReadOnlyMemory<char> line, out StoryboardVariable variable)
        {
            var cur_item = root;
            variable = StoryboardVariable.Empty;

            foreach (var ch in line.Span)
            {
                if (!cur_item.NextMap.TryGetValue(ch, out var item))
                    break;

                cur_item = cur_item.NextMap[ch];
                variable = cur_item.Val;
            }

            return variable!=StoryboardVariable.Empty;
        }

        #endregion
    }
}
