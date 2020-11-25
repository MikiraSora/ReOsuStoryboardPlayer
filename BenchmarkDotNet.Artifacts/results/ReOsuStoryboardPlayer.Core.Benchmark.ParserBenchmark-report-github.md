``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18363.1139 (1909/November2018Update/19H2)
Intel Core i3-4170 CPU 3.70GHz (Haswell), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=3.1.402
  [Host] : .NET Core 2.1.22 (CoreCLR 4.6.29220.03, CoreFX 4.6.29220.01), X64 RyuJIT

Job=.NET 4.7.2  Runtime=.NET 4.7.2  

```
|      Method | Mean | Error |
|------------ |-----:|------:|
| ParseNormal |   NA |    NA |
|  ParseLarge |   NA |    NA |
| ParseSimple |   NA |    NA |

Benchmarks with issues:
  ParserBenchmark.ParseNormal: .NET 4.7.2(Runtime=.NET 4.7.2)
  ParserBenchmark.ParseLarge: .NET 4.7.2(Runtime=.NET 4.7.2)
  ParserBenchmark.ParseSimple: .NET 4.7.2(Runtime=.NET 4.7.2)
