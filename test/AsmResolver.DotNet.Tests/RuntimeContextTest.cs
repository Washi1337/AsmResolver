using AsmResolver.DotNet.Serialized;
using AsmResolver.IO;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class RuntimeContextTest
    {
        [Fact]
        public void ResolveDependencyShouldUseSameRuntimeContext()
        {
            var main = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var dependency = main.CorLibTypeFactory.CorLibScope.GetAssembly()!.Resolve()!.ManifestModule!;

            Assert.Same(main.RuntimeContext, dependency.RuntimeContext);
        }

        [Fact]
        public void ResolveDependencyShouldUseSameFileService()
        {
            var service = new ByteArrayFileService();
            service.OpenBytesAsFile("HelloWorld.dll", Properties.Resources.HelloWorld);

            var main = ModuleDefinition.FromFile("HelloWorld.dll", new ModuleReaderParameters(service));
            var dependency = main.CorLibTypeFactory.CorLibScope.GetAssembly()!.Resolve()!.ManifestModule!;

            Assert.Contains(main.FilePath, service.GetOpenedFiles());
            Assert.Contains(dependency.FilePath, service.GetOpenedFiles());
        }
    }
}
