using System;
using System.IO;
using System.Runtime.InteropServices;
using AsmResolver.DotNet.Config.Json;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.TestCases.NestedClasses;
using AsmResolver.IO;
using AsmResolver.PE.File;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class AssemblyResolverTest
    {
        private const string NonWindowsPlatform = "Test checks for the presence of Windows specific runtime libraries.";

        private readonly SignatureComparer _comparer = new();

        [Fact]
        public void ResolveCorLib()
        {
            var assemblyName = typeof(object).Assembly.GetName();
            var assemblyRef = new AssemblyReference(
                assemblyName.Name,
                assemblyName.Version!,
                false,
                assemblyName.GetPublicKeyToken());

            var resolver = new DotNetCoreAssemblyResolver(new Version(3, 1, 0));
            var assemblyDef = resolver.Resolve(assemblyRef);

            Assert.NotNull(assemblyDef);
            Assert.Equal(assemblyName.Name, assemblyDef.Name);
            Assert.NotNull(assemblyDef.ManifestModule!.FilePath);
        }

        [Fact]
        public void ResolveCorLibUsingFileService()
        {
            using var service = new ByteArrayFileService();

            var assemblyName = typeof(object).Assembly.GetName();
            var assemblyRef = new AssemblyReference(
                assemblyName.Name,
                assemblyName.Version!,
                false,
                assemblyName.GetPublicKeyToken());

            var resolver = new DotNetCoreAssemblyResolver(service, new Version(3, 1, 0));
            Assert.Empty(service.GetOpenedFiles());
            Assert.NotNull(resolver.Resolve(assemblyRef));
            Assert.NotEmpty(service.GetOpenedFiles());
        }

        [Fact]
        public void ResolveLocalLibrary()
        {
            var resolver = new DotNetCoreAssemblyResolver(new Version(3, 1, 0));
            resolver.SearchDirectories.Add(Path.GetDirectoryName(typeof(AssemblyResolverTest).Assembly.Location));

            var assemblyDef = AssemblyDefinition.FromFile(typeof(TopLevelClass1).Assembly.Location, TestReaderParameters);
            var assemblyRef = new AssemblyReference(assemblyDef);

            Assert.Equal(assemblyDef, resolver.Resolve(assemblyRef), _comparer);
            Assert.NotNull(assemblyDef.ManifestModule!.FilePath);

            resolver.ClearCache();
            Assert.False(resolver.HasCached(assemblyRef));

            resolver.AddToCache(assemblyRef, assemblyDef);
            Assert.True(resolver.HasCached(assemblyRef));
            Assert.Equal(assemblyDef, resolver.Resolve(assemblyRef));

            resolver.RemoveFromCache(assemblyRef);
            Assert.NotEqual(assemblyDef, resolver.Resolve(assemblyRef));
        }

        [Fact]
        public void ResolveWithConfiguredRuntime()
        {
            var assemblyName = typeof(object).Assembly.GetName();
            var assemblyRef = new AssemblyReference(
                assemblyName.Name,
                assemblyName.Version!,
                false,
                assemblyName.GetPublicKeyToken());

            var config = RuntimeConfiguration.FromJson(@"{
    ""runtimeOptions"": {
        ""tfm"": ""netcoreapp3.1"",
        ""framework"": {
            ""name"": ""Microsoft.NETCore.App"",
            ""version"": ""3.1.0""
        }
    }
}");
            var resolver = new DotNetCoreAssemblyResolver(config, new Version(3, 1, 0));
            var assemblyDef = resolver.Resolve(assemblyRef);

            Assert.NotNull(assemblyDef);
            Assert.Equal(assemblyName.Name, assemblyDef.Name);
            Assert.NotNull(assemblyDef.ManifestModule!.FilePath);
            Assert.Contains("Microsoft.NETCore.App", assemblyDef.ManifestModule.FilePath);
        }

        [SkippableFact]
        public void ResolveWithConfiguredRuntimeWindowsCanStillResolveCorLib()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), NonWindowsPlatform);

            var assemblyName = typeof(object).Assembly.GetName();
            var assemblyRef = new AssemblyReference(
                assemblyName.Name,
                assemblyName.Version,
                false,
                assemblyName.GetPublicKeyToken());

            var config = RuntimeConfiguration.FromJson(@"{
    ""runtimeOptions"": {
        ""tfm"": ""netcoreapp3.1"",
        ""framework"": {
            ""name"": ""Microsoft.WindowsDesktop.App"",
            ""version"": ""3.1.0""
        }
    }
}");
            var resolver = new DotNetCoreAssemblyResolver(config, new Version(3, 1, 0));
            var assemblyDef = resolver.Resolve(assemblyRef);

            Assert.NotNull(assemblyDef);
            Assert.Equal(assemblyName.Name, assemblyDef.Name);
            Assert.NotNull(assemblyDef.ManifestModule!.FilePath);
        }

        [SkippableFact]
        public void ResolveWithConfiguredRuntimeWindowsResolvesRightWindowsBase()
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), NonWindowsPlatform);

            var assemblyRef = new AssemblyReference(
                "WindowsBase",
                new Version(4,0,0,0),
                false,
                new byte[] {0x31, 0xbf, 0x38, 0x56, 0xad, 0x36, 0x4e, 0x35});

            var config = RuntimeConfiguration.FromJson(@"{
    ""runtimeOptions"": {
        ""tfm"": ""netcoreapp3.1"",
        ""framework"": {
            ""name"": ""Microsoft.WindowsDesktop.App"",
            ""version"": ""3.1.0""
        }
    }
}");
            var resolver = new DotNetCoreAssemblyResolver(config, new Version(3, 1, 0));
            var assemblyDef = resolver.Resolve(assemblyRef);

            Assert.NotNull(assemblyDef);
            Assert.Equal("WindowsBase", assemblyDef.Name);
            Assert.NotNull(assemblyDef.ManifestModule!.FilePath);
            Assert.Contains("Microsoft.WindowsDesktop.App", assemblyDef.ManifestModule.FilePath);
        }

        [SkippableTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void PreferResolveFromGac32If32BitAssembly(bool legacy)
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), NonWindowsPlatform);

            var module = new ModuleDefinition("SomeAssembly", legacy
                ? KnownCorLibs.MsCorLib_v2_0_0_0
                : KnownCorLibs.MsCorLib_v4_0_0_0);

            module.IsBit32Preferred = true;
            module.IsBit32Required = true;
            module.MachineType = MachineType.I386;
            module.PEKind = OptionalHeaderMagic.PE32;

            var resolved = module.CorLibTypeFactory.CorLibScope.GetAssembly()!.Resolve();
            Assert.NotNull(resolved);
            Assert.Contains("GAC_32", resolved.ManifestModule!.FilePath!);
        }

        [SkippableTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void PreferResolveFromGac64If64BitAssembly(bool legacy)
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), NonWindowsPlatform);

            var module = new ModuleDefinition("SomeAssembly", legacy
                ? KnownCorLibs.MsCorLib_v2_0_0_0
                : KnownCorLibs.MsCorLib_v4_0_0_0);

            module.IsBit32Preferred = false;
            module.IsBit32Required = false;
            module.MachineType = MachineType.Amd64;
            module.PEKind = OptionalHeaderMagic.PE32Plus;

            var resolved = module.CorLibTypeFactory.CorLibScope.GetAssembly()!.Resolve();
            Assert.NotNull(resolved);
            Assert.Contains("GAC_64", resolved.ManifestModule!.FilePath!);
        }

        [Fact]
        public void ResolveReferenceWithExplicitPublicKey()
        {
            // https://github.com/Washi1337/AsmResolver/issues/381

            var reference = new AssemblyReference(
                "System.Collections",
                new Version(6, 0, 0, 0),
                true,
                new byte[]
            {
                0x00, 0x24, 0x00, 0x00, 0x04, 0x80, 0x00, 0x00, 0x94, 0x00, 0x00, 0x00, 0x06, 0x02, 0x00, 0x00, 0x00,
                0x24, 0x00, 0x00, 0x52, 0x53, 0x41, 0x31, 0x00, 0x04, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x07, 0xD1,
                0xFA, 0x57, 0xC4, 0xAE, 0xD9, 0xF0, 0xA3, 0x2E, 0x84, 0xAA, 0x0F, 0xAE, 0xFD, 0x0D, 0xE9, 0xE8, 0xFD,
                0x6A, 0xEC, 0x8F, 0x87, 0xFB, 0x03, 0x76, 0x6C, 0x83, 0x4C, 0x99, 0x92, 0x1E, 0xB2, 0x3B, 0xE7, 0x9A,
                0xD9, 0xD5, 0xDC, 0xC1, 0xDD, 0x9A, 0xD2, 0x36, 0x13, 0x21, 0x02, 0x90, 0x0B, 0x72, 0x3C, 0xF9, 0x80,
                0x95, 0x7F, 0xC4, 0xE1, 0x77, 0x10, 0x8F, 0xC6, 0x07, 0x77, 0x4F, 0x29, 0xE8, 0x32, 0x0E, 0x92, 0xEA,
                0x05, 0xEC, 0xE4, 0xE8, 0x21, 0xC0, 0xA5, 0xEF, 0xE8, 0xF1, 0x64, 0x5C, 0x4C, 0x0C, 0x93, 0xC1, 0xAB,
                0x99, 0x28, 0x5D, 0x62, 0x2C, 0xAA, 0x65, 0x2C, 0x1D, 0xFA, 0xD6, 0x3D, 0x74, 0x5D, 0x6F, 0x2D, 0xE5,
                0xF1, 0x7E, 0x5E, 0xAF, 0x0F, 0xC4, 0x96, 0x3D, 0x26, 0x1C, 0x8A, 0x12, 0x43, 0x65, 0x18, 0x20, 0x6D,
                0xC0, 0x93, 0x34, 0x4D, 0x5A, 0xD2, 0x93
            });

            var module = new ModuleDefinition("Dummy", KnownCorLibs.SystemRuntime_v6_0_0_0);
            module.AssemblyReferences.Add(reference);

            var definition = reference.Resolve();

            Assert.NotNull(definition);
            Assert.Equal(reference.Name, definition.Name);
            Assert.NotNull(definition.ManifestModule!.FilePath);
        }
    }
}
