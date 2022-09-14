using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMGDiscordBot.Tests.Benchmarks
{
    [MemoryDiagnoser]
    public class MemoryBenchmarkerDemo
    {
        [Benchmark]
        public void SpanTest()
        {
            var enumerator = File.ReadLines(@"C:\Program Files (x86)\Steam\steamapps\common\NeosVR\Logs\DESKTOP-4GOD64O - 2022.1.28.1335 - 2022-09-08 19_56_03.log").GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (Utils.startsWithTimePrefix.IsMatch(enumerator.Current))
                {
                    ReadOnlySpan<char> current = enumerator.Current;
                    current = current.Slice(current.IndexOf(')') + 2);
                }
            }
        }
        [Benchmark]
        public void StringTest()
        {
            var enumerator = File.ReadLines(@"C:\Program Files (x86)\Steam\steamapps\common\NeosVR\Logs\DESKTOP-4GOD64O - 2022.1.28.1335 - 2022-09-08 19_56_03.log").GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (Utils.startsWithTimePrefix.IsMatch(enumerator.Current))
                {
                    string current = enumerator.Current;
                    current = current.Substring(current.IndexOf(')') + 2);
                }
            }
        }
    }
}
