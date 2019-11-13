using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.TestCases.NestedClasses;
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
            Assert.Equal("System.Object", module.TopLevelTypes[1].BaseType.FullName);
        }

        [Fact]
        public void ReadNestedTypes()
        {
            var module = ModuleDefinition.FromFile(typeof(TopLevelClass1).Assembly.Location);

            var class1 = module.TopLevelTypes.First(t => t.Name == nameof(TopLevelClass1));
            Assert.Equal(new HashSet<string>
            {
                nameof(TopLevelClass1.Nested1),
                nameof(TopLevelClass1.Nested2)
            }, class1.NestedTypes.Select(t => t.Name));

            var nested1 = class1.NestedTypes.First(t => t.Name == nameof(TopLevelClass1.Nested1));
            Assert.Equal(new HashSet<string>
            {
                nameof(TopLevelClass1.Nested1.Nested1Nested1),
                nameof(TopLevelClass1.Nested1.Nested1Nested2)
            }, nested1.NestedTypes.Select(t => t.Name));

            var nested2 = class1.NestedTypes.First(t => t.Name == nameof(TopLevelClass1.Nested2));
            Assert.Equal(new HashSet<string>
            {
                nameof(TopLevelClass1.Nested2.Nested2Nested1),
                nameof(TopLevelClass1.Nested2.Nested2Nested2)
            }, nested2.NestedTypes.Select(t => t.Name));

            var class2 = module.TopLevelTypes.First(t => t.Name == nameof(TopLevelClass2));
            Assert.Equal(new HashSet<string>
            {
                nameof(TopLevelClass2.Nested3),
                nameof(TopLevelClass2.Nested4)
            }, class2.NestedTypes.Select(t => t.Name));

            var nested3 = class2.NestedTypes.First(t => t.Name == nameof(TopLevelClass2.Nested3));
            Assert.Equal(new HashSet<string>
            {
                nameof(TopLevelClass2.Nested3.Nested3Nested1),
                nameof(TopLevelClass2.Nested3.Nested3Nested2)
            }, nested3.NestedTypes.Select(t => t.Name));

            var nested4 = class2.NestedTypes.First(t => t.Name == nameof(TopLevelClass2.Nested4));
            Assert.Equal(new HashSet<string>
            {
                nameof(TopLevelClass2.Nested4.Nested4Nested1),
                nameof(TopLevelClass2.Nested4.Nested4Nested2)
            }, nested4.NestedTypes.Select(t => t.Name));
        }
        
    }
}