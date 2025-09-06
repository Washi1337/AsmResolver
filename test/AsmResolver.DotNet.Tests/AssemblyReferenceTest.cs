using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Xunit;

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
            var assemblyDef = assemblyRef.Resolve();
            Assert.NotNull(assemblyDef);
            Assert.Equal(assemblyDef.Name, assemblyDef.Name);
            Assert.Equal(assemblyDef.Version, assemblyDef.Version);
        }

        [Fact]
        public void CreateTypeReferenceFromImportedAssemblyShouldBeImported()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var someAssembly = new AssemblyReference("SomeAssembly", new Version(1, 2, 3, 4));
            module.AssemblyReferences.Add(someAssembly);

            var reference = someAssembly.CreateTypeReference("Namespace", "Type");
            Assert.True(reference.IsImportedInModule(module));
        }

        [Fact]
        public void CreateTypeReferenceFromImportedAssemblyShouldNotBeImported()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld, TestReaderParameters);
            var someAssembly = new AssemblyReference("SomeAssembly", new Version(1, 2, 3, 4));

            var reference = someAssembly.CreateTypeReference("Namespace", "Type");
            Assert.False(reference.IsImportedInModule(module));
        }
    }
}
