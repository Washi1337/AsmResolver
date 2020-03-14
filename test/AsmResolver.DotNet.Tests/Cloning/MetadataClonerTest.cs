using System;
using System.Linq;
using AsmResolver.DotNet.Cloning;
using AsmResolver.Tests.Runners;
using Xunit;

namespace AsmResolver.DotNet.Tests.Cloning
{
    public class MetadataClonerTest : IClassFixture<TemporaryDirectoryFixture>
    {
        private readonly TemporaryDirectoryFixture _fixture;

        public MetadataClonerTest(TemporaryDirectoryFixture fixture)
        {
            _fixture = fixture;
        }

        private static ModuleDefinition PrepareTempModule()
        {
            var assembly = new AssemblyDefinition("SomeAssembly", new Version(1, 0, 0, 0));
            var module = new ModuleDefinition("SomeModule");
            assembly.Modules.Add(module);
            return module;
        }

        [Fact]
        public void CloneHelloWorldProgramType()
        {
            var sourceModule = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var targetModule = PrepareTempModule();

            var result = new MetadataCloner(targetModule)
                .IncludeType(sourceModule.TopLevelTypes.First(t => t.Name == "Program"))
                .Clone();

            foreach (var type in result.ClonedMembers.OfType<TypeDefinition>())
                targetModule.TopLevelTypes.Add(type);

            targetModule.ManagedEntrypointMethod = (MethodDefinition) result.ClonedMembers.First(m => m.Name == "Main");
            _fixture
                .GetRunner<FrameworkPERunner>()
                .RebuildAndRun(targetModule, "HelloWorld.exe", "Hello, World" + Environment.NewLine);
        }
    }
}