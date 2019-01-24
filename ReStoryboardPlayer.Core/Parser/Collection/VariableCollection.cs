using System;
using System.Collections.Generic;

namespace ReOsuStoryBoardPlayer.Core.Parser.Collection
{
    public class VariableCollection : CharPatternCollectionBase<StoryboardVariable>
    {
        public VariableCollection(IEnumerable<StoryboardVariable> variables = null) : base(v => v.Name, StoryboardVariable.Empty, variables ?? new List<StoryboardVariable>())
        {
        }

        #region Match Variables

        public IEnumerable<StoryboardVariable> MatchReplacableVariables(string line)
        {
            int cur_pos = 0;

            while (!(cur_pos < 0 || cur_pos >= line.Length - 1))
            {
                cur_pos = line.IndexOf('$', cur_pos);
                if (cur_pos < 0 || cur_pos >= line.Length - 1)
                    break;

                if (TryGetReplacableVariable(line.Substring(cur_pos), out var variable))
                    yield return variable;

                cur_pos++;
            }
        }

        private bool TryGetReplacableVariable(string line, out StoryboardVariable variable)
        {
            var cur_item = root;
            variable = StoryboardVariable.Empty;

            foreach (var ch in line)
            {
                if (!cur_item.NextMap.TryGetValue(ch, out var item))
                    break;

                cur_item = cur_item.NextMap[ch];
                variable = cur_item.Val;
            }

            return variable != StoryboardVariable.Empty;
        }

        #endregion Match Variables
    }
}