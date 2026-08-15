using System;
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
    public void PreferReferenceAssembliesOverSdkRuntimeAndSearchDirectories()
    {
        var directories = CreateResolverDirectories("priority");
        string expectedPath = CreatePlaceholder(directories.ReferenceDirectory, "Dependency.dll");

        CreatePlaceholder(directories.SdkDirectory, "Dependency.dll");
        CreatePlaceholder(directories.RuntimeDirectory, "Dependency.dll");
        CreatePlaceholder(directories.SearchDirectory, "Dependency.dll");

        var resolver = CreateResolver(directories);
        resolver.SearchDirectories.Add(directories.SearchDirectory);

        Assert.Equal(expectedPath, ProbeAssembly(resolver, "Dependency"));
    }

    [Fact]
    public void FallsBackFromReferenceAssembliesToSdkThenRuntime()
    {
        var directories = CreateResolverDirectories("fallback-order");
        string sdkPath = CreatePlaceholder(directories.SdkDirectory, "SdkDependency.dll");
        string runtimePath = CreatePlaceholder(directories.RuntimeDirectory, "RuntimeDependency.dll");
        var resolver = CreateResolver(directories);

        Assert.Equal(sdkPath, ProbeAssembly(resolver, "SdkDependency"));
        Assert.Equal(runtimePath, ProbeAssembly(resolver, "RuntimeDependency"));
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
            directories.ReferenceDirectory,
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

        var provider = new MicrosoftSilverlightPathProvider([emptyProgramFilesRoot]);
        var resolver = new SilverlightAssemblyResolver(new Version(5, 0), provider);

        resolver.SearchDirectories.Add(directories.SearchDirectory);

        Assert.Equal(expectedPath, ProbeAssembly(resolver, "ApplicationDependency"));
    }

    [Theory]
    [InlineData(4, 0)]
    [InlineData(5, 0)]
    public void ResolveCorLibFromSyntheticReferenceInstallation(int major, int minor)
    {
        var directories = CreateResolverDirectories($"resolve-sl{major}");
        var runtime = DotNetRuntimeInfo.Silverlight(major, minor);
        var corLib = runtime.GetDefaultCorLib();

        WriteAssembly(directories.ReferenceDirectory, "mscorlib", corLib.Version, null);

        var resolver = CreateResolver(directories, runtime.Version);
        var status = resolver.Resolve(corLib, null, out var assembly);

        Assert.Equal(ResolutionStatus.Success, status);
        Assert.Equal(directories.ReferenceDirectory, Path.GetDirectoryName(assembly!.ManifestModule!.FilePath));
    }

    [SkippableTheory]
    [InlineData(4, 0)]
    [InlineData(5, 0)]
    public void ResolveCoreSilverlightLibrariesFromInstalledSdk(int major, int minor)
    {
        Skip.IfNot(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), NonWindowsPlatform);

        var version = new Version(major, minor);
        bool hasInstallation = MicrosoftSilverlightPathProvider.Instance.TryGetCompatibleInstallation(
            version,
            out var installation
        ) && installation?.ReferenceAssemblyDirectory is not null;

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
        ModuleReaderParameters? readerParameters = null)
    {
        var installation = new SilverlightInstallation(
            version ?? new Version(5, 0),
            directories.ReferenceDirectory,
            directories.SdkDirectory,
            directories.RuntimeDirectory
        );

        return new SilverlightAssemblyResolver(installation, readerParameters);
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
}
