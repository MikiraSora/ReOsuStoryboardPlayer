using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Commands;
using ReOsuStoryboardPlayer.Core.Commands.Group;
using ReOsuStoryboardPlayer.Core.Parser.CommandParser;
using ReOsuStoryboardPlayer.Core.Serialization;
using ReOsuStoryboardPlayer.Core.Serialization.FileInfo;
using ReOsuStoryboardPlayer.Core.Utils;
using ReOsuStoryboardPlayer.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            var text = new[]{
                " L,18952,65",
                " V,0,0,300,1.25,1.25",
                " P,0,75,225,H",
                " P,0,150,300,V"
            };

            var c = CommandParserIntance.Parse(text[0]).First() as LoopCommand;
            foreach (var item in text.Skip(1))
            {
                var v = CommandParserIntance.Parse(item);
                foreach (var vv in v)
                    c.AddSubCommand(vv);
            }

            c.UpdateSubCommand();

            var expand = c.SubCommandExpand();
        }
    }
}
