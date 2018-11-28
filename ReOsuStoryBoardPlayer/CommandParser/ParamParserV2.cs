using System;
using System.Collections.Generic;
using System.Linq;

namespace ReOsuStoryBoardPlayer.CommandParser
{
    public class ParamParserV2 : IParamParser
    {
        private readonly string[] _quotesStr;
        private readonly string _cmdFlagStr;
        public char[] Quotes { get; }
        public char CmdFlag { get; }

        public ParamParserV2(char cmdFlag, params char[] quotes)
        {
            _quotesStr = quotes.Select(k => k.ToString()).ToArray();
            _cmdFlagStr = cmdFlag.ToString();
            Quotes = quotes;
            CmdFlag = cmdFlag;
        }

        public bool TryDivide(string args, out IParameters p)
        {
            Log.User(args);
            p = new Parameters();
            string argStr = args.Trim();
            p.SimpleArgs.AddRange(argStr.Split(' '));
            if (argStr == "")
            {
                p = null;
                return false;
            }

            var splitedParam = new List<string>();
            try
            {
                splitedParam.AddRange(argStr.Split(' '));
                foreach (var item in splitedParam)
                {
                    if (Quotes.Any(q => ContainsChar(q, item)))
                    {
                        throw new ArgumentException();
                    }
                }

                bool combined = true;
                foreach (var item in _quotesStr)
                {
                    for (int i = 0; i < splitedParam.Count - 1; i++)
                    {
                        string cur = splitedParam[i], next = splitedParam[i + 1];

                        if (cur.StartsWith(item) && !cur.EndsWith(item))
                        {
                            combined = false;
                            splitedParam[i] = cur + " " + next;
                            splitedParam.Remove(next);
                            if (splitedParam[i].EndsWith(item))
                                combined = true;
                            i--;
                        }
                    }
                    if (!combined) throw new ArgumentException("Expect '" + item + "'.");
                }

                string tmpKey = null;
                bool isLastKeyOrValue = false;

                splitedParam.Add(_cmdFlagStr);
                foreach (var item in splitedParam)
                {
                    string tmpValue = null;
                    if (item.StartsWith(_cmdFlagStr))
                    {
                        if (tmpKey != null)
                        {
                            p.Switches.Add(tmpKey);
                        }

                        tmpKey = item.Remove(0, 1);
                        isLastKeyOrValue = true;
                    }
                    else
                    {
                        foreach (var q in Quotes)
                        {
                            tmpValue = tmpValue == null ? item.Trim(q) : tmpValue.Trim(q);
                        }
                        if (!isLastKeyOrValue)
                        {
                            p.FreeArgs.Add(tmpValue);
                            //throw new ArgumentException("Expect key.");
                        }
                        else
                        {
                            p.Args.Add(tmpKey, tmpValue);
                            tmpKey = null;
                            isLastKeyOrValue = false;
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                p = null;
                return false;
            }
            return true;
        }

        private bool ContainsChar(char ch, string str)
        {
            char[] cs = str.ToCharArray();
            for (int i = 1; i < cs.Length - 1; i++)
            {
                if (cs[i] == ch)
                    return true;
            }

            return false;
        }
    }
}

