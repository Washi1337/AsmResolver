using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using AsmResolver.DotNet.Config.Json;
using AsmResolver.DotNet.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.Tests.Runners;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class AssemblyResolverTest : IClassFixture<TemporaryDirectoryFixture>
    {
        private const string NonWindowsPlatform = "Test checks for the presence of Windows specific runtime libraries.";

        private readonly TemporaryDirectoryFixture _fixture;

        public AssemblyResolverTest(TemporaryDirectoryFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData(2, 0)]
        [InlineData(4, 0)]
        [InlineData(4, 7)]
        public void ResolveFrameworkCorLib(int major, int minor)
        {
            var runtimeInfo = DotNetRuntimeInfo.NetFramework(major, minor);
            var corlib = KnownCorLibs.FromRuntimeInfo(runtimeInfo);

            var resolver = new DotNetFxAssemblyResolver(runtimeInfo.Version, nint.Size == sizeof(uint));
            var status = resolver.Resolve(corlib, null, out var assemblyDef);

            Assert.Equal(ResolutionStatus.Success, status);
            Assert.Equal(corlib.Name, assemblyDef!.Name);
            Assert.NotNull(assemblyDef.ManifestModule!.FilePath);

            // mscorlib always resolves to the default install directory.
            Assert.DoesNotContain("gac", assemblyDef.ManifestModule!.FilePath!, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ResolveFramework20CorLibOnFramework40()
        {
            var runtimeInfo = DotNetRuntimeInfo.NetFramework(4, 0);
            var corlib = KnownCorLibs.MsCorLib_v2_0_0_0;

            var resolver = new DotNetFxAssemblyResolver(runtimeInfo.Version, nint.Size == sizeof(uint));
            var status = resolver.Resolve(corlib, null, out var assemblyDef);

            Assert.Equal(ResolutionStatus.Success, status);
            Assert.Equal<AssemblyDescriptor>(KnownCorLibs.MsCorLib_v4_0_0_0, assemblyDef, SignatureComparer.Default);
            Assert.NotNull(assemblyDef!.ManifestModule!.FilePath);
        }

        [Theory]
        [InlineData(3, 1)]
        [InlineData(8, 0)]
        public void ResolveCoreCorLib(int major, int minor)
        {
            var corlib = KnownCorLibs.FromRuntimeInfo(DotNetRuntimeInfo.NetCoreApp(major, minor));

            var resolver = new DotNetCoreAssemblyResolver(new Version(major, minor, 0));
            var status = resolver.Resolve(corlib, null, out var assemblyDef);

            Assert.Equal(ResolutionStatus.Success, status);
            Assert.Equal(corlib.Name, assemblyDef!.Name);
            Assert.NotNull(assemblyDef.ManifestModule!.FilePath);
        }

        [Fact]
        public void ResolveCorLibUsingFileService()
        {
            using var service = new ByteArrayFileService();

            var assemblyRef = KnownCorLibs.SystemPrivateCoreLib_v10_0_0_0;

            var resolver = new DotNetCoreAssemblyResolver(
                new Version(10, 0),
                readerParameters: new ModuleReaderParameters(service)
            );

            Assert.Empty(service.GetOpenedFiles());
            Assert.Equal(ResolutionStatus.Success, resolver.Resolve(assemblyRef, null, out _));
            Assert.NotEmpty(service.GetOpenedFiles());
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

            var config = RuntimeConfiguration.FromJson(
                """
                {
                    "runtimeOptions": {
                        "tfm": "netcoreapp3.1",
                        "framework": {
                            "name": "Microsoft.NETCore.App",
                            "version": "3.1.0"
                        }
                    }
                }
                """
            )!;

            var resolver = new DotNetCoreAssemblyResolver(config);
            var status = resolver.Resolve(assemblyRef, null, out var assemblyDef);

            Assert.Equal(ResolutionStatus.Success, status);
            Assert.Equal(assemblyName.Name, assemblyDef!.Name);
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
                assemblyName.Version!,
                false,
                assemblyName.GetPublicKeyToken()
            );

            var config = RuntimeConfiguration.FromJson(
                """
                {
                    "runtimeOptions": {
                        "tfm": "netcoreapp3.1",
                        "framework": {
                            "name": "Microsoft.WindowsDesktop.App",
                            "version": "3.1.0"
                        }
                    }
                }
                """
            );

            var resolver = new DotNetCoreAssemblyResolver(config);
            var status = resolver.Resolve(assemblyRef, null, out var assemblyDef);

            Assert.Equal(ResolutionStatus.Success, status);
            Assert.Equal(assemblyName.Name, assemblyDef!.Name);
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

            var config = RuntimeConfiguration.FromJson(
                """
                {
                    "runtimeOptions": {
                        "tfm": "netcoreapp3.1",
                        "framework": {
                            "name": "Microsoft.WindowsDesktop.App",
                            "version": "3.1.0"
                        }
                    }
                }
                """)
            ;
            var resolver = new DotNetCoreAssemblyResolver(config);
            var status = resolver.Resolve(assemblyRef, null, out var assemblyDef);

            Assert.Equal(ResolutionStatus.Success, status);
            Assert.Equal("WindowsBase", assemblyDef!.Name);
            Assert.NotNull(assemblyDef.ManifestModule!.FilePath);
            Assert.Contains("Microsoft.WindowsDesktop.App", assemblyDef.ManifestModule.FilePath);
        }

        [SkippableTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void PreferResolveFromGac32If32BitAssembly(bool legacy)
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), NonWindowsPlatform);

            var reference = new AssemblyReference(
                name: "System.Web",
                version: legacy ? new Version(2, 0, 0, 0) : new Version(4, 0, 0, 0),
                publicKey: false,
                publicKeyOrToken: [0xb0, 0x3f, 0x5f, 0x7f, 0x11, 0xd5, 0x0a, 0x3a]
            );

            var resolver = new DotNetFxAssemblyResolver(
                legacy ? new Version(2, 0) : new Version(4, 0),
                is32Bit: true
            );
            var status = resolver.Resolve(reference, null, out var definition);

            Assert.Equal(ResolutionStatus.Success, status);
            Assert.Contains("GAC_32", definition!.ManifestModule!.FilePath!);
        }

        [SkippableTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void PreferResolveFromGac64If64BitAssembly(bool legacy)
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), NonWindowsPlatform);

            var reference = new AssemblyReference(
                name: "System.Web",
                version: legacy ? new Version(2, 0, 0, 0) : new Version(4, 0, 0, 0),
                publicKey: false,
                publicKeyOrToken: [0xb0, 0x3f, 0x5f, 0x7f, 0x11, 0xd5, 0x0a, 0x3a]
            );

            var resolver = new DotNetFxAssemblyResolver(
                legacy ? new Version(2, 0) : new Version(4, 0),
                is32Bit: false
            );
            var status = resolver.Resolve(reference, null, out var definition);

            Assert.Equal(ResolutionStatus.Success, status);
            Assert.Contains("GAC_64", definition!.ManifestModule!.FilePath!);
        }

        [SkippableTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void ResolveFromGacMsil(bool legacy)
        {
            Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), NonWindowsPlatform);

            var reference = new AssemblyReference(
                name: "System",
                version: legacy ? new Version(2, 0, 0, 0) : new Version(4, 0, 0, 0),
                publicKey: false,
                publicKeyOrToken: [0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89]
            );

            var resolver = new DotNetFxAssemblyResolver(
                legacy ? new Version(2, 0) : new Version(4, 0),
                is32Bit: false
            );
            var status = resolver.Resolve(reference, null, out var definition);

            Assert.Equal(ResolutionStatus.Success, status);
            Assert.Contains("GAC_MSIL", definition!.ManifestModule!.FilePath!);
        }

        [Fact]
        public void ResolveReferenceWithExplicitPublicKey()
        {
            // https://github.com/Washi1337/AsmResolver/issues/381

            var reference = new AssemblyReference(
                "System.Collections",
                new Version(6, 0, 0, 0),
                true,
                [
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
                ]
            );

            var resolver = new DotNetCoreAssemblyResolver(new Version(6, 0));
            var status = resolver.Resolve(reference, null, out var definition);

            Assert.Equal(ResolutionStatus.Success, status);
            Assert.Equal(reference.Name, definition!.Name);
            Assert.NotNull(definition.ManifestModule!.FilePath);
        }

        [Fact]
        public void ResolveNonRuntimeDependencyInSameDirectory()
        {
            string basePath = _fixture.GetRunner<CorePERunner>().GetTestDirectory();
            PrepareTestEnvironment(basePath);

            var module = ModuleDefinition.FromFile(Path.Combine(basePath, "Main.dll"));
            var dependency = module.AssemblyReferences.First(x => x.Name == "Dependency").Resolve(module.RuntimeContext);

            Assert.Equal(basePath, Path.GetDirectoryName(dependency.ManifestModule!.FilePath));
            return;

            static void PrepareTestEnvironment(string basePath)
            {
                var dependencyAssembly = new AssemblyDefinition("Dependency", new Version(1, 0, 0, 0));
                var dependencyModule = new ModuleDefinition("Dependency.dll", KnownCorLibs.SystemRuntime_v10_0_0_0);
                dependencyAssembly.Modules.Add(dependencyModule);
                var dependencyType = new TypeDefinition(
                    "Dependency",
                    "Type",
                    TypeAttributes.Public,
                    dependencyModule.CorLibTypeFactory.Object.Type
                );
                dependencyModule.TopLevelTypes.Add(dependencyType);
                dependencyModule.Write(Path.Combine(basePath, "Dependency.dll"));

                var mainAssembly = new AssemblyDefinition("Main", new Version(1, 0, 0, 0));
                var mainModule = new ModuleDefinition("Main.dll", KnownCorLibs.SystemRuntime_v10_0_0_0);
                mainAssembly.Modules.Add(mainModule);
                mainModule.GetOrCreateModuleType().Fields.Add(new FieldDefinition(
                    "Foo",
                    FieldAttributes.Static,
                    dependencyType.ToTypeSignature()
                ));

                mainAssembly.Write(Path.Combine(basePath, "Main.dll"));

                File.WriteAllText(Path.Combine(basePath, "Main.runtimeconfig.json"),
                    """
                    {
                        "runtimeOptions": {
                            "tfm": "net10.0",
                            "includedFrameworks": [{
                                "name": "Microsoft.NETCore.App",
                                "version": "10.0.0"
                            }]
                        }
                    }
                    """
                );
            }
        }

        [Fact]
        public void ResolveRuntimeDependencyLookalikeNonSelfContainedShouldPreferSystemDirectory()
        {
            string basePath = _fixture.GetRunner<CorePERunner>().GetTestDirectory();
            PrepareTestEnvironment(basePath);

            var module = ModuleDefinition.FromFile(Path.Combine(basePath, "Main.dll"));
            var dependency = module.AssemblyReferences.First(x => x.Name == "System.Private.CoreLib").Resolve(module.RuntimeContext);

            Assert.NotEqual(basePath, Path.GetDirectoryName(dependency.ManifestModule!.FilePath));
            return;

            static void PrepareTestEnvironment(string basePath)
            {
                File.Copy(typeof(object).Assembly.Location, Path.Combine(basePath, "System.Private.CoreLib.dll"));

                var mainAssembly = new AssemblyDefinition("Main", new Version(1, 0, 0, 0));
                var mainModule = new ModuleDefinition("Main.dll", KnownCorLibs.SystemPrivateCoreLib_v10_0_0_0);
                mainAssembly.Modules.Add(mainModule);
                mainModule.GetOrCreateModuleType().Fields.Add(new FieldDefinition(
                    "Foo",
                    FieldAttributes.Static,
                    KnownCorLibs.SystemRuntime_v10_0_0_0.CreateTypeReference("System.IO", "Stream").ToTypeSignature(false)
                ));

                mainAssembly.Write(Path.Combine(basePath, "Main.dll"));

                File.WriteAllText(Path.Combine(basePath, "Main.runtimeconfig.json"),
                    """
                    {
                        "runtimeOptions": {
                            "tfm": "net10.0",
                            "includedFrameworks": [{
                                "name": "Microsoft.NETCore.App",
                                "version": "10.0.0"
                            }]
                        }
                    }
                    """
                );
            }
        }

        [Fact]
        public void ResolveRuntimeDependencySelfContainedShouldPreferSameDirectory()
        {
            string basePath = _fixture.GetRunner<CorePERunner>().GetTestDirectory();
            PrepareTestEnvironment(basePath);

            var module = ModuleDefinition.FromFile(Path.Combine(basePath, "Main.dll"));
            var dependency = module.AssemblyReferences.First(x => x.Name == "System.Private.CoreLib").Resolve(module.RuntimeContext);

            Assert.Equal(basePath, Path.GetDirectoryName(dependency.ManifestModule!.FilePath));
            return;

            static void PrepareTestEnvironment(string basePath)
            {
                File.Copy(typeof(object).Assembly.Location, Path.Combine(basePath, "System.Private.CoreLib.dll"));

                var mainAssembly = new AssemblyDefinition("Main", new Version(1, 0, 0, 0));
                var mainModule = new ModuleDefinition("Main.dll", KnownCorLibs.SystemPrivateCoreLib_v10_0_0_0);
                mainAssembly.Modules.Add(mainModule);
                mainModule.GetOrCreateModuleType().Fields.Add(new FieldDefinition(
                    "Foo",
                    FieldAttributes.Static,
                    KnownCorLibs.SystemRuntime_v10_0_0_0.CreateTypeReference("System.IO", "Stream").ToTypeSignature(false)
                ));

                mainAssembly.Write(Path.Combine(basePath, "Main.dll"));

                File.WriteAllText(Path.Combine(basePath, "Main.runtimeconfig.json"),
                    """
                    {
                        "runtimeOptions": {
                            "tfm": "net10.0",
                            "framework": {
                                "name": "Microsoft.NETCore.App",
                                "version": "10.0.0"
                            }
                        }
                    }
                    """
                );
            }
        }

        [Fact]
        public void ResolveUsingNullResolver()
        {
            var assemblyRef = KnownCorLibs.SystemPrivateCoreLib_v10_0_0_0;

            var defaultResolver = new DotNetCoreAssemblyResolver(new Version(10, 0));
            var nullResolver = NullAssemblyResolver.Instance;

            Assert.Equal(ResolutionStatus.Success, defaultResolver.Resolve(assemblyRef, null, out _));
            Assert.Equal(ResolutionStatus.AssemblyNotFound, nullResolver.Resolve(assemblyRef, null, out _));
        }
    }
}
