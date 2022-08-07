using System.IO;
using AsmResolver.DotNet.Bundles;
using BenchmarkDotNet.Attributes;

namespace AsmResolver.Benchmarks
{
    [MemoryDiagnoser]
    public class DotNetBundleBenchmark
    {
        private static readonly byte[] HelloWorldSingleFileV6 = Properties.Resources.HelloWorld_SingleFile_V6;
        private readonly MemoryStream _outputStream = new();

        [Benchmark]
        public void ReadBundleManifestV6()
        {
            _ = BundleManifest.FromBytes(HelloWorldSingleFileV6);
        }
    }
}
