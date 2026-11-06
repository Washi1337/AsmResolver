using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using AsmResolver.DotNet.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.IO;
using AsmResolver.Tests.Runners;
using Xunit;

namespace AsmResolver.DotNet.Tests;

public class SilverlightAssemblyResolverTest : IClassFixture<TemporaryDirectoryFixture>
{
    private const string NonWindowsPlatform =
        "Test checks for the presence of Windows-specific Silverlight libraries.";

    private readonly TemporaryDirectoryFixture _fixture;

    public SilverlightAssemblyResolverTest(TemporaryDirectoryFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void PreferRuntimeAssembliesOverReferenceSdkAndSearchDirectories()
    {
        var directories = CreateResolverDirectories("priority");
        string expectedPath = CreatePlaceholder(directories.RuntimeDirectory, "Dependency.dll");

        CreatePlaceholder(directories.ReferenceDirectory, "Dependency.dll");
        CreatePlaceholder(directories.SdkDirectory, "Dependency.dll");
        CreatePlaceholder(directories.SearchDirectory, "Dependency.dll");

        var resolver = CreateResolver(directories);
        resolver.SearchDirectories.Add(directories.SearchDirectory);

        Assert.Equal(expectedPath, ProbeAssembly(resolver, "Dependency"));
    }

    [Fact]
    public void FallsBackToReferenceAssembliesAndThenSdkWhenRuntimeIsUnavailable()
    {
        var directories = CreateResolverDirectories("fallback-order");
        string referencePath = CreatePlaceholder(directories.ReferenceDirectory, "SharedDependency.dll");
        CreatePlaceholder(directories.SdkDirectory, "SharedDependency.dll");
        string sdkPath = CreatePlaceholder(directories.SdkDirectory, "SdkDependency.dll");
        var resolver = CreateResolver(directories, includeRuntime: false);

        Assert.Equal(referencePath, ProbeAssembly(resolver, "SharedDependency"));
        Assert.Equal(sdkPath, ProbeAssembly(resolver, "SdkDependency"));
    }

    [Fact]
    public void FallsBackToCustomSearchDirectories()
    {
        var directories = CreateResolverDirectories("search-directories");
        string expectedPath = CreatePlaceholder(directories.SearchDirectory, "ApplicationDependency.dll");
        var resolver = CreateResolver(directories);

        resolver.SearchDirectories.Add(directories.SearchDirectory);

        Assert.Equal(expectedPath, ProbeAssembly(resolver, "ApplicationDependency"));
    }

    [Fact]
    public void DoesNotUseReferenceInstallationWhenRuntimeIsAvailable()
    {
        var directories = CreateResolverDirectories("runtime-with-reference");
        CreatePlaceholder(directories.ReferenceDirectory, "ApplicationDependency.dll");
        string expectedPath = CreatePlaceholder(directories.SearchDirectory, "ApplicationDependency.dll");
        var resolver = CreateResolver(directories);

        resolver.SearchDirectories.Add(directories.SearchDirectory);

        Assert.Equal(expectedPath, ProbeAssembly(resolver, "ApplicationDependency"));
    }

    [Fact]
    public void FallsBackToOriginModuleDirectory()
    {
        var directories = CreateResolverDirectories("origin-module");
        string applicationDirectory = CreateDirectory("origin-module", "Application");
        string expectedPath = CreatePlaceholder(applicationDirectory, "ApplicationDependency.dll");

        string originPath = WriteAssembly(
            applicationDirectory,
            "Application",
            new Version(1, 0, 0, 0),
            KnownCorLibs.MsCorLib_v4_0_0_0
        );

        var originAssembly = AssemblyDefinition.FromFile(originPath, createRuntimeContext: false);
        var resolver = CreateResolver(directories);

        Assert.Equal(expectedPath, ProbeAssembly(resolver, "ApplicationDependency", originAssembly.ManifestModule));
    }

    [Fact]
    public void FallsBackToReaderWorkingDirectory()
    {
        var directories = CreateResolverDirectories("working-directory");
        string expectedPath = CreatePlaceholder(directories.SearchDirectory, "ApplicationDependency.dll");

        var readerParameters = new ModuleReaderParameters(directories.SearchDirectory);

        var resolver = CreateResolver(directories, readerParameters: readerParameters);

        Assert.Same(readerParameters, resolver.ReaderParameters);
        Assert.Equal(expectedPath, ProbeAssembly(resolver, "ApplicationDependency"));
    }

    [Fact]
    public void ResolveAssemblyUsingConfiguredFileService()
    {
        var directories = CreateResolverDirectories("file-service");

        WriteAssembly(
            directories.RuntimeDirectory,
            "mscorlib",
            KnownCorLibs.MsCorLib_v5_0_5_0.Version,
            null
        );

        using var service = new ByteArrayFileService();
        var resolver = CreateResolver(directories, readerParameters: new ModuleReaderParameters(service));

        Assert.Empty(service.GetOpenedFiles());
        Assert.Equal(ResolutionStatus.Success, resolver.Resolve(KnownCorLibs.MsCorLib_v5_0_5_0, null, out _));
        Assert.NotEmpty(service.GetOpenedFiles());
    }

    [Fact]
    public void ResolverWithoutInstalledSilverlightStillUsesCustomSearchDirectories()
    {
        var directories = CreateResolverDirectories("no-installation");
        string emptyProgramFilesRoot = CreateDirectory("empty-program-files");
        string expectedPath = CreatePlaceholder(directories.SearchDirectory, "ApplicationDependency.dll");

        var provider = new MicrosoftSilverlightPathProvider([emptyProgramFilesRoot], []);
        var resolver = new SilverlightAssemblyResolver(new Version(5, 0), is32Bit: true, provider);

        resolver.SearchDirectories.Add(directories.SearchDirectory);

        Assert.Equal(expectedPath, ProbeAssembly(resolver, "ApplicationDependency"));
    }

    [Theory]
    [InlineData(4, 0, true)]
    [InlineData(5, 0, false)]
    public void RequestsRuntimeUsingTargetVersionAndArchitecture(int major, int minor, bool is32Bit)
    {
        var version = new Version(major, minor);
        var runtime = new SilverlightInstallation(
            new Version(major, 1),
            CreateDirectory($"runtime-request-{major}-{is32Bit}")
        );
        var referenceRuntime = new SilverlightInstallation(
            version,
            CreateDirectory($"unused-reference-request-{major}-{is32Bit}")
        );
        var provider = new TestPathProvider(runtime, referenceRuntime);

        _ = new SilverlightAssemblyResolver(version, is32Bit, provider);

        Assert.Equal(version, provider.RuntimeRequest?.Version);
        Assert.Equal(is32Bit, provider.RuntimeRequest?.Is32Bit);
        Assert.Null(provider.ReferenceRuntimeRequest);
    }

    [Theory]
    [InlineData(4, 0, true)]
    [InlineData(5, 0, false)]
    public void RequestsReferenceRuntimeUsingTargetVersionAndArchitecture(int major, int minor, bool is32Bit)
    {
        var version = new Version(major, minor);
        var referenceRuntime = new SilverlightInstallation(
            version,
            CreateDirectory($"reference-request-{major}-{is32Bit}")
        );
        var provider = new TestPathProvider(null, referenceRuntime);

        _ = new SilverlightAssemblyResolver(version, is32Bit, provider);

        Assert.Equal(version, provider.RuntimeRequest?.Version);
        Assert.Equal(is32Bit, provider.RuntimeRequest?.Is32Bit);
        Assert.Equal(version, provider.ReferenceRuntimeRequest?.Version);
        Assert.Equal(is32Bit, provider.ReferenceRuntimeRequest?.Is32Bit);
    }

    [Theory]
    [InlineData(4, 0)]
    [InlineData(5, 0)]
    public void ResolveCorLibFromSyntheticRuntimeInstallation(int major, int minor)
    {
        var directories = CreateResolverDirectories($"resolve-sl{major}");
        var runtime = DotNetRuntimeInfo.Silverlight(major, minor);
        var corLib = runtime.GetDefaultCorLib();

        WriteAssembly(directories.RuntimeDirectory, "mscorlib", corLib.Version, null);

        var resolver = CreateResolver(directories, runtime.Version);
        var status = resolver.Resolve(corLib, null, out var assembly);

        Assert.Equal(ResolutionStatus.Success, status);
        Assert.Equal(directories.RuntimeDirectory, Path.GetDirectoryName(assembly!.ManifestModule!.FilePath));
    }

    [SkippableTheory]
    [InlineData(4, 0)]
    [InlineData(5, 0)]
    public void ResolveCoreSilverlightLibrariesFromInstalledSdk(int major, int minor)
    {
        Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), NonWindowsPlatform);

        var version = new Version(major, minor);
        bool hasInstallation = MicrosoftSilverlightPathProvider.Instance.TryGetCompatibleReferenceRuntime(
            version,
            IntPtr.Size == sizeof(uint),
            out var installation
        ) && File.Exists(Path.Combine(installation.InstallDirectory, "mscorlib.dll"));

        Skip.IfNot(hasInstallation, $"Silverlight {version} reference assemblies are not installed.");

        var resolver = new SilverlightAssemblyResolver(installation!);
        var corLib = ResolveAssembly(resolver, "mscorlib");

        Assert.Equal("mscorlib", corLib.Name);
        Assert.Equal<AssemblyDescriptor>(
            DotNetRuntimeInfo.Silverlight(major, minor).GetDefaultCorLib(),
            corLib,
            SignatureComparer.Default
        );

        AssertCanResolve(resolver, "System");
        AssertCanResolve(resolver, "System.Windows");

        if (installation!.SdkLibraryDirectory is { } sdkDirectory
            && File.Exists(Path.Combine(sdkDirectory, "System.Windows.Controls.dll")))
            AssertCanResolve(resolver, "System.Windows.Controls");
    }

    private static void AssertCanResolve(SilverlightAssemblyResolver resolver, string name)
    {
        var assembly = ResolveAssembly(resolver, name);

        Assert.Equal(name, assembly.Name);
    }

    private static AssemblyDefinition ResolveAssembly(SilverlightAssemblyResolver resolver, string name)
    {
        var status = resolver.Resolve(CreateReference(name), null, out var assembly);

        Assert.Equal(ResolutionStatus.Success, status);

        return assembly!;
    }

    private static string? ProbeAssembly(
        SilverlightAssemblyResolver resolver,
        string name,
        ModuleDefinition? originModule = null)
    {
        return resolver.ProbeAssemblyFilePath(CreateReference(name), originModule);
    }

    private static AssemblyReference CreateReference(string name) => new(name, new Version());

    private SilverlightAssemblyResolver CreateResolver(
        ResolverDirectories directories,
        Version? version = null,
        ModuleReaderParameters? readerParameters = null,
        bool includeRuntime = true,
        bool is32Bit = true)
    {
        version ??= new Version(5, 0);
        var referenceRuntime = new SilverlightInstallation(
            version,
            directories.ReferenceDirectory,
            directories.SdkDirectory
        );
        var runtime = includeRuntime
            ? new SilverlightInstallation(
                new Version(version.Major, 1),
                directories.RuntimeDirectory
            )
            : null;
        var provider = new TestPathProvider(runtime, referenceRuntime);

        return new SilverlightAssemblyResolver(version, is32Bit, provider, readerParameters);
    }

    private ResolverDirectories CreateResolverDirectories(string name)
    {
        return new ResolverDirectories(
            CreateDirectory(name, "Reference"),
            CreateDirectory(name, "Sdk"),
            CreateDirectory(name, "Runtime"),
            CreateDirectory(name, "Search")
        );
    }

    private string CreateDirectory(params string[] segments)
    {
        string path = Path.Combine(_fixture.BasePath, "SilverlightResolver");

        for (int i = 0; i < segments.Length; i++)
            path = Path.Combine(path, segments[i]);

        return Directory.CreateDirectory(path).FullName;
    }

    private static string CreatePlaceholder(string directory, string name)
    {
        string path = Path.Combine(directory, name);

        File.WriteAllBytes(path, []);

        return path;
    }

    private static string WriteAssembly(
        string directory,
        string assemblyName,
        Version assemblyVersion,
        AssemblyReference? corLib)
    {
        string fileName = $"{assemblyName}.dll";
        string path = Path.Combine(directory, fileName);
        var assembly = new AssemblyDefinition(assemblyName, assemblyVersion);
        var module = new ModuleDefinition(fileName, corLib);
        assembly.Modules.Add(module);
        assembly.Write(path);
        return path;
    }

    private readonly record struct ResolverDirectories(
        string ReferenceDirectory,
        string SdkDirectory,
        string RuntimeDirectory,
        string SearchDirectory);

    private sealed class TestPathProvider(
        SilverlightInstallation? runtime,
        SilverlightInstallation? referenceRuntime) : SilverlightPathProvider
    {
        public (Version Version, bool Is32Bit)? RuntimeRequest
        {
            get;
            private set;
        }

        public (Version Version, bool Is32Bit)? ReferenceRuntimeRequest
        {
            get;
            private set;
        }

        public override bool TryGetCompatibleRuntime(
            Version version,
            bool is32Bit,
            [NotNullWhen(true)] out SilverlightInstallation? installation)
        {
            RuntimeRequest = (version, is32Bit);
            installation = runtime;
            return installation is not null;
        }

        public override bool TryGetCompatibleReferenceRuntime(
            Version version,
            bool is32Bit,
            [NotNullWhen(true)] out SilverlightInstallation? installation)
        {
            ReferenceRuntimeRequest = (version, is32Bit);
            installation = referenceRuntime;
            return installation is not null;
        }
    }
}
