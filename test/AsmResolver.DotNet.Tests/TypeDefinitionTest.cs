using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class TypeDefinitionTest
    {
        [Fact]
        public void LinkedToModule()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            foreach (var type in module.TopLevelTypes)
                Assert.Same(module, type.Module);
        }
        
        [Fact]
        public void ReadName()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            Assert.Equal("<Module>", module.TopLevelTypes[0].Name);
            Assert.Equal("Program", module.TopLevelTypes[1].Name);
        }

        [Fact]
        public void ReadNamespace()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            Assert.Null(module.TopLevelTypes[0].Namespace);
            Assert.Equal("HelloWorld", module.TopLevelTypes[1].Namespace);
        }

        [Fact]
        public void ReadBaseType()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            Assert.Null(module.TopLevelTypes[0].BaseType);
            Assert.NotNull(module.TopLevelTypes[1].BaseType);
            Assert.Equal("System", module.TopLevelTypes[1].BaseType.Namespace);
            Assert.Equal("Object", module.TopLevelTypes[1].BaseType.Name);
        }
    }
}