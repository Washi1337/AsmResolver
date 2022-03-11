using System.IO;
using AsmResolver.PE;
using AsmResolver.PE.DotNet.Builder;
using BenchmarkDotNet.Attributes;
using static AsmResolver.Benchmarks.Properties.Resources;

namespace AsmResolver.Benchmarks
{
    [MemoryDiagnoser]
    public class PEImageReadWriteBenchmark
    {
        private static readonly byte[] HelloWorldApp = HelloWorld;
        private static readonly byte[] CrackMeApp = Test;
        private static readonly byte[] ManyMethods = Utilities.DecompressDeflate(HelloWorld_ManyMethods);

        private readonly MemoryStream _outputStream = new();

        [Benchmark]
        public void HelloWorld_Read()
        {
            var file = PEImage.FromBytes(HelloWorldApp);
        }

        [Benchmark]
        public void HelloWorld_ReadWrite()
        {
            var image = PEImage.FromBytes(HelloWorldApp);
            new ManagedPEFileBuilder().CreateFile(image).Write(_outputStream);
        }

        [Benchmark]
        public void CrackMe_Read()
        {
            var file = PEImage.FromBytes(HelloWorldApp);
        }

        [Benchmark]
        public void CrackMe_ReadWrite()
        {
            var image = PEImage.FromBytes(CrackMeApp);
            new ManagedPEFileBuilder().CreateFile(image).Write(_outputStream);
        }

        [Benchmark]
        public void ManyMethods_Read()
        {
            var file = PEImage.FromBytes(ManyMethods);
        }

        [Benchmark]
        public void ManyMethods_ReadWrite()
        {
            var image = PEImage.FromBytes(ManyMethods);
            new ManagedPEFileBuilder().CreateFile(image).Write(_outputStream);
        }
    }
}
