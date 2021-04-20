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
        private readonly byte[] _helloWorldApp = Properties.Resources.HelloWorld;
        private readonly byte[] _crackMeApp = Properties.Resources.Test;
        private readonly MemoryStream _outputStream = new();

        [Benchmark]
        public void HelloWorld_PEFile_Read()
        {
            var file = PE.File.PEFile.FromBytes(_helloWorldApp);
        }

        [Benchmark]
        public void HelloWorld_PEFile_ReadWrite()
        {
            var file = PE.File.PEFile.FromBytes(_helloWorldApp);
            file.Write(_outputStream);
        }

        [Benchmark]
        public void HelloWorld_PEImage_Read()
        {
            var file = PEImage.FromBytes(_helloWorldApp);
        }

        [Benchmark]
        public void HelloWorld_PEImage_ReadWrite()
        {
            var image = PEImage.FromBytes(_helloWorldApp);
            new ManagedPEFileBuilder().CreateFile(image).Write(_outputStream);
        }

        [Benchmark]
        public void HelloWorld_ModuleDefinition_Read()
        {
            var file = ModuleDefinition.FromBytes(_helloWorldApp);
        }

        [Benchmark]
        public void HelloWorld_ModuleDefinition_ReadWrite()
        {
            var file = ModuleDefinition.FromBytes(_helloWorldApp);
            file.Write(_outputStream);
        }

        [Benchmark]
        public void CrackMe_PEFile_Read()
        {
            var file = PE.File.PEFile.FromBytes(_crackMeApp);
        }

        [Benchmark]
        public void CrackMe_PEFile_ReadWrite()
        {
            var file = PE.File.PEFile.FromBytes(_crackMeApp);
            file.Write(_outputStream);
        }

        [Benchmark]
        public void CrackMe_PEImage_Read()
        {
            var file = PEImage.FromBytes(_helloWorldApp);
        }

        [Benchmark]
        public void CrackMe_PEImage_ReadWrite()
        {
            var image = PEImage.FromBytes(_crackMeApp);
            new ManagedPEFileBuilder().CreateFile(image).Write(_outputStream);
        }

        [Benchmark]
        public void CrackMe_ModuleDefinition_Read()
        {
            var file = ModuleDefinition.FromBytes(_crackMeApp);
        }

        [Benchmark]
        public void CrackMe_ModuleDefinition_ReadWrite()
        {
            var file = ModuleDefinition.FromBytes(_crackMeApp);
            file.Write(_outputStream);
        }
    }
}
