using System.IO;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class ModuleReferenceTest
    {
        [Fact]
        public void ReadName()
        {
            var module = ModuleDefinition.FromFile(Path.Combine("Resources", "Manifest.exe"), TestReaderParameters);
            var moduleRef = module.ModuleReferences[0];
            Assert.Equal("MyModel.netmodule", moduleRef.Name);
        }
    }
}
