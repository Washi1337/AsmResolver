using System;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Xunit;

namespace AsmResolver.DotNet.Tests.Signatures
{
    public class SignatureComparerTest
    {
        private readonly SignatureComparer _comparer;

        private readonly AssemblyReference _someAssemblyReference =
            new AssemblyReference("SomeAssembly", new Version(1, 2, 3, 4));

        public SignatureComparerTest()
        {
            _comparer = new SignatureComparer();
        }

        [Fact]
        public void MatchCorLibTypeSignatures()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            Assert.Equal(module.CorLibTypeFactory.Boolean, module.CorLibTypeFactory.Boolean, _comparer);
        }

        [Fact]
        public void MatchDifferentCorLibTypeSignatures()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            Assert.NotEqual(module.CorLibTypeFactory.Byte, module.CorLibTypeFactory.Boolean, _comparer);
        }
        
        [Fact]
        public void MatchTopLevelTypeRefTypeRef()
        {
            var reference1 = new TypeReference(_someAssemblyReference, "SomeNamespace", "SomeType");
            var reference2 = new TypeReference(_someAssemblyReference, "SomeNamespace", "SomeType");
            Assert.Equal(reference1, reference2, _comparer);
        }

        [Fact]
        public void MatchTopLevelTypeRefTypeRefDifferentName()
        {
            var reference1 = new TypeReference(_someAssemblyReference, "SomeNamespace", "SomeType");
            var reference2 = new TypeReference(_someAssemblyReference, "SomeNamespace", "SomeOtherType");
            Assert.NotEqual(reference1, reference2, _comparer);
        }

        [Fact]
        public void MatchTopLevelTypeRefTypeRefDifferentNamespace()
        {
            var reference1 = new TypeReference(_someAssemblyReference, "SomeNamespace", "SomeType");
            var reference2 = new TypeReference(_someAssemblyReference, "SomeOtherNamespace", "SomeType");
            Assert.NotEqual(reference1, reference2, _comparer);
        }

        [Fact]
        public void MatchTopLevelTypeRefTypeDef()
        {
            var assembly = new AssemblyDefinition(_someAssemblyReference.Name, _someAssemblyReference.Version);
            var module = new ModuleDefinition(assembly.Name + ".dll");
            assembly.Modules.Add(module);
            
            var definition = new TypeDefinition("SomeNamespace", "SomeType", TypeAttributes.Public);
            module.TopLevelTypes.Add(definition);
            
            var reference = new TypeReference(_someAssemblyReference, "SomeNamespace", "SomeType");
            Assert.Equal((ITypeDefOrRef) definition, reference, _comparer);
        }

        [Fact]
        public void MatchTopLevelTypeRefTypeDefDifferentScope()
        {
            var assembly = new AssemblyDefinition(_someAssemblyReference.Name + "2", _someAssemblyReference.Version);
            var module = new ModuleDefinition(assembly.Name + ".dll");
            assembly.Modules.Add(module);
            
            var definition = new TypeDefinition("SomeNamespace", "SomeType", TypeAttributes.Public);
            module.TopLevelTypes.Add(definition);
            
            var reference = new TypeReference(_someAssemblyReference, "SomeNamespace", "SomeType");
            Assert.NotEqual((ITypeDefOrRef) definition, reference, _comparer);
        }

        [Fact]
        public void MatchTypeDefOrRefSignatures()
        {
            var reference = new TypeReference(_someAssemblyReference, "SomeNamespace", "SomeType");
            var typeSig1 = new TypeDefOrRefSignature(reference);
            var typeSig2 = new TypeDefOrRefSignature(reference);

            Assert.Equal(typeSig1, typeSig2, _comparer);
        }

        [Fact]
        public void MatchTypeDefOrRefSignaturesDifferentClass()
        {
            var reference1 = new TypeReference(_someAssemblyReference, "SomeNamespace", "SomeType");
            var reference2 = new TypeReference(_someAssemblyReference, "SomeNamespace", "SomeOtherType");
            var typeSig1 = new TypeDefOrRefSignature(reference1);
            var typeSig2 = new TypeDefOrRefSignature(reference2);

            Assert.NotEqual(typeSig1, typeSig2, _comparer);
        }

        [Fact]
        public void MatchPropertySignature()
        {
            var type = new TypeReference(_someAssemblyReference, "SomeNamespace", "SomeType");
            var signature1 = PropertySignature.CreateStatic(type.ToTypeSignature());
            var signature2 = PropertySignature.CreateStatic(type.ToTypeSignature());

            Assert.Equal(signature1, signature2, _comparer);
        }
    }
}