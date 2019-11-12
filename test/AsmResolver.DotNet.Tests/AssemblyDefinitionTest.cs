using System;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class AssemblyDefinitionTest
    {
        [Fact]
        public void ReadNameTest()
        {
            var assemblyDef = AssemblyDefinition.FromBytes(Properties.Resources.HelloWorld);
            Assert.Equal("HelloWorld", assemblyDef.Name);
        }

        [Fact]
        public void ReadVersion()
        {
            var assemblyDef = AssemblyDefinition.FromBytes(Properties.Resources.HelloWorld);
            Assert.Equal(new Version(1,0,0,0), assemblyDef.Version);
        }
    }
}