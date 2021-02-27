using System;
using System.IO;
using System.Runtime.InteropServices;
using AsmResolver.DotNet.Config.Json;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.TestCases.NestedClasses;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class AssemblyResolverTest
    {
        private const string NonWindowsPlatform = "Test checks for the presence of the Microsoft.WindowsDesktop.App runtime, which is only available on Windows.";

        private readonly SignatureComparer _comparer = new SignatureComparer();

        [Fact]
        public void ResolveCorLib()
        {
            var assemblyName = typeof(object).Assembly.GetName();
            var assemblyRef = new AssemblyReference(
                assemblyName.Name,
                assemblyName.Version,
                false,
                assemblyName.GetPublicKeyToken());

            var resolver = new DotNetCoreAssemblyResolver(new Version(3, 1, 0));
            var assemblyDef = resolver.Resolve(assemblyRef);

            Assert.NotNull(assemblyDef);
            Assert.Equal(assemblyName.Name, assemblyDef.Name);
            Assert.NotNull(assemblyDef.ManifestModule.FilePath);
        }

        [Fact]
        public void ResolveLocalLibrary()
        {
            var resolver = new DotNetCoreAssemblyResolver(new Version(3, 1, 0));
            resolver.SearchDirectories.Add(Path.GetDirectoryName(typeof(AssemblyResolverTest).Assembly.Location));

            var assemblyDef = AssemblyDefinition.FromFile(typeof(TopLevelClass1).Assembly.Location);
            var assemblyRef = new AssemblyReference(assemblyDef);

            Assert.Equal(assemblyDef, resolver.Resolve(assemblyRef), _comparer);
            Assert.NotNull(assemblyDef.ManifestModule.FilePath);

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
                assemblyName.Version,
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
            var resolver = new DotNetCoreAssemblyResolver(config);
            var assemblyDef = resolver.Resolve(assemblyRef);

            Assert.NotNull(assemblyDef);
            Assert.Equal(assemblyName.Name, assemblyDef.Name);
            Assert.NotNull(assemblyDef.ManifestModule.FilePath);
            Assert.Contains("Microsoft.NETCore.App", assemblyDef.ManifestModule.FilePath);
        }

        [SkippableFact]
        public void ResolveWithConfiguredRuntimeWindows()
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
            var resolver = new DotNetCoreAssemblyResolver(config);
            var assemblyDef = resolver.Resolve(assemblyRef);

            Assert.NotNull(assemblyDef);
            Assert.Equal(assemblyName.Name, assemblyDef.Name);
            Assert.NotNull(assemblyDef.ManifestModule.FilePath);
            Assert.Contains("Microsoft.WindowsDesktop.App", assemblyDef.ManifestModule.FilePath);
        }
    }
}
