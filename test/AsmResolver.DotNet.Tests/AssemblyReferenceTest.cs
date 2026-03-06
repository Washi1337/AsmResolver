using System;
using System.IO;
using System.Reflection;
using AsmResolver.DotNet.Signatures;
using Xunit;
using FieldAttributes = AsmResolver.PE.DotNet.Metadata.Tables.FieldAttributes;

namespace AsmResolver.DotNet.Tests
{
    public class AssemblyReferenceTest
    {
        [Fact]
        public void ReadName()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var assemblyRef = module.AssemblyReferences[0];
            Assert.Equal("mscorlib", assemblyRef.Name);
        }

        [Fact]
        public void ReadVersion()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var assemblyRef = module.AssemblyReferences[0];
            Assert.Equal(new Version(4,0,0,0), assemblyRef.Version);
        }

        [Fact]
        public void ReadPublicKeyOrToken()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var assemblyRef = module.AssemblyReferences[0];
            Assert.False(assemblyRef.HasPublicKey);
            var expectedToken = new byte[] {0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89};
            Assert.Equal(expectedToken, assemblyRef.PublicKeyOrToken);
            Assert.Equal(expectedToken, assemblyRef.GetPublicKeyToken());
        }

        [Fact]
        public void GetFullNameCultureNeutralAssembly()
        {
            var name = new AssemblyName(KnownCorLibs.MsCorLib_v4_0_0_0.FullName);
            Assert.Equal("mscorlib", name.Name);
            Assert.Equal(new Version(4,0,0,0), name.Version);
            Assert.Equal(string.Empty, name.CultureName);
            Assert.Equal(new byte[] {0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89}, name.GetPublicKeyToken());
        }

        [Fact]
        public void GetFullNameCultureNonNeutralAssembly()
        {
            var assembly = new AssemblyReference("SomeAssembly", new Version(1, 2, 3, 4));
            assembly.Culture = "en-US";

            var name = new AssemblyName(assembly.FullName);
            Assert.Equal("SomeAssembly", name.Name);
            Assert.Equal(new Version(1,2,3,4), name.Version);
            Assert.Equal("en-US", name.CultureName);
            Assert.Equal(Array.Empty<byte>(), name.GetPublicKeyToken());
        }

        [Fact]
        public void CorLibResolution()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var assemblyRef = module.AssemblyReferences[0];
            var assemblyDef = assemblyRef.Resolve(module.RuntimeContext);
            Assert.Equal(assemblyDef.Name, assemblyDef.Name);
            Assert.Equal(assemblyDef.Version, assemblyDef.Version);
        }

        [Fact]
        public void CreateTypeReferenceFromImportedAssemblyShouldBeImported()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var systemConsole = new AssemblyReference("System.Console", new Version(8, 0, 0, 0));
            module.DefaultImporter.ImportScope(systemConsole);

            var someAssembly = new AssemblyReference("SomeAssembly", new Version(1, 2, 3, 4));
            module.AssemblyReferences.Add(someAssembly);

            var reference = someAssembly.CreateTypeReference("Namespace", "Type");
            Assert.True(reference.IsImportedInModule(module));
        }

        [Fact]
        public void CreateTypeReferenceFromNonImportedAssemblyShouldNotBeImported()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var someAssembly = new AssemblyReference("SomeAssembly", new Version(1, 2, 3, 4));

            var reference = someAssembly.CreateTypeReference("Namespace", "Type");
            Assert.False(reference.IsImportedInModule(module));
        }

        [Fact]
        public void CreateClassGenericType()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var genericType = module.CorLibTypeFactory.CorLibScope
                .CreateTypeReference("System.Collections.Generic", "List`1")
                .MakeGenericInstanceType(isValueType: false, [module.CorLibTypeFactory.Int32]);
            Assert.False(genericType.IsValueType);
        }

        [Fact]
        public void CreateValueTypeGenericType()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var genericType = module.CorLibTypeFactory.CorLibScope
                .CreateTypeReference("System", "Nullable`1")
                .MakeGenericInstanceType(isValueType: true, [module.CorLibTypeFactory.Int32]);
            Assert.True(genericType.IsValueType);
        }

        [Fact]
        public void AddReferenceTwiceShouldAddCustomAttributesOnce()
        {
            // https://github.com/Washi1337/AsmResolver/pull/697

            // Prepare module.
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);

            // Add some assembly ref with a CA.
            var reference = new AssemblyReference("Foo", new Version(1, 0, 0, 0));
            var ctor = module.CorLibTypeFactory.CorLibScope
                .CreateTypeReference("System", "ObsoleteAttribute")
                .CreateMemberReference(".ctor", MethodSignature.CreateInstance(module.CorLibTypeFactory.Void));
            reference.CustomAttributes.Add(new CustomAttribute(ctor));

            // Reference the assembly twice.
            var moduleType = module.GetOrCreateModuleType();
            var type = reference.CreateTypeReference("SomeNamespace", "SomeType").ToTypeSignature(false);
            moduleType.Fields.Add(new FieldDefinition("Foo1", FieldAttributes.Static, type));
            moduleType.Fields.Add(new FieldDefinition("Foo2", FieldAttributes.Static, type));

            // Rebuild.
            using var stream = new MemoryStream();
            module.Write(stream);

            // Verify the assembly ref still has only one CA.
            var newModule =  ModuleDefinition.FromBytes(stream.ToArray());
            var newReference = Assert.Single(newModule.AssemblyReferences, x => x.Name == "Foo");
            var newAttribute = Assert.Single(newReference.CustomAttributes);
            Assert.Equal(ctor, newAttribute.Constructor, SignatureComparer.Default!);
        }

        [Fact]
        public void AllKnownCorLibsAreCorLib()
        {
            Assert.All(KnownCorLibs.KnownCorLibReferences,
                reference => reference.IsCorLib(DotNetRuntimeInfo.NetFramework(4, 0)));
            Assert.All(KnownCorLibs.KnownCorLibReferences,
                reference => reference.IsCorLib(DotNetRuntimeInfo.NetCoreApp(10, 0)));
            Assert.All(KnownCorLibs.KnownCorLibReferences,
                reference => reference.IsCorLib(DotNetRuntimeInfo.NetStandard(2, 0)));
        }

        [Fact]
        public void MscorlibClassification()
        {
            Assert.True(KnownCorLibs.MsCorLib_v4_0_0_0.IsReferenceCorLib(DotNetRuntimeInfo.NetFramework(4, 0)));
            Assert.True(KnownCorLibs.MsCorLib_v4_0_0_0.IsImplementationCorLib(DotNetRuntimeInfo.NetFramework(4, 0)));
            Assert.True(KnownCorLibs.MsCorLib_v4_0_0_0.IsReferenceCorLib(DotNetRuntimeInfo.NetCoreApp(10, 0)));
            Assert.False(KnownCorLibs.MsCorLib_v4_0_0_0.IsImplementationCorLib(DotNetRuntimeInfo.NetCoreApp(10, 0)));
            Assert.True(KnownCorLibs.MsCorLib_v4_0_0_0.IsReferenceCorLib(DotNetRuntimeInfo.NetStandard(2, 0)));
            Assert.False(KnownCorLibs.MsCorLib_v4_0_0_0.IsImplementationCorLib(DotNetRuntimeInfo.NetStandard(2, 0)));
        }

        [Fact]
        public void NetStandardClassification()
        {
            Assert.All(
                [
                    DotNetRuntimeInfo.NetFramework(4, 0),
                    DotNetRuntimeInfo.NetStandard(2, 0),
                    DotNetRuntimeInfo.NetCoreApp(10, 0),
                ],
                info =>
                {
                    Assert.True(KnownCorLibs.NetStandard_v2_0_0_0.IsReferenceCorLib(info));
                    Assert.False(KnownCorLibs.NetStandard_v2_0_0_0.IsImplementationCorLib(info));
                }
            );
        }

        [Fact]
        public void SystemRuntimeClassification()
        {
            Assert.False(KnownCorLibs.SystemRuntime_v10_0_0_0.IsReferenceCorLib(DotNetRuntimeInfo.NetFramework(4, 0)));
            Assert.False(KnownCorLibs.SystemRuntime_v10_0_0_0.IsImplementationCorLib(DotNetRuntimeInfo.NetFramework(4, 0)));
            Assert.True(KnownCorLibs.SystemRuntime_v10_0_0_0.IsReferenceCorLib(DotNetRuntimeInfo.NetCoreApp(10, 0)));
            Assert.False(KnownCorLibs.SystemRuntime_v10_0_0_0.IsImplementationCorLib(DotNetRuntimeInfo.NetCoreApp(10, 0)));
            Assert.True(KnownCorLibs.SystemRuntime_v10_0_0_0.IsReferenceCorLib(DotNetRuntimeInfo.NetStandard(2, 0)));
            Assert.False(KnownCorLibs.SystemRuntime_v10_0_0_0.IsImplementationCorLib(DotNetRuntimeInfo.NetStandard(2, 0)));
        }

        [Fact]
        public void SystemPrivateCorLibClassification()
        {
            Assert.False(KnownCorLibs.SystemPrivateCoreLib_v10_0_0_0.IsReferenceCorLib(DotNetRuntimeInfo.NetFramework(4, 0)));
            Assert.False(KnownCorLibs.SystemPrivateCoreLib_v10_0_0_0.IsImplementationCorLib(DotNetRuntimeInfo.NetFramework(4, 0)));
            Assert.False(KnownCorLibs.SystemPrivateCoreLib_v10_0_0_0.IsReferenceCorLib(DotNetRuntimeInfo.NetCoreApp(10, 0)));
            Assert.True(KnownCorLibs.SystemPrivateCoreLib_v10_0_0_0.IsImplementationCorLib(DotNetRuntimeInfo.NetCoreApp(10, 0)));
            Assert.False(KnownCorLibs.SystemPrivateCoreLib_v10_0_0_0.IsReferenceCorLib(DotNetRuntimeInfo.NetStandard(2, 0)));
            Assert.False(KnownCorLibs.SystemPrivateCoreLib_v10_0_0_0.IsImplementationCorLib(DotNetRuntimeInfo.NetStandard(2, 0)));
        }
    }
}
