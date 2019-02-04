using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Serialization;
using ReOsuStoryboardPlayer.Core.Utils;
using ReOsuStoryboardPlayer.Parser;
using System;
using System.Collections.Generic;
using System.IO;
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
            Write();

            //Read();
        }

        private static void Read()
        {
            var stream = File.OpenRead("test.osbin");

            var objs=StoryboardSerializationHelper.Deserialize(stream).ToList();
        }

        private static void Write()
        {
            var objects = StoryboardParserHelper
                   .GetStoryboardObjects(@"G:\SBTest\94790 Hatsuki Yura - Fuuga\Hatsuki Yura - Fuuga (Lan wings).osb");

            var stream = File.OpenWrite("test.osbin");

            StoryboardSerializationHelper.Serialize(objects, stream);
        }
    }
}
