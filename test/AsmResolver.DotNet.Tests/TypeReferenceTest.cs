using System;
using System.Linq;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class TypeReferenceTest
    {
        private static readonly SignatureComparer Comparer = new();

        [Fact]
        public void ReadAssemblyRefScope()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var typeRef = module.LookupMember<TypeReference>(new MetadataToken(TableIndex.TypeRef, 13));

            var scope = Assert.IsAssignableFrom<AssemblyReference>(typeRef.Scope);
            Assert.Equal("mscorlib", scope.Name);
        }

        [Fact]
        public void WriteAssemblyRefScope()
        {
            var module = new ModuleDefinition("SomeModule");
            module.GetOrCreateModuleType().Fields.Add(new FieldDefinition(
                "SomeField",
                FieldAttributes.Static,
                new TypeDefOrRefSignature(new TypeReference(
                    new AssemblyReference("SomeAssembly", new Version(1, 0, 0, 0)),
                    "SomeNamespace",
                    "SomeName")
                ).ImportWith(module.DefaultImporter)
            ));

            var image = module.ToPEImage();

            var newModule = ModuleDefinition.FromImage(image, TestReaderParameters);
            var typeRef = newModule.GetOrCreateModuleType().Fields.First().Signature!.FieldType.GetUnderlyingTypeDefOrRef()!;

            var scope = Assert.IsAssignableFrom<AssemblyReference>(typeRef.Scope);
            Assert.Equal("SomeAssembly", scope.Name);
        }

        [Fact]
        public void ReadTypeRefScope()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var typeRef = module.LookupMember<TypeReference>(new MetadataToken(TableIndex.TypeRef, 4));

            var scope = Assert.IsAssignableFrom<TypeReference>(typeRef.Scope);
            Assert.Equal("DebuggableAttribute", scope.Name);
        }

        [Fact]
        public void WriteTypeRefScope()
        {
            var module = new ModuleDefinition("SomeModule");
            module.GetOrCreateModuleType().Fields.Add(new FieldDefinition(
                "SomeField",
                FieldAttributes.Static,
                new TypeDefOrRefSignature(new TypeReference(
                    new TypeReference(
                        new AssemblyReference("SomeAssembly", new Version(1, 0, 0, 0)),
                        "SomeNamespace",
                        "SomeName"),
                    null,
                    "SomeNestedType"
                )).ImportWith(module.DefaultImporter)
            ));

            var image = module.ToPEImage();

            var newModule = ModuleDefinition.FromImage(image, TestReaderParameters);
            var typeRef = newModule.GetOrCreateModuleType().Fields.First().Signature!.FieldType.GetUnderlyingTypeDefOrRef()!;

            var scope = Assert.IsAssignableFrom<TypeReference>(typeRef.Scope);
            Assert.Equal("SomeName", scope.Name);
        }

        [Fact]
        public void ReadModuleScope()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.TypeRefModuleScope, TestReaderParameters);
            var typeRef = module.LookupMember<TypeReference>(new MetadataToken(TableIndex.TypeRef, 2));

            var scope = Assert.IsAssignableFrom<ModuleDefinition>(typeRef.Scope);
            Assert.Same(module, scope);
        }

        [Fact]
        public void WriteModuleScope()
        {
            var module = new ModuleDefinition("SomeModule");
            module.GetOrCreateModuleType().Fields.Add(new FieldDefinition(
                "SomeField",
                FieldAttributes.Static,
                new TypeDefOrRefSignature(new TypeReference(
                    module,
                    "SomeNamepace",
                    "SomeName")
                ).ImportWith(module.DefaultImporter)
            ));

            var image = module.ToPEImage();

            var newModule = ModuleDefinition.FromImage(image, TestReaderParameters);
            var typeRef = newModule.GetOrCreateModuleType().Fields.First().Signature!.FieldType.GetUnderlyingTypeDefOrRef()!;

            var scope = Assert.IsAssignableFrom<ModuleDefinition>(typeRef.Scope);
            Assert.Equal(module.Name, scope.Name);
        }

        [Fact]
        public void WriteNullScope()
        {
            var module = new ModuleDefinition("SomeModule");
            module.GetOrCreateModuleType().Fields.Add(new FieldDefinition(
                "SomeField",
                FieldAttributes.Static,
                new TypeDefOrRefSignature(new TypeReference(
                    null,
                    "SomeNamespace",
                    "SomeName")
                ).ImportWith(module.DefaultImporter)
            ));

            var image = module.ToPEImage();

            var newModule = ModuleDefinition.FromImage(image, TestReaderParameters);
            var typeRef = newModule.GetOrCreateModuleType().Fields.First().Signature!.FieldType.GetUnderlyingTypeDefOrRef()!;

            Assert.Null(typeRef.Scope);
        }

        [Fact]
        public void ReadNullScopeCurrentModule()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.TypeRefNullScope_CurrentModule, TestReaderParameters);
            var typeRef = module.LookupMember<TypeReference>(new MetadataToken(TableIndex.TypeRef, 2));

            Assert.Null(typeRef.Scope);
        }

        [Fact]
        public void ReadNullScopeExportedType()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.TypeRefNullScope_ExportedType, TestReaderParameters);
            var typeRef = module.LookupMember<TypeReference>(new MetadataToken(TableIndex.TypeRef, 1));

            Assert.Null(typeRef.Scope);
        }

        [Fact]
        public void ReadName()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var typeRef = module.LookupMember<TypeReference>(new MetadataToken(TableIndex.TypeRef, 13));

            Assert.Equal("Console", typeRef.Name);
        }

        [Fact]
        public void ReadNamespace()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var typeRef = module.LookupMember<TypeReference>(new MetadataToken(TableIndex.TypeRef, 13));

            Assert.Equal("System", typeRef.Namespace);
        }

        [Fact]
        public void CorLibTypeToTypeSignatureShouldReturnCorLibTypeSignature()
        {
            var module = new ModuleDefinition("SomeModule");
            var reference = new TypeReference(module, module.CorLibTypeFactory.CorLibScope, "System", "Object");
            var signature = Assert.IsAssignableFrom<CorLibTypeSignature>(reference.ToTypeSignature());

            Assert.Equal(ElementType.Object, signature.ElementType);
        }

        [Fact]
        public void NonCorLibTypeToTypeSignatureShouldReturnTypeDefOrRef()
        {
            var module = new ModuleDefinition("SomeModule");
            var reference = new TypeReference(module, module.CorLibTypeFactory.CorLibScope, "System", "Array");
            var signature = Assert.IsAssignableFrom<TypeDefOrRefSignature>(reference.ToTypeSignature());

            Assert.Equal(signature.Type, reference, Comparer);
        }
    }
}
