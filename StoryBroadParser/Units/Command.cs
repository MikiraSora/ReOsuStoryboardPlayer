using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryBroadParser
{
    public class Command: IComparable<Command>
    {
        public Easing _easing;
        public Events _event;
        public int _startTime, _endTime;
        public List<String> _params;

        public int CompareTo(Command other)
        {
            /*
            if (other._event == this._event)
            {
                return this._startTime - other._startTime;
            }
            */
            return this._startTime - other._startTime;
        }
    }
}
