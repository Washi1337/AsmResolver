using System;
using System.IO;
using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.IO;
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
        private static readonly IInputFile SystemPrivateCoreLib;
        private static readonly IInputFile SystemRuntime;
        private static readonly IInputFile SystemPrivateXml;

        private readonly MemoryStream _outputStream = new();

        static ModuleReadWriteBenchmark()
        {
            string runtimePath = DotNetCorePathProvider.Default
                .GetRuntimePathCandidates("Microsoft.NETCore.App", new Version(3, 1, 0))
                .FirstOrDefault() ?? throw new InvalidOperationException(".NET Core 3.1 is not installed.");

            var fs = new ByteArrayFileService();
            SystemPrivateCoreLib = fs.OpenFile(Path.Combine(runtimePath, "System.Private.CoreLib.dll"));
            SystemRuntime = fs.OpenFile(Path.Combine(runtimePath, "System.Runtime.dll"));
            SystemPrivateXml = fs.OpenFile(Path.Combine(runtimePath, "System.Private.Xml.dll"));
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
        public void SystemPrivateCoreLib_ReadWrite()
        {
            var module = ModuleDefinition.FromFile(SystemPrivateCoreLib);
            module.Write(_outputStream);
        }

        [Benchmark]
        public void SystemRuntimeLib_ReadWrite()
        {
            var module = ModuleDefinition.FromFile(SystemRuntime);
            module.Write(_outputStream);
        }

        [Benchmark]
        public void SystemPrivateXml_ReadWrite()
        {
            var module = ModuleDefinition.FromFile(SystemPrivateXml);
            module.Write(_outputStream);
        }
    }
}
