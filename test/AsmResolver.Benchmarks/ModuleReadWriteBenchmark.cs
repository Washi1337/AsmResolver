using System;
using System.IO;
using AsmResolver.DotNet;
using BenchmarkDotNet.Attributes;
using static AsmResolver.Benchmarks.Properties.Resources;

namespace AsmResolver.Benchmarks
{
    [MemoryDiagnoser]
    public class ModuleReadWriteBenchmark
    {
        private static readonly byte[] HelloWorldApp = HelloWorld;
        private static readonly byte[] CrackMeApp = Test;
        private static readonly byte[] ManyMethods = Utilities.DecompressDeflate(HelloWorld_ManyMethods);
        private static readonly byte[] CoreLib;

        private readonly MemoryStream _outputStream = new();

        static ModuleReadWriteBenchmark()
        {
            var resolver = new DotNetCoreAssemblyResolver(new Version(3, 1, 0));
            string path = resolver.Resolve(KnownCorLibs.SystemPrivateCoreLib_v4_0_0_0)!.ManifestModule!.FilePath;
            CoreLib = File.ReadAllBytes(path);
        }

        [Benchmark]
        public void HelloWorld_Read()
        {
            var file = ModuleDefinition.FromBytes(HelloWorldApp);
        }

        [Benchmark]
        public void HelloWorld_ReadWrite()
        {
            var file = ModuleDefinition.FromBytes(HelloWorldApp);
            file.Write(_outputStream);
        }

        [Benchmark]
        public void CrackMe_Read()
        {
            var file = ModuleDefinition.FromBytes(CrackMeApp);
        }

        [Benchmark]
        public void CrackMe_ReadWrite()
        {
            var file = ModuleDefinition.FromBytes(CrackMeApp);
            file.Write(_outputStream);
        }

        [Benchmark]
        public void ManyMethods_Read()
        {
            var file = ModuleDefinition.FromBytes(ManyMethods);
        }

        [Benchmark]
        public void ManyMethods_ReadWrite()
        {
            var file = ModuleDefinition.FromBytes(ManyMethods);
            file.Write(_outputStream);
        }

        [Benchmark]
        public void CoreLib_ReadWrite()
        {
            var module = ModuleDefinition.FromBytes(CoreLib);
            module.Write(_outputStream);
        }
    }
}
