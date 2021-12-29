using System;
using System.IO;
using System.Runtime.InteropServices;
using AsmResolver.DotNet.Config.Json;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.TestCases.NestedClasses;
using AsmResolver.IO;
using AsmResolver.PE.File.Headers;
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

            var assemblyDef = AssemblyDefinition.FromFile(typeof(TopLevelClass1).Assembly.Location);
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
            module.PEKind = OptionalHeaderMagic.Pe32;

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
            module.PEKind = OptionalHeaderMagic.Pe32Plus;

            var resolved = module.CorLibTypeFactory.CorLibScope.GetAssembly()!.Resolve();
            Assert.NotNull(resolved);
            Assert.Contains("GAC_64", resolved.ManifestModule!.FilePath!);
        }
    }
}
