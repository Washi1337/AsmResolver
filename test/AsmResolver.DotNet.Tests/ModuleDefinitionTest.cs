using System;
using AsmResolver.PE;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class ModuleDefinitionTest
    {
        [Fact]
        public void ReadNameTest()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            Assert.Equal("HelloWorld.exe", module.Name);
        }

        [Fact]
        public void ReadManifestModule()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            Assert.NotNull(module.Assembly);
            Assert.Same(module, module.Assembly.ManifestModule);
        }
    }
}