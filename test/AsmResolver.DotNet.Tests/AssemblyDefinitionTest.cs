using System;
using System.IO;
using System.Security.Cryptography;
using AsmResolver.PE.DotNet.StrongName;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class AssemblyDefinitionTest
    {
        private static AssemblyDefinition Rebuild(AssemblyDefinition assembly)
        {
            using var stream = new MemoryStream();
            assembly.ManifestModule.Write(stream);
            return AssemblyDefinition.FromReader(new ByteArrayReader(stream.ToArray()));
        }

        [Fact]
        public void ReadNameTest()
        {
            var assemblyDef = AssemblyDefinition.FromBytes(Properties.Resources.HelloWorld);
            Assert.Equal("HelloWorld", assemblyDef.Name);
        }

        [Fact]
        public void NameIsPersistentAfterRebuild()
        {
            const string newName = "OtherAssembly";
            
            var assemblyDef = AssemblyDefinition.FromBytes(Properties.Resources.HelloWorld);
            assemblyDef.Name = newName;

            var rebuilt = Rebuild(assemblyDef);
            Assert.Equal(newName, rebuilt.Name);
        }

        [Fact]
        public void ReadVersion()
        {
            var assemblyDef = AssemblyDefinition.FromBytes(Properties.Resources.HelloWorld);
            Assert.Equal(new Version(1,0,0,0), assemblyDef.Version);
        }

        [Fact]
        public void VersionIsPersistentAfterRebuild()
        {
            var newVersion = new Version(1,2,3,4);
            
            var assemblyDef = AssemblyDefinition.FromBytes(Properties.Resources.HelloWorld);
            assemblyDef.Version = newVersion;
            
            var rebuilt = Rebuild(assemblyDef);
            Assert.Equal(newVersion, rebuilt.Version);
        }

        [Fact]
        public void ReadSingleModuleAssembly()
        {
            var assemblyDef = AssemblyDefinition.FromBytes(Properties.Resources.HelloWorld);
            Assert.Single(assemblyDef.Modules);
            Assert.NotNull(assemblyDef.ManifestModule);
            Assert.Equal(new[] {assemblyDef.ManifestModule}, assemblyDef.Modules);
            Assert.Same(assemblyDef, assemblyDef.ManifestModule.Assembly);
        }

        [Fact]
        public void ReadMultiModuleAssembly()
        {
            var assemblyDef = AssemblyDefinition.FromFile(Path.Combine("Resources", "Manifest.exe"));
            Assert.Equal(2, assemblyDef.Modules.Count);
            Assert.Equal("Manifest.exe", assemblyDef.ManifestModule.Name);
            Assert.Equal("MyModel.netmodule", assemblyDef.Modules[1].Name);
        }

        [Fact]
        public void ReadSecondaryModuleAsAssemblyShouldThrow()
        {
            Assert.Throws<BadImageFormatException>(() =>
                AssemblyDefinition.FromFile(Path.Combine("Resources", "MyModel.netmodule")));
        }
        
        [Fact]
        public void ReadPublicKeyToken()
        {
            var corlibAssembly = typeof(object).Assembly;
            var corlibName = corlibAssembly.GetName();
            var corlibAssemblyDef = AssemblyDefinition.FromFile(corlibAssembly.Location);

            Assert.Equal(corlibName.GetPublicKey(), corlibAssemblyDef.PublicKey);
            Assert.Equal(corlibName.GetPublicKeyToken(), corlibAssemblyDef.GetPublicKeyToken());
        }

        [Fact]
        public void PublicKeyIsPersistentAfterRebuild()
        {
            using var rsa = RSA.Create();
            var rsaParameters = rsa.ExportParameters(true);
            var snk = new StrongNamePrivateKey(rsaParameters);
            
            var assemblyDef = AssemblyDefinition.FromBytes(Properties.Resources.HelloWorld);
            assemblyDef.PublicKey = snk.CreatePublicKeyBlob(assemblyDef.HashAlgorithm);
            
            var rebuilt = Rebuild(assemblyDef);
            Assert.Equal(assemblyDef.PublicKey, rebuilt.PublicKey);
        }
    }
}