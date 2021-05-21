using System.IO;
using System.IO.Compression;
using AsmResolver.DotNet;
using BenchmarkDotNet.Attributes;

namespace AsmResolver.Benchmarks
{
    [MemoryDiagnoser]
    public class LargeMethodBodiesBenchmark
    {
        private static readonly byte[] HelloWorldPumped;
        private readonly MemoryStream _outputStream = new();

        static LargeMethodBodiesBenchmark()
        {
            using var compressed = new MemoryStream(Properties.Resources.HelloWorld_Pumped);
            using var deflate = new DeflateStream(compressed, CompressionMode.Decompress);

            using var decompressed = new MemoryStream();
            deflate.CopyTo(decompressed);

            decompressed.Position = 0;
            HelloWorldPumped = decompressed.ToArray();
        }

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
