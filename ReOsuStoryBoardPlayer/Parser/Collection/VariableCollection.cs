using ReOsuStoryBoardPlayer.Parser.Collection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser.Collection
{
    public class VariableCollection : CharPatternCollectionBase<StoryboardVariable>
    {
        public VariableCollection(IEnumerable<StoryboardVariable> variables=null) : base(v=>v.Name, StoryboardVariable.Empty, variables??new List<StoryboardVariable>())
        {

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

                if (TryGetReplacableVariable(span.Slice(cur_pos), out var variable))
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
