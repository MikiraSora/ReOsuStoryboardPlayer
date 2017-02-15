using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryBroadParser
{
    public class Sprite
    {
        public List<Command> _commands = new List<Command>();

        public Layer _layer;

        public Origin _origin;

        public string _imgPath;

        public int _x, _y;
    }
}
