using ReOsuStoryboardPlayer.Core.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class CommandTimelineCompare : IComparer<Command>
    {
        public int Compare(Command x, Command y)
        {
            var z = x.StartTime-y.StartTime;

            if (z!=0)
                return z;

            return x.EndTime-y.EndTime;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            SortedList<Command, Command> sort = new SortedList<Command, Command>(new CommandTimelineCompare());


        }
    }
}
