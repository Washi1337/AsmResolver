using System.IO;
using BenchmarkDotNet.Attributes;
using static AsmResolver.Benchmarks.Properties.Resources;

namespace AsmResolver.Benchmarks
{
    public class PEFileReadWriteBenchmark
    {
        private static readonly byte[] HelloWorldApp = HelloWorld;
        private static readonly byte[] CrackMeApp = Test;
        private static readonly byte[] ManyMethods = Utilities.DecompressDeflate(HelloWorld_ManyMethods);

        private readonly MemoryStream _outputStream = new();

        [Benchmark]
        public void HelloWorld_Read()
        {
            var file = PE.File.PEFile.FromBytes(HelloWorldApp);
        }

        [Benchmark]
        public void HelloWorld_ReadWrite()
        {
            var file = PE.File.PEFile.FromBytes(HelloWorldApp);
            file.Write(_outputStream);
        }

        [Benchmark]
        public void CrackMe_Read()
        {
            var file = PE.File.PEFile.FromBytes(CrackMeApp);
        }

        [Benchmark]
        public void CrackMe_ReadWrite()
        {
            var file = PE.File.PEFile.FromBytes(CrackMeApp);
            file.Write(_outputStream);
        }

        [Benchmark]
        public void ManyMethods_Read()
        {
            var file = PE.File.PEFile.FromBytes(ManyMethods);
        }

        [Benchmark]
        public void ManyMethods_ReadWrite()
        {
            var file = PE.File.PEFile.FromBytes(ManyMethods);
            file.Write(_outputStream);
        }
    }
}
