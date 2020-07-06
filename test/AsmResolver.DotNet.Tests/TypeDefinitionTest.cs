using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.DotNet.TestCases.CustomAttributes;
using AsmResolver.DotNet.TestCases.Events;
using AsmResolver.DotNet.TestCases.Fields;
using AsmResolver.DotNet.TestCases.Generics;
using AsmResolver.DotNet.TestCases.Methods;
using AsmResolver.DotNet.TestCases.NestedClasses;
using AsmResolver.DotNet.TestCases.Properties;
using AsmResolver.DotNet.TestCases.Types;
using AsmResolver.DotNet.TestCases.Types.Structs;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class TypeDefinitionTest
    {
        private TypeDefinition RebuildAndLookup(TypeDefinition type)
        {
            var stream = new MemoryStream();
            type.Module.Write(stream);

            var newModule = ModuleDefinition.FromBytes(stream.ToArray());
            return newModule.TopLevelTypes.FirstOrDefault(t => t.FullName == type.FullName);
        }

        private void AssertNamesEqual(IEnumerable<INameProvider> expected, IEnumerable<INameProvider> actual)
        {
            Assert.Equal(expected.Select(n => n.Name), actual.Select(n => n.Name));
        }

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
        public void NameIsPersistentAfterRebuild()
        {
            const string newName = "SomeType";

            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var type = module.TopLevelTypes.First(t => t.Name == "Program");
            type.Name = newName;

            var newType = RebuildAndLookup(type);
            Assert.Equal(newName, newType.Name);
        }

        [Fact]
        public void ReadNamespace()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            Assert.Null(module.TopLevelTypes[0].Namespace);
            Assert.Equal("HelloWorld", module.TopLevelTypes[1].Namespace);
        }

        [Fact]
        public void NonNullNamespaceIsPersistentAfterRebuild()
        {
            const string newNameSpace = "SomeNamespace";

            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var type = module.TopLevelTypes.First(t => t.Name == "Program");
            type.Namespace = newNameSpace;

            var newType = RebuildAndLookup(type);
            Assert.Equal(newNameSpace, newType.Namespace);
        }

        [Fact]
        public void NullNamespaceIsPersistentAfterRebuild()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var type = module.TopLevelTypes.First(t => t.Name == "Program");
            type.Namespace = null;

            var newType = RebuildAndLookup(type);
            Assert.Null(newType.Namespace);
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
        public void PersistentEmptyFields()
        {
            var module = ModuleDefinition.FromFile(typeof(NoFields).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(NoFields));
            var newType = RebuildAndLookup(type);
            AssertNamesEqual(type.Fields, newType.Fields);
        }

        [Fact]
        public void ReadSingleField()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleField).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleField));
            Assert.Equal(new[]
            {
                nameof(SingleField.IntField),
            }, type.Fields.Select(p => p.Name));
        }

        [Fact]
        public void PersistentSingleField()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleField).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleField));
            var newType = RebuildAndLookup(type);
            AssertNamesEqual(type.Fields, newType.Fields);
        }

        [Fact]
        public void ReadMultipleFields()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleFields).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleFields));
            Assert.Equal(new[]
            {
                nameof(MultipleFields.IntField),
                nameof(MultipleFields.StringField),
                nameof(MultipleFields.TypeDefOrRefFieldType),
            }, type.Fields.Select(p => p.Name));
        }

        [Fact]
        public void PersistentMultipleFields()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleFields).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleFields));
            var newType = RebuildAndLookup(type);
            AssertNamesEqual(type.Fields, newType.Fields);
        }

        [Fact]
        public void ReadEmptyMethods()
        {
            var module = ModuleDefinition.FromFile(typeof(NoMethods).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(NoMethods));
            Assert.Empty(type.Methods);
        }

        [Fact]
        public void PersistentEmptyMethods()
        {
            var module = ModuleDefinition.FromFile(typeof(NoMethods).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(NoMethods));
            var newType = RebuildAndLookup(type);
            Assert.Empty(newType.Methods);
        }

        [Fact]
        public void ReadSingleMethod()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleMethod).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleMethod));
            Assert.Equal(new[]
            {
                nameof(SingleMethod.VoidParameterlessMethod),
            }, type.Methods.Select(p => p.Name));
        }

        [Fact]
        public void PersistentSingleMethod()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleMethod).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleMethod));
            var newType = RebuildAndLookup(type);
            AssertNamesEqual(type.Methods, newType.Methods);
        }

        [Fact]
        public void ReadMultipleMethods()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleMethods));
            Assert.Equal(new[]
            {
                ".ctor",
                nameof(MultipleMethods.VoidParameterlessMethod),
                nameof(MultipleMethods.IntParameterlessMethod),
                nameof(MultipleMethods.TypeDefOrRefParameterlessMethod),
                nameof(MultipleMethods.SingleParameterMethod),
                nameof(MultipleMethods.MultipleParameterMethod),
            }, type.Methods.Select(p => p.Name));
        }

        [Fact]
        public void PersistentMultipleMethods()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleMethods));
            var newType = RebuildAndLookup(type);
            AssertNamesEqual(type.Methods, newType.Methods);
        }

        [Fact]
        public void ReadEmptyProperties()
        {
            var module = ModuleDefinition.FromFile(typeof(NoProperties).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(NoProperties));
            Assert.Empty(type.Properties);
        }

        [Fact]
        public void PersistentEmptyProperties()
        {
            var module = ModuleDefinition.FromFile(typeof(NoProperties).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(NoProperties));
            var newType = RebuildAndLookup(type);
            Assert.Empty(newType.Properties);
        }

        [Fact]
        public void ReadSingleProperty()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleProperty).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleProperty));
            Assert.Equal(new[]
            {
                nameof(SingleProperty.IntProperty)
            }, type.Properties.Select(p => p.Name));
        }

        [Fact]
        public void PersistentSingleProperty()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleProperty).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleProperty));
            var newType = RebuildAndLookup(type);
            Assert.Equal(new[]
            {
                nameof(SingleProperty.IntProperty)
            }, newType.Properties.Select(p => p.Name));
        }

        [Fact]
        public void ReadMultipleProperties()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleProperties).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleProperties));
            Assert.Equal(new[]
            {
                nameof(MultipleProperties.ReadOnlyProperty), nameof(MultipleProperties.WriteOnlyProperty),
                nameof(MultipleProperties.ReadWriteProperty), "Item",
            }, type.Properties.Select(p => p.Name));
        }

        [Fact]
        public void PersistentMultipleProperties()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleProperties).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleProperties));
            var newType = RebuildAndLookup(type);
            Assert.Equal(new[]
            {
                nameof(MultipleProperties.ReadOnlyProperty), nameof(MultipleProperties.WriteOnlyProperty),
                nameof(MultipleProperties.ReadWriteProperty), "Item",
            }, newType.Properties.Select(p => p.Name));
        }

        [Fact]
        public void ReadEmptyEvents()
        {
            var module = ModuleDefinition.FromFile(typeof(NoEvents).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(NoEvents));
            Assert.Empty(type.Events);
        }

        [Fact]
        public void PersistentEmptyEvent()
        {
            var module = ModuleDefinition.FromFile(typeof(NoEvents).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(NoEvents));
            var newType = RebuildAndLookup(type);
            Assert.Empty(newType.Events);
        }

        [Fact]
        public void ReadSingleEvent()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleEvent).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleEvent));
            Assert.Equal(new[]
            {
                nameof(SingleEvent.SimpleEvent)
            }, type.Events.Select(p => p.Name));
        }

        [Fact]
        public void PersistentSingleEvent()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleEvent).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleEvent));
            var newType = RebuildAndLookup(type);
            Assert.Equal(new[]
            {
                nameof(SingleEvent.SimpleEvent)
            }, newType.Events.Select(p => p.Name));
        }

        [Fact]
        public void ReadMultipleEvents()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleEvents).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleEvents));
            Assert.Equal(new[]
            {
                nameof(MultipleEvents.Event1),
                nameof(MultipleEvents.Event2),
                nameof(MultipleEvents.Event3),
            }, type.Events.Select(p => p.Name));
        }

        [Fact]
        public void PersistentMultipleEvents()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleEvents).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleEvents));
            var newType = RebuildAndLookup(type);
            Assert.Equal(new[]
            {
                nameof(MultipleEvents.Event1),
                nameof(MultipleEvents.Event2),
                nameof(MultipleEvents.Event3),
            }, newType.Events.Select(p => p.Name));
        }

        [Fact]
        public void ReadCustomAttributes()
        {
            var module = ModuleDefinition.FromFile(typeof(CustomAttributesTestClass).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(CustomAttributesTestClass));
            Assert.Single(type.CustomAttributes);
        }

        [Fact]
        public void ReadGenericParameters()
        {
            var module = ModuleDefinition.FromFile(typeof(GenericType<,,>).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == typeof(GenericType<,,>).Name);
            Assert.Equal(3, type.GenericParameters.Count);
        }

        [Fact]
        public void ReadInterfaces()
        {
            var module = ModuleDefinition.FromFile(typeof(InterfaceImplementations).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(InterfaceImplementations));
            Assert.Equal(new HashSet<string>(new[]
            {
                nameof(IInterface1), nameof(IInterface2),
            }), new HashSet<string>(type.Interfaces.Select(i => i.Interface.Name)));
        }

        [Fact]
        public void PersistentInterfaces()
        {
            var module = ModuleDefinition.FromFile(typeof(InterfaceImplementations).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(InterfaceImplementations));
            var newType = RebuildAndLookup(type);
            Assert.Equal(new HashSet<string>(new[]
            {
                nameof(IInterface1), nameof(IInterface2),
            }), new HashSet<string>(newType.Interfaces.Select(i => i.Interface.Name)));
        }

        [Fact]
        public void ReadMethodImplementations()
        {
            var module = ModuleDefinition.FromFile(typeof(InterfaceImplementations).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(InterfaceImplementations));

            Assert.Contains(type.MethodImplementations, i => i.Declaration.Name == "Interface2Method");
        }

        [Fact]
        public void PersistentMethodImplementations()
        {
            var module = ModuleDefinition.FromFile(typeof(InterfaceImplementations).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(InterfaceImplementations));
            var newType = RebuildAndLookup(type);

            Assert.Contains(newType.MethodImplementations, i => i.Declaration.Name == "Interface2Method");
        }

        [Fact]
        public void ReadClassLayout()
        {
            var module = ModuleDefinition.FromFile(typeof(ExplicitSizeStruct).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(ExplicitSizeStruct));

            Assert.NotNull(type.ClassLayout);
            Assert.Equal(100u, type.ClassLayout.ClassSize);
        }

        [Fact]
        public void PersistentClassLayout()
        {
            var module = ModuleDefinition.FromFile(typeof(ExplicitSizeStruct).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(ExplicitSizeStruct));
            var newType = RebuildAndLookup(type);

            Assert.NotNull(newType.ClassLayout);
            Assert.Equal(100u, newType.ClassLayout.ClassSize);
        }

        [Fact]
        public void InheritanceMultipleLevels()
        {
            var module = ModuleDefinition.FromFile(typeof(DerivedDerivedClass).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(DerivedDerivedClass));

            Assert.True(type.InheritsFrom(typeof(AbstractClass).FullName));
            Assert.False(type.InheritsFrom(typeof(Class).FullName));
        }

        [Fact]
        public void InterfaceImplementedFromInheritanceHierarchy()
        {
            var module = ModuleDefinition.FromFile(typeof(DerivedInterfaceImplementations).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(DerivedInterfaceImplementations));

            Assert.True(type.Implements(typeof(IInterface1).FullName));
            Assert.True(type.Implements(typeof(IInterface2).FullName));
            Assert.True(type.Implements(typeof(IInterface3).FullName));
            Assert.False(type.Implements(typeof(IInterface4).FullName));
        }
    }
}