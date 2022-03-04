using System.IO;
using AsmResolver.DotNet;
using BenchmarkDotNet.Attributes;

namespace AsmResolver.Benchmarks
{
    [MemoryDiagnoser]
    public class LargeMethodBodiesBenchmark
    {
        private static readonly byte[] HelloWorldPumped = Utilities.DecompressDeflate(Properties.Resources.HelloWorld_Pumped);
        private readonly MemoryStream _outputStream = new();

        [Benchmark]
        public void HelloWorldPumped_ModuleDefinition_Read()
        {
            var file = ModuleDefinition.FromBytes(HelloWorldPumped);
        }

        [Benchmark]
        public void HelloWorldPumped_ModuleDefinition_ReadWrite()
        {
            var file = ModuleDefinition.FromBytes(HelloWorldPumped);
            file.Write(_outputStream);
        }
    }
}
