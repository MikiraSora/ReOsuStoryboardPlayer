using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Parser.Collection;
using ReOsuStoryboardPlayer.Core.Parser.Reader;
using ReOsuStoryboardPlayer.Core.Parser.Stream;

namespace ReOsuStoryboardPlayer.Core.Benchmark
{
    [SimpleJob(RuntimeMoniker.Net472)]
    public class ParserBenchmark
    {
        public static byte[][] fileStreams;

        [GlobalSetup]
        public void Init()
        {
            fileStreams = new[]{
                File.ReadAllBytes(Path.Combine("OsbFiles", "Camellia vs Akira Complex - Reality Distortion (rrtyui).osb")),
                File.ReadAllBytes(Path.Combine("OsbFiles", "fripSide - Hesitation Snow (Kawayi Rika) [Normal].osu")),
                File.ReadAllBytes(Path.Combine("OsbFiles", "Hatsuki Yura - Fuuga (Lan wings).osb")),
                File.ReadAllBytes(Path.Combine("OsbFiles", "IOSYS feat. 3L - Miracle-Hinacle (_lolipop).osb")),
                File.ReadAllBytes(Path.Combine("OsbFiles", "NOMA - LOUDER MACHINE (Skystar).osb")),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<StoryboardObject> GetObjects(byte[] data)
        {
            var stream = new MemoryStream(data);

            var reader = new OsuFileReader(stream);

            var collection = new VariableCollection(new VariableReader(reader).EnumValues());

            var er = new EventReader(reader, collection);

            var StoryboardReader = new StoryboardReader(er);

            var list = StoryboardReader.EnumValues().ToList();
            list.RemoveAll(c => c == null);

            foreach (var obj in list)
                obj.CalculateAndApplyBaseFrameTime();

            stream.Dispose();

            return list;
        }

        [Benchmark]
        public void ParseNormal()
        {
            GetObjects(fileStreams[0]).Count();
        }

        [Benchmark]
        public void ParseLarge()
        {
            GetObjects(fileStreams[3]).Count();
        }

        [Benchmark]
        public void ParseSimple()
        {
            GetObjects(fileStreams[1]).Count();
        }
    }
}
