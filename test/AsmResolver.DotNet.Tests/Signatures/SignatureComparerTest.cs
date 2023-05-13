using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
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

        [Fact]
        public void NestedTypesWithSameNameButDifferentDeclaringTypeShouldNotMatch()
        {
            var nestedTypes = ModuleDefinition.FromFile(typeof(SignatureComparerTest).Assembly.Location)
                .GetAllTypes().First(t => t.Name == nameof(SignatureComparerTest))
                .NestedTypes.First(t => t.Name == nameof(NestedTypes));

            var firstType = nestedTypes.NestedTypes
                .First(t => t.Name == nameof(NestedTypes.FirstType)).NestedTypes
                .First(t => t.Name == nameof(NestedTypes.FirstType.TypeWithCommonName));
            var secondType = nestedTypes.NestedTypes
                .First(t => t.Name == nameof(NestedTypes.SecondType)).NestedTypes
                .First(t => t.Name == nameof(NestedTypes.SecondType.TypeWithCommonName));

            Assert.NotEqual(firstType, secondType, _comparer);
        }

        [Fact]
        public void MatchForwardedNestedTypes()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.ForwarderRefTest);
            var forwarder = ModuleDefinition.FromBytes(Properties.Resources.ForwarderLibrary).Assembly!;
            var library = ModuleDefinition.FromBytes(Properties.Resources.ActualLibrary).Assembly!;

            module.MetadataResolver.AssemblyResolver.AddToCache(forwarder, forwarder);
            module.MetadataResolver.AssemblyResolver.AddToCache(library, library);
            forwarder.ManifestModule!.MetadataResolver.AssemblyResolver.AddToCache(library, library);

            var referencedTypes = module.ManagedEntryPointMethod!.CilMethodBody!.Instructions
                .Where(i => i.OpCode.Code == CilCode.Call)
                .Select(i => ((IMethodDefOrRef) i.Operand!).DeclaringType)
                .Where(t => t.Name == "MyNestedClass")
                .ToArray();

            var type1 = referencedTypes[0]!;
            var type2 = referencedTypes[1]!;

            var resolvedType1 = type1.Resolve();
            var resolvedType2 = type2.Resolve();

            Assert.Equal(type1, resolvedType1, _comparer);
            Assert.Equal(type2, resolvedType2, _comparer);

            Assert.NotEqual(type1, type2, _comparer);
            Assert.NotEqual(type1, resolvedType2, _comparer); // Fails
            Assert.NotEqual(type2, resolvedType1, _comparer); // Fails
        }

        [Fact]
        public void AssemblyHashCodeStrict()
        {
            var assembly1 = new AssemblyReference("SomeAssembly", new Version(1, 2, 3, 4));
            var assembly2 = new AssemblyReference("SomeAssembly", new Version(1, 2, 3, 4));

            Assert.Equal(
                _comparer.GetHashCode((AssemblyDescriptor) assembly1),
                _comparer.GetHashCode((AssemblyDescriptor) assembly2));
        }

        [Fact]
        public void AssemblyHashCodeVersionAgnostic()
        {
            var assembly1 = new AssemblyReference("SomeAssembly", new Version(1, 2, 3, 4));
            var assembly2 = new AssemblyReference("SomeAssembly", new Version(5, 6, 7, 8));

            var comparer = new SignatureComparer(SignatureComparisonFlags.VersionAgnostic);
            Assert.Equal(
                comparer.GetHashCode((AssemblyDescriptor) assembly1),
                comparer.GetHashCode((AssemblyDescriptor) assembly2));
        }

        [Fact]
        public void CorlibComparison()
        {
            // https://github.com/Washi1337/AsmResolver/issues/427

            var comparer = new SignatureComparer(SignatureComparisonFlags.VersionAgnostic);

            var reference1 = KnownCorLibs.SystemRuntime_v5_0_0_0;
            var reference2 = KnownCorLibs.SystemRuntime_v6_0_0_0;
            Assert.Equal(reference1, reference2, comparer);

            var set = new HashSet<AssemblyReference>(comparer);
            Assert.True(set.Add(reference1));
            Assert.False(set.Add(reference2));
        }

        [Fact]
        public void CompareSimpleTypeDescriptors()
        {
            var assembly = new DotNetFrameworkAssemblyResolver().Resolve(KnownCorLibs.MsCorLib_v4_0_0_0);
            var definition = assembly.ManifestModule!.TopLevelTypes.First(x => x.IsTypeOf("System.IO", "Stream"));
            var reference = definition.ToTypeReference();
            var signature = reference.ToTypeSignature();

            Assert.Equal((ITypeDescriptor) reference, signature, _comparer);
            Assert.Equal((ITypeDescriptor) definition, signature, _comparer);
            Assert.Equal(_comparer.GetHashCode(reference), _comparer.GetHashCode(signature));
            Assert.Equal(_comparer.GetHashCode(definition), _comparer.GetHashCode(signature));
        }

        private class NestedTypes
        {
            public class FirstType
            {
                public class TypeWithCommonName
                {
                }

            }
            public class SecondType
            {
                public class TypeWithCommonName
                {
                }
            }
        }
    }
}
