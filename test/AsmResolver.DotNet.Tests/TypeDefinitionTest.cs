using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.DotNet.Signatures;
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
        private static readonly SignatureComparer Comparer = new();

        private TypeDefinition RebuildAndLookup(TypeDefinition type)
        {
            var stream = new MemoryStream();
            type.DeclaringModule!.Write(stream);

            var newModule = ModuleDefinition.FromBytes(stream.ToArray(), TestReaderParameters);
            return newModule.TopLevelTypes.FirstOrDefault(t => t.FullName == type.FullName);
        }

        private void AssertNamesEqual(IEnumerable<INameProvider> expected, IEnumerable<INameProvider> actual)
        {
            Assert.Equal(expected.Select(n => n.Name), actual.Select(n => n.Name));
        }

        [Fact]
        public void LinkedToModule()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            foreach (var type in module.TopLevelTypes)
                Assert.Same(module, type.DeclaringModule);
        }

        [Fact]
        public void ReadName()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            Assert.Equal("<Module>", module.TopLevelTypes[0].Name);
            Assert.Equal("Program", module.TopLevelTypes[1].Name);
        }

        [Fact]
        public void ReadNameFromNormalMetadata()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_DoubleStringsStream, TestReaderParameters);
            Assert.Equal("Class_2", module.TopLevelTypes[1].Name);
        }

        [Fact]
        public void ReadNameFromEnCMetadata()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_DoubleStringsStream_EnC, TestReaderParameters);
            Assert.Equal("Class_1", module.TopLevelTypes[1].Name);
        }

        [Fact]
        public void NameIsPersistentAfterRebuild()
        {
            const string newName = "SomeType";

            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == "Program");
            type.Name = newName;

            var newType = RebuildAndLookup(type);
            Assert.Equal(newName, newType.Name);
        }

        [Fact]
        public void ReadNamespace()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            Assert.Null(module.TopLevelTypes[0].Namespace);
            Assert.Equal("HelloWorld", module.TopLevelTypes[1].Namespace);
        }

        [Fact]
        public void ReadTopLevelTypeFullName()
        {
            var module = ModuleDefinition.FromFile(typeof(Class).Assembly.Location, TestReaderParameters);
            var type = (TypeDefinition) module.LookupMember(typeof(Class).MetadataToken);
            Assert.Equal("AsmResolver.DotNet.TestCases.Types.Class", type.FullName);
        }

        [Fact]
        public void NonNullNamespaceIsPersistentAfterRebuild()
        {
            const string newNameSpace = "SomeNamespace";

            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == "Program");
            type.Namespace = newNameSpace;

            var newType = RebuildAndLookup(type);
            Assert.Equal(newNameSpace, newType.Namespace);
        }

        [Fact]
        public void NullNamespaceIsPersistentAfterRebuild()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == "Program");
            type.Namespace = null;

            var newType = RebuildAndLookup(type);
            Assert.Null(newType.Namespace);
        }

        [Fact]
        public void ReadBaseType()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            Assert.Null(module.TopLevelTypes[0].BaseType);
            Assert.NotNull(module.TopLevelTypes[1].BaseType);
            Assert.Equal("System.Object", module.TopLevelTypes[1].BaseType.FullName);
        }

        [Fact]
        public void ReadNestedTypes()
        {
            var module = ModuleDefinition.FromFile(typeof(TopLevelClass1).Assembly.Location, TestReaderParameters);

            var class1 = module.TopLevelTypes.First(t => t.Name == nameof(TopLevelClass1));
            Assert.Equal(new Utf8String[]
            {
                nameof(TopLevelClass1.Nested1),
                nameof(TopLevelClass1.Nested2)
            }, class1.NestedTypes.Select(t => t.Name));

            var nested1 = class1.NestedTypes.First(t => t.Name == nameof(TopLevelClass1.Nested1));
            Assert.Equal(new Utf8String[]
            {
                nameof(TopLevelClass1.Nested1.Nested1Nested1),
                nameof(TopLevelClass1.Nested1.Nested1Nested2)
            }, nested1.NestedTypes.Select(t => t.Name));

            var nested2 = class1.NestedTypes.First(t => t.Name == nameof(TopLevelClass1.Nested2));
            Assert.Equal(new Utf8String[]
            {
                nameof(TopLevelClass1.Nested2.Nested2Nested1),
                nameof(TopLevelClass1.Nested2.Nested2Nested2)
            }, nested2.NestedTypes.Select(t => t.Name));

            var class2 = module.TopLevelTypes.First(t => t.Name == nameof(TopLevelClass2));
            Assert.Equal(new Utf8String[]
            {
                nameof(TopLevelClass2.Nested3),
                nameof(TopLevelClass2.Nested4)
            }, class2.NestedTypes.Select(t => t.Name));

            var nested3 = class2.NestedTypes.First(t => t.Name == nameof(TopLevelClass2.Nested3));
            Assert.Equal(new Utf8String[]
            {
                nameof(TopLevelClass2.Nested3.Nested3Nested1),
                nameof(TopLevelClass2.Nested3.Nested3Nested2)
            }, nested3.NestedTypes.Select(t => t.Name));

            var nested4 = class2.NestedTypes.First(t => t.Name == nameof(TopLevelClass2.Nested4));
            Assert.Equal(new Utf8String[]
            {
                nameof(TopLevelClass2.Nested4.Nested4Nested1),
                nameof(TopLevelClass2.Nested4.Nested4Nested2)
            }, nested4.NestedTypes.Select(t => t.Name));

            Assert.Same(class1, nested1.DeclaringType);
            Assert.Same(class1, nested2.DeclaringType);
            Assert.Same(class2, nested3.DeclaringType);
            Assert.Same(class2, nested4.DeclaringType);
            Assert.Same(module, nested1.DeclaringModule);
            Assert.Same(module, nested2.DeclaringModule);
            Assert.Same(module, nested3.DeclaringModule);
            Assert.Same(module, nested4.DeclaringModule);
        }

        [Fact]
        public void ReadNestedFullName()
        {
            var module = ModuleDefinition.FromFile(typeof(TopLevelClass1).Assembly.Location, TestReaderParameters);
            var type = (TypeDefinition) module.LookupMember(typeof(TopLevelClass1.Nested1).MetadataToken);
            Assert.Equal("AsmResolver.DotNet.TestCases.NestedClasses.TopLevelClass1+Nested1", type.FullName);
        }

        [Fact]
        public void ReadNestedNestedFullName()
        {
            var module = ModuleDefinition.FromFile(typeof(TopLevelClass1).Assembly.Location, TestReaderParameters);
            var type = (TypeDefinition) module.LookupMember(typeof(TopLevelClass1.Nested1.Nested1Nested2)
                .MetadataToken);
            Assert.Equal("AsmResolver.DotNet.TestCases.NestedClasses.TopLevelClass1+Nested1+Nested1Nested2",
                type.FullName);
        }

        [Fact]
        public void ResolveNestedType()
        {
            var module = ModuleDefinition.FromFile(typeof(TopLevelClass1).Assembly.Location, TestReaderParameters);
            var member = (TypeDefinition) module.LookupMember(new MetadataToken(TableIndex.TypeDef, 4));
            Assert.NotNull(member.DeclaringType);
            Assert.Equal(nameof(TopLevelClass1), member.DeclaringType.Name);
        }

        [Fact]
        public void ReadEmptyFields()
        {
            var module = ModuleDefinition.FromFile(typeof(NoFields).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(NoFields));
            Assert.Empty(type.Fields);
        }

        [Fact]
        public void PersistentEmptyFields()
        {
            var module = ModuleDefinition.FromFile(typeof(NoFields).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(NoFields));
            var newType = RebuildAndLookup(type);
            AssertNamesEqual(type.Fields, newType.Fields);
        }

        [Fact]
        public void ReadSingleField()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleField).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleField));
            Assert.Equal(new Utf8String[]
            {
                nameof(SingleField.IntField),
            }, type.Fields.Select(p => p.Name));
        }

        [Fact]
        public void PersistentSingleField()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleField).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleField));
            var newType = RebuildAndLookup(type);
            AssertNamesEqual(type.Fields, newType.Fields);
        }

        [Fact]
        public void ReadMultipleFields()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleFields).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleFields));
            Assert.Equal(new Utf8String[]
            {
                nameof(MultipleFields.IntField),
                nameof(MultipleFields.StringField),
                nameof(MultipleFields.TypeDefOrRefFieldType),
            }, type.Fields.Select(p => p.Name));
        }

        [Fact]
        public void PersistentMultipleFields()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleFields).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleFields));
            var newType = RebuildAndLookup(type);
            AssertNamesEqual(type.Fields, newType.Fields);
        }

        [Fact]
        public void ReadEmptyMethods()
        {
            var module = ModuleDefinition.FromFile(typeof(NoMethods).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(NoMethods));
            Assert.Empty(type.Methods);
        }

        [Fact]
        public void PersistentEmptyMethods()
        {
            var module = ModuleDefinition.FromFile(typeof(NoMethods).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(NoMethods));
            var newType = RebuildAndLookup(type);
            Assert.Empty(newType.Methods);
        }

        [Fact]
        public void ReadSingleMethod()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleMethod).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleMethod));
            Assert.Equal(new Utf8String[]
            {
                nameof(SingleMethod.VoidParameterlessMethod),
            }, type.Methods.Select(p => p.Name));
        }

        [Fact]
        public void PersistentSingleMethod()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleMethod).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleMethod));
            var newType = RebuildAndLookup(type);
            AssertNamesEqual(type.Methods, newType.Methods);
        }

        [Fact]
        public void ReadMultipleMethods()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleMethods));
            Assert.Equal(new Utf8String[]
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
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleMethods));
            var newType = RebuildAndLookup(type);
            AssertNamesEqual(type.Methods, newType.Methods);
        }

        [Fact]
        public void ReadEmptyProperties()
        {
            var module = ModuleDefinition.FromFile(typeof(NoProperties).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(NoProperties));
            Assert.Empty(type.Properties);
        }

        [Fact]
        public void PersistentEmptyProperties()
        {
            var module = ModuleDefinition.FromFile(typeof(NoProperties).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(NoProperties));
            var newType = RebuildAndLookup(type);
            Assert.Empty(newType.Properties);
        }

        [Fact]
        public void ReadSingleProperty()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleProperty).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleProperty));
            Assert.Equal(new Utf8String[]
            {
                nameof(SingleProperty.IntProperty)
            }, type.Properties.Select(p => p.Name));
        }

        [Fact]
        public void PersistentSingleProperty()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleProperty).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleProperty));
            var newType = RebuildAndLookup(type);
            Assert.Equal(new Utf8String[]
            {
                nameof(SingleProperty.IntProperty)
            }, newType.Properties.Select(p => p.Name));
        }

        [Fact]
        public void ReadMultipleProperties()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleProperties).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleProperties));
            Assert.Equal(new Utf8String[]
            {
                nameof(MultipleProperties.ReadOnlyProperty), nameof(MultipleProperties.WriteOnlyProperty),
                nameof(MultipleProperties.ReadWriteProperty), "Item",
            }, type.Properties.Select(p => p.Name));
        }

        [Fact]
        public void PersistentMultipleProperties()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleProperties).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleProperties));
            var newType = RebuildAndLookup(type);
            Assert.Equal(new Utf8String[]
            {
                nameof(MultipleProperties.ReadOnlyProperty), nameof(MultipleProperties.WriteOnlyProperty),
                nameof(MultipleProperties.ReadWriteProperty), "Item",
            }, newType.Properties.Select(p => p.Name));
        }

        [Fact]
        public void ReadEmptyEvents()
        {
            var module = ModuleDefinition.FromFile(typeof(NoEvents).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(NoEvents));
            Assert.Empty(type.Events);
        }

        [Fact]
        public void PersistentEmptyEvent()
        {
            var module = ModuleDefinition.FromFile(typeof(NoEvents).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(NoEvents));
            var newType = RebuildAndLookup(type);
            Assert.Empty(newType.Events);
        }

        [Fact]
        public void ReadSingleEvent()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleEvent).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleEvent));
            Assert.Equal(new Utf8String[]
            {
                nameof(SingleEvent.SimpleEvent)
            }, type.Events.Select(p => p.Name));
        }

        [Fact]
        public void PersistentSingleEvent()
        {
            var module = ModuleDefinition.FromFile(typeof(SingleEvent).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(SingleEvent));
            var newType = RebuildAndLookup(type);
            Assert.Equal(new Utf8String[]
            {
                nameof(SingleEvent.SimpleEvent)
            }, newType.Events.Select(p => p.Name));
        }

        [Fact]
        public void ReadMultipleEvents()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleEvents).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleEvents));
            Assert.Equal(new Utf8String[]
            {
                nameof(MultipleEvents.Event1),
                nameof(MultipleEvents.Event2),
                nameof(MultipleEvents.Event3),
            }, type.Events.Select(p => p.Name));
        }

        [Fact]
        public void PersistentMultipleEvents()
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleEvents).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleEvents));
            var newType = RebuildAndLookup(type);
            Assert.Equal(new Utf8String[]
            {
                nameof(MultipleEvents.Event1),
                nameof(MultipleEvents.Event2),
                nameof(MultipleEvents.Event3),
            }, newType.Events.Select(p => p.Name));
        }

        [Fact]
        public void ReadCustomAttributes()
        {
            var module = ModuleDefinition.FromFile(typeof(CustomAttributesTestClass).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(CustomAttributesTestClass));
            Assert.Single(type.CustomAttributes);
        }

        [Fact]
        public void ReadGenericParameters()
        {
            var module = ModuleDefinition.FromFile(typeof(GenericType<,,>).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == typeof(GenericType<,,>).Name);
            Assert.True(type.HasGenericParameters);
            Assert.Equal(3, type.GenericParameters.Count);
        }

        [Fact]
        public void ReadNoGenericParameters()
        {
            var module = ModuleDefinition.FromFile(typeof(GenericType<,,>).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(NonGenericType));
            Assert.False(type.HasGenericParameters);
            Assert.Empty(type.GenericParameters);
        }

        [Fact]
        public void AddGenericParameter()
        {
            var module = ModuleDefinition.FromFile(typeof(GenericType<,,>).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(NonGenericType));
            Assert.False(type.HasGenericParameters);
            type.GenericParameters.Add(new GenericParameter("T"));
            Assert.True(type.HasGenericParameters);
        }

        [Fact]
        public void ReadInterfaces()
        {
            var module = ModuleDefinition.FromFile(typeof(InterfaceImplementations).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(InterfaceImplementations));
            Assert.Equal(new HashSet<Utf8String>(new Utf8String[]
            {
                nameof(IInterface1), nameof(IInterface2),
            }), new HashSet<Utf8String>(type.Interfaces.Select(i => i.Interface?.Name)));
        }

        [Fact]
        public void PersistentInterfaces()
        {
            var module = ModuleDefinition.FromFile(typeof(InterfaceImplementations).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(InterfaceImplementations));
            var newType = RebuildAndLookup(type);
            Assert.Equal(new HashSet<Utf8String>(new Utf8String[]
            {
                nameof(IInterface1), nameof(IInterface2),
            }), new HashSet<Utf8String>(newType.Interfaces.Select(i => i.Interface?.Name)));
        }

        [Fact]
        public void ReadMethodImplementations()
        {
            var module = ModuleDefinition.FromFile(typeof(InterfaceImplementations).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(InterfaceImplementations));

            Assert.Contains(type.MethodImplementations, i => i.Declaration!.Name == "Interface2Method");
        }

        [Fact]
        public void PersistentMethodImplementations()
        {
            var module = ModuleDefinition.FromFile(typeof(InterfaceImplementations).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(InterfaceImplementations));
            var newType = RebuildAndLookup(type);

            Assert.Contains(newType.MethodImplementations, i => i.Declaration!.Name == "Interface2Method");
        }

        [Fact]
        public void ReadClassLayout()
        {
            var module = ModuleDefinition.FromFile(typeof(ExplicitSizeStruct).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(ExplicitSizeStruct));

            Assert.NotNull(type.ClassLayout);
            Assert.Equal(100u, type.ClassLayout.ClassSize);
        }

        [Fact]
        public void PersistentClassLayout()
        {
            var module = ModuleDefinition.FromFile(typeof(ExplicitSizeStruct).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(ExplicitSizeStruct));
            var newType = RebuildAndLookup(type);

            Assert.NotNull(newType.ClassLayout);
            Assert.Equal(100u, newType.ClassLayout.ClassSize);
        }

        [Fact]
        public void InheritanceMultipleLevels()
        {
            var module = ModuleDefinition.FromFile(typeof(DerivedDerivedClass).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(DerivedDerivedClass));

            Assert.True(type.InheritsFrom(typeof(AbstractClass).FullName!));
            Assert.False(type.InheritsFrom(typeof(Class).FullName!));
        }

        [Fact]
        public void InheritanceMultipleLevelsTypeOf()
        {
            var module = ModuleDefinition.FromFile(typeof(DerivedDerivedClass).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(DerivedDerivedClass));

            Assert.True(type.InheritsFrom(typeof(AbstractClass).Namespace, nameof(AbstractClass)));
            Assert.False(type.InheritsFrom(typeof(Class).Namespace, nameof(Class)));
        }

        [Fact]
        public void InterfaceImplementedFromInheritanceHierarchy()
        {
            var module = ModuleDefinition.FromFile(typeof(DerivedInterfaceImplementations).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(DerivedInterfaceImplementations));

            Assert.True(type.Implements(typeof(IInterface1).FullName!));
            Assert.True(type.Implements(typeof(IInterface2).FullName!));
            Assert.True(type.Implements(typeof(IInterface3).FullName!));
            Assert.False(type.Implements(typeof(IInterface4).FullName!));
        }

        [Fact]
        public void InterfaceImplementedFromInheritanceHierarchyTypeOf()
        {
            var module = ModuleDefinition.FromFile(typeof(DerivedInterfaceImplementations).Assembly.Location, TestReaderParameters);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(DerivedInterfaceImplementations));

            Assert.True(type.Implements(typeof(IInterface1).Namespace, nameof(IInterface1)));
            Assert.True(type.Implements(typeof(IInterface2).Namespace, nameof(IInterface2)));
            Assert.True(type.Implements(typeof(IInterface3).Namespace, nameof(IInterface3)));
            Assert.False(type.Implements(typeof(IInterface4).Namespace, nameof(IInterface4)));
        }

        [Fact]
        public void CorLibTypeDefinitionToSignatureShouldResultInCorLibTypeSignature()
        {
            var module = new ModuleDefinition("Test");
            var type = module.CorLibTypeFactory.Object.Resolve()!;
            var signature = type.ToTypeSignature();
            var corlibType = Assert.IsAssignableFrom<CorLibTypeSignature>(signature);
            Assert.Equal(ElementType.Object, corlibType.ElementType);
        }

        [Fact]
        public void InvalidMetadataLoopInBaseTypeShouldNotCrashIsValueType()
        {
            var module = new ModuleDefinition("Test");
            var typeA = new TypeDefinition(null, "A", TypeAttributes.Public);
            var typeB = new TypeDefinition(null, "B", TypeAttributes.Public, typeA);
            typeA.BaseType = typeB;
            module.TopLevelTypes.Add(typeA);
            module.TopLevelTypes.Add(typeB);

            Assert.False(typeB.IsValueType);
            Assert.False(typeB.IsEnum);
        }

        [Fact]
        public void AddTypeWithCorLibBaseTypeToAssemblyWithCorLibTypeReferenceInAttribute()
        {
            // https://github.com/Washi1337/AsmResolver/issues/263

            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_WithAttribute, TestReaderParameters);
            var corlib = module.CorLibTypeFactory;

            var type = new TypeDefinition(null, "Test", TypeAttributes.Class, corlib.Object.Type);
            module.TopLevelTypes.Add(type);

            var scope = corlib.Object.Scope;

            var newType = RebuildAndLookup(type);
            Assert.Equal(newType.BaseType, type.BaseType, Comparer);

            Assert.Same(scope, corlib.Object.Scope);
            var reference = Assert.IsAssignableFrom<AssemblyReference>(corlib.Object.Scope!.GetAssembly());
            Assert.Same(module, reference.ContextModule);
        }

        [Fact]
        public void ReadIsByRefLike()
        {
            var resolver = new DotNetCoreAssemblyResolver(new Version(8, 0));
            var corLib = resolver.Resolve(KnownCorLibs.SystemPrivateCoreLib_v8_0_0_0)!;

            var intType = corLib.ManifestModule!.TopLevelTypes.First(t => t.Name == "Int32");
            var spanType = corLib.ManifestModule.TopLevelTypes.First(t => t.Name == "Span`1");

            Assert.False(intType.IsByRefLike);
            Assert.True(spanType.IsByRefLike);
        }

        [Fact]
        public void GetStaticConstructor()
        {
            var module = ModuleDefinition.FromFile(typeof(Constructors).Assembly.Location, TestReaderParameters);

            var type1 = module.LookupMember<TypeDefinition>(typeof(Constructors).MetadataToken);
            var cctor = type1.GetStaticConstructor();
            Assert.NotNull(cctor);
            Assert.True(cctor.IsStatic);
            Assert.True(cctor.IsConstructor);

            var type2 = module.LookupMember<TypeDefinition>(typeof(NoStaticConstructor).MetadataToken);
            Assert.Null(type2.GetStaticConstructor());
        }

        [Fact]
        public void GetOrCreateStaticConstructor()
        {
            var module = ModuleDefinition.FromFile(typeof(Constructors).Assembly.Location, TestReaderParameters);
            var type1 = module.LookupMember<TypeDefinition>(typeof(Constructors).MetadataToken);

            // If cctor already exists, we expect this to be returned.
            var cctor = type1.GetStaticConstructor();
            Assert.NotNull(cctor);
            Assert.Same(cctor, type1.GetOrCreateStaticConstructor());

            var type2 = module.LookupMember<TypeDefinition>(typeof(NoStaticConstructor).MetadataToken);
            Assert.Null(type2.GetStaticConstructor());

            // If cctor doesn't exist yet, it should be added.
            cctor = type2.GetOrCreateStaticConstructor();
            Assert.NotNull(cctor);
            Assert.Same(type2, cctor.DeclaringType);
            Assert.Same(cctor, type2.GetOrCreateStaticConstructor());
            Assert.True(cctor.IsStatic);
            Assert.True(cctor.IsConstructor);
        }

        [Fact]
        public void GetParameterlessConstructor()
        {
            var module = ModuleDefinition.FromFile(typeof(Constructors).Assembly.Location, TestReaderParameters);
            var type = module.LookupMember<TypeDefinition>(typeof(Constructors).MetadataToken);

            var ctor = type.GetConstructor();
            Assert.NotNull(ctor);
            Assert.False(ctor.IsStatic);
            Assert.True(ctor.IsConstructor);
            Assert.Empty(ctor.Parameters);
        }

        [Theory]
        [InlineData(new[] {ElementType.I4, ElementType.I4})]
        [InlineData(new[] {ElementType.I4, ElementType.String})]
        [InlineData(new[] {ElementType.I4, ElementType.String, ElementType.R8})]
        public void GetParametersConstructor(ElementType[] types)
        {
            var module = ModuleDefinition.FromFile(typeof(Constructors).Assembly.Location, TestReaderParameters);
            var type = module.LookupMember<TypeDefinition>(typeof(Constructors).MetadataToken);

            var signatures = types.Select(x => (TypeSignature) module.CorLibTypeFactory.FromElementType(x)).ToArray();
            var ctor = type.GetConstructor(signatures);
            Assert.NotNull(ctor);
            Assert.False(ctor.IsStatic);
            Assert.True(ctor.IsConstructor);
            Assert.Equal(signatures, ctor.Signature!.ParameterTypes);
        }

        [Fact]
        public void GetNonExistingConstructorShouldReturnNull()
        {
            var module = ModuleDefinition.FromFile(typeof(Constructors).Assembly.Location, TestReaderParameters);
            var type = module.LookupMember<TypeDefinition>(typeof(Constructors).MetadataToken);
            var factory = module.CorLibTypeFactory;

            Assert.Null(type.GetConstructor(factory.String));
            Assert.Null(type.GetConstructor(factory.String, factory.String));
        }

        [Fact]
        public void SystemEnumShouldNotBeValueType()
        {
            var module = ModuleDefinition.FromFile(typeof(Enum).Assembly.Location, TestReaderParameters);
            var type = module.LookupMember<TypeDefinition>(typeof(Enum).MetadataToken);

            Assert.False(type.IsValueType);
        }

        [Fact]
        public void AddTypeToModuleShouldSetOwner()
        {
            var module = new ModuleDefinition("Dummy");
            var type = new TypeDefinition("SomeNamespace", "SomeType", TypeAttributes.Public);
            module.TopLevelTypes.Add(type);
            Assert.Same(module, type.DeclaringModule);
        }

        [Fact]
        public void AddNestedTypeToModuleShouldSetOwner()
        {
            var module = new ModuleDefinition("Dummy");
            var type1 = new TypeDefinition("SomeNamespace", "SomeType", TypeAttributes.Public);
            var type2 = new TypeDefinition(null, "NestedType", TypeAttributes.NestedPublic);
            module.TopLevelTypes.Add(type1);
            type1.NestedTypes.Add(type2);
            Assert.Same(type1, type2.DeclaringType);
            Assert.Same(module, type2.DeclaringModule);
        }

        [Fact]
        public void AddSameTypeTwiceToModuleShouldThrow()
        {
            var module = new ModuleDefinition("Dummy");
            var type = new TypeDefinition("SomeNamespace", "SomeType", TypeAttributes.Public);
            module.TopLevelTypes.Add(type);
            Assert.Throws<ArgumentException>(() => module.TopLevelTypes.Add(type));
        }

        [Fact]
        public void AddSameTypeTwiceToNestedTypeShouldThrow()
        {
            var type = new TypeDefinition("SomeNamespace", "SomeType", TypeAttributes.Public);
            var nestedType = new TypeDefinition(null, "SomeType", TypeAttributes.NestedPublic);
            type.NestedTypes.Add(nestedType);
            Assert.Throws<ArgumentException>(() => type.NestedTypes.Add(nestedType));
        }

        [Fact]
        public void AddSameNestedTypeToDifferentTypesShouldThrow()
        {
            var type1 = new TypeDefinition("SomeNamespace", "SomeType1", TypeAttributes.Public);
            var type2 = new TypeDefinition("SomeNamespace", "SomeType2", TypeAttributes.Public);
            var nestedType = new TypeDefinition(null, "SomeType", TypeAttributes.NestedPublic);
            type1.NestedTypes.Add(nestedType);
            Assert.Throws<ArgumentException>(() => type2.NestedTypes.Add(nestedType));
        }

        [Fact]
        public void NamespaceShouldBeNullIfEmptyStringGiven()
        {
            var type = new TypeDefinition(string.Empty, "SomeType", TypeAttributes.Public);
            Assert.Null(type.Namespace);
        }

        [Fact]
        public void NamespaceShouldBeNullIfEmptyStringSet()
        {
            var type = new TypeDefinition("SomeNamespace", "SomeType", TypeAttributes.Public);
            type.Namespace = string.Empty;
            Assert.Null(type.Namespace);
        }

        [Fact]
        public void NestedTypeRemovedFromOwnerShouldHaveNoModule()
        {
            var module = ModuleDefinition.FromFile(typeof(TopLevelClass1).Assembly.Location, TestReaderParameters);
            var nestedType = module.LookupMember<TypeDefinition>(typeof(TopLevelClass1.Nested1).MetadataToken);

            nestedType.DeclaringType!.NestedTypes.Remove(nestedType);

            Assert.Null(nestedType.DeclaringModule);
        }

        [Fact]
        public void ExternalTypeDefAsBaseTypeShouldAutoConvertToTypeRef()
        {
            var sourceModule = ModuleDefinition.FromFile(typeof(TopLevelClass1).Assembly.Location, TestReaderParameters);
            var sourceType = sourceModule.LookupMember<TypeDefinition>(typeof(TopLevelClass1).MetadataToken);

            var targetModule = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var type = new TypeDefinition("Namespace", "Name", TypeAttributes.Class, sourceType);
            targetModule.TopLevelTypes.Add(type);

            var newType = RebuildAndLookup(type);

            Assert.IsAssignableFrom<TypeReference>(newType.BaseType);
            Assert.Equal(type.BaseType, newType.BaseType, Comparer);
        }
    }
}
