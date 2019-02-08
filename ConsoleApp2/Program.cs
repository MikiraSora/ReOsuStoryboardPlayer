using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Commands;
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
            Write();

            Read();

            //Test();
        }

        private static void Test()
        {
            var text = new[]{
                " F,0,6109,6435,0,1",
                " M,0,6109,11652,425,328,440,328",
                " F,0,6435,11326,1",
                " F,0,11326,11652,1,0",
                " S,0,22739,,1"
            };

            var commands = text.Select(l => CommandParserIntance.Parse(l.Split(','))).SelectMany(l => l);

            StoryboardObject obj = new StoryboardObject();
            obj.ImageFilePath=@"SB\2.png";
            obj.AddCommandRange(commands);
            obj.CalculateAndApplyBaseFrameTime();

            var list = new List<StoryboardObject>();
            list.Add(obj);

            MemoryStream stream = new MemoryStream();

            StoryboardBinaryFormatter.Serialize(0, list, stream);

            stream.Position=0;

            var new_list=StoryboardBinaryFormatter.Deserialize(stream).ToList();
            }

        private static void Read()
        {
            var stream = File.OpenRead("test.osbin");

            var objs=StoryboardBinaryFormatter.UnzipDeserialize(stream).ToList();
        }

        private static void Write()
        {
            var objects = StoryboardParserHelper
                   .GetStoryboardObjects(@"G:\OsuStoryBroadPlayer\ReOsuStoryboardPlayer.Core.UnitTest\TestData\Hatsuki Yura - Fuuga (Lan wings).osb");

            File.Delete("test.osbin");
            var stream = File.OpenWrite("test.osbin");

            StoryboardBinaryFormatter.ZipSerialize(0,objects, stream);
            stream.Dispose();
        }
    }
}
