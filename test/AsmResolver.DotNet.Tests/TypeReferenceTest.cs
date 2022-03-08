using System;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class TypeReferenceTest
    {
        private static readonly SignatureComparer Comparer = new();

        [Fact]
        public void ReadAssemblyRefScope()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var typeRef = (TypeReference) module.LookupMember(new MetadataToken(TableIndex.TypeRef, 13));
            Assert.Equal("mscorlib", typeRef.Scope.Name);
        }

        [Fact]
        public void ReadName()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var typeRef = (TypeReference) module.LookupMember(new MetadataToken(TableIndex.TypeRef, 13));
            Assert.Equal("Console", typeRef.Name);
        }

        [Fact]
        public void ReadNamespace()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var typeRef = (TypeReference) module.LookupMember(new MetadataToken(TableIndex.TypeRef, 13));
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
