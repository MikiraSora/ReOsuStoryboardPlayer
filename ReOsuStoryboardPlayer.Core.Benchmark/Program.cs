using BenchmarkDotNet.Running;
using System;

namespace ReOsuStoryboardPlayer.Core.Benchmark
{
    public class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<ParserBenchmark>();
            //var o = new ParserBenchmark();
            //o.Init();
            //o.ParseSimple();
        }
    }   
}
