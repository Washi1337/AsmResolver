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
            HelloWorldPumped = DecompressDeflate(Properties.Resources.HelloWorld_Pumped);
        }

        private static byte[] DecompressDeflate(byte[] compressedData)
        {
            using var decompressed = new MemoryStream();
            using (var compressed = new MemoryStream(compressedData))
            {
                using var deflate = new DeflateStream(compressed, CompressionMode.Decompress);
                deflate.CopyTo(decompressed);
            }

            return decompressed.ToArray();
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
