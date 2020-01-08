using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.TestCases.CustomAttributes;
using AsmResolver.DotNet.TestCases.Events;
using AsmResolver.DotNet.TestCases.Fields;
using AsmResolver.DotNet.TestCases.Methods;
using AsmResolver.DotNet.TestCases.NestedClasses;
using AsmResolver.DotNet.TestCases.Properties;
using AsmResolver.PE.DotNet.Metadata.Tables;
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
            
            Assert.Same(class1, nested1.DeclaringType);
            Assert.Same(class1, nested2.DeclaringType);
            Assert.Same(class2, nested3.DeclaringType);
            Assert.Same(class2, nested4.DeclaringType);
            Assert.Same(module, nested1.Module);
            Assert.Same(module, nested2.Module);
            Assert.Same(module, nested3.Module);
            Assert.Same(module, nested4.Module);
        }

        [Fact]
        public void ResolveNestedType()
        {
            var module = ModuleDefinition.FromFile(typeof(TopLevelClass1).Assembly.Location);
            var member = (TypeDefinition) module.LookupMember(new MetadataToken(TableIndex.TypeDef, 4));
            Assert.NotNull(member.DeclaringType);
            Assert.Equal(nameof(TopLevelClass1), member.DeclaringType.Name);
        }

        [Fact]
        public void ReadEmptyFields()
        {
            var module = ModuleDefinition.FromFile(typeof(NoFields).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(NoFields));
            Assert.Empty(type.Fields);
        }

        [Fact]
        public void ReadSingleField()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleField).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleField));
            Assert.Single(type.Fields);
        }

        [Fact]
        public void ReadEmptyMethods()
        {
            var module = ModuleDefinition.FromFile(typeof(NoMethods).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(NoMethods));
            Assert.Empty(type.Methods);
        }
        
        [Fact]
        public void ReadSingleMethod()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleMethod).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleMethod));
            Assert.Single(type.Methods);
        }

        [Fact]
        public void ReadMultipleMethods()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleMethods));
            Assert.Equal(6, type.Methods.Count);
        }

        [Fact]
        public void ReadEmptyProperties()
        {
            var module = ModuleDefinition.FromFile(typeof(NoProperties).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(NoProperties));
            Assert.Empty(type.Properties);
        }
        
        [Fact]
        public void ReadSingleProperty()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleProperty).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleProperty));
            Assert.Single(type.Properties);
        }

        [Fact]
        public void ReadMultipleProperties()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleProperties).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleProperties));
            Assert.Equal(4, type.Properties.Count);
        }

        [Fact]
        public void ReadEmptyEvents()
        {
            var module = ModuleDefinition.FromFile(typeof(NoEvents).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(NoEvents));
            Assert.Empty(type.Events);
        }
        
        [Fact]
        public void ReadSingleEvent()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleEvent).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleEvent));
            Assert.Single(type.Events);
        }

        [Fact]
        public void ReadMultipleEvents()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleEvents).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleEvents));
            Assert.Equal(3, type.Events.Count);
        }

        [Fact]
        public void ReadCustomAttributes()
        {
            var module = ModuleDefinition.FromFile(typeof(CustomAttributesTestClass).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(CustomAttributesTestClass));
            Assert.Single(type.CustomAttributes);
        }
    }
}