using System.IO;
using AsmResolver.DotNet;
using AsmResolver.PE;
using AsmResolver.PE.DotNet.Builder;
using BenchmarkDotNet.Attributes;

namespace AsmResolver.Benchmarks
{
    [MemoryDiagnoser]
    public class PEReadWriteBenchmark
    {
        private static readonly byte[] HelloWorldApp = Properties.Resources.HelloWorld;
        private static readonly byte[] CrackMeApp = Properties.Resources.Test;
        private readonly MemoryStream _outputStream = new();

        [Benchmark]
        public void HelloWorld_PEFile_Read()
        {
            var file = PE.File.PEFile.FromBytes(HelloWorldApp);
        }

        [Benchmark]
        public void HelloWorld_PEFile_ReadWrite()
        {
            var file = PE.File.PEFile.FromBytes(HelloWorldApp);
            file.Write(_outputStream);
        }

        [Benchmark]
        public void HelloWorld_PEImage_Read()
        {
            var file = PEImage.FromBytes(HelloWorldApp);
        }

        [Benchmark]
        public void HelloWorld_PEImage_ReadWrite()
        {
            var image = PEImage.FromBytes(HelloWorldApp);
            new ManagedPEFileBuilder().CreateFile(image).Write(_outputStream);
        }

        [Benchmark]
        public void HelloWorld_ModuleDefinition_Read()
        {
            var file = ModuleDefinition.FromBytes(HelloWorldApp);
        }

        [Benchmark]
        public void HelloWorld_ModuleDefinition_ReadWrite()
        {
            var file = ModuleDefinition.FromBytes(HelloWorldApp);
            file.Write(_outputStream);
        }

        [Benchmark]
        public void CrackMe_PEFile_Read()
        {
            var file = PE.File.PEFile.FromBytes(CrackMeApp);
        }

        [Benchmark]
        public void CrackMe_PEFile_ReadWrite()
        {
            var file = PE.File.PEFile.FromBytes(CrackMeApp);
            file.Write(_outputStream);
        }

        [Benchmark]
        public void CrackMe_PEImage_Read()
        {
            var file = PEImage.FromBytes(HelloWorldApp);
        }

        [Benchmark]
        public void CrackMe_PEImage_ReadWrite()
        {
            var image = PEImage.FromBytes(CrackMeApp);
            new ManagedPEFileBuilder().CreateFile(image).Write(_outputStream);
        }

        [Benchmark]
        public void CrackMe_ModuleDefinition_Read()
        {
            var file = ModuleDefinition.FromBytes(CrackMeApp);
        }

        [Benchmark]
        public void CrackMe_ModuleDefinition_ReadWrite()
        {
            var file = ModuleDefinition.FromBytes(CrackMeApp);
            file.Write(_outputStream);
        }
    }
}
