using System;
using System.IO;
using AsmResolver.Tests.Runners;
using Xunit;

namespace AsmResolver.DotNet.Tests;

public class SilverlightPathProviderTest : IClassFixture<TemporaryDirectoryFixture>
{
    private readonly TemporaryDirectoryFixture _fixture;

    public SilverlightPathProviderTest(TemporaryDirectoryFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData(4, 0)]
    [InlineData(5, 0)]
    public void DetectReferenceAndSdkDirectories(int major, int minor)
    {
        var version = new Version(major, minor);
        string root = CreateRoot($"sl{major}");
        string referenceDirectory = CreateReferenceDirectory(root, version);
        string sdkDirectory = CreateSdkDirectory(root, version);
        var provider = CreateProvider(root, is32Bit: true);

        var installation = GetReferenceRuntime(provider, version, is32Bit: true);

        Assert.Equal(version, installation.Version);
        Assert.Equal(referenceDirectory, installation.InstallDirectory);
        Assert.Equal(sdkDirectory, installation.SdkLibraryDirectory);
        Assert.False(provider.TryGetCompatibleRuntime(version, is32Bit: true, out _));
    }

    [Fact]
    public void DetectReferenceOnlyInstallation()
    {
        string root = CreateRoot("reference-only");
        string referenceDirectory = CreateReferenceDirectory(root, new Version(5, 0));
        var provider = CreateProvider(root, is32Bit: true);

        var installation = GetReferenceRuntime(provider, new Version(5, 0), is32Bit: true);

        Assert.Equal(referenceDirectory, installation.InstallDirectory);
        Assert.Null(installation.SdkLibraryDirectory);
    }

    [Fact]
    public void DetectSdkOnlyInstallation()
    {
        string root = CreateRoot("sdk-only");
        string sdkDirectory = CreateSdkDirectory(root, new Version(5, 0));
        var provider = CreateProvider(root, is32Bit: true);

        var installation = GetReferenceRuntime(provider, new Version(5, 0), is32Bit: true);

        Assert.Equal(sdkDirectory, installation.InstallDirectory);
        Assert.Null(installation.SdkLibraryDirectory);
    }

    [Fact]
    public void DetectRuntimeOnlyInstallation()
    {
        string root = CreateRoot("runtime-only");
        var runtimeVersion = new Version(5, 1, 50918, 0);
        string runtimeDirectory = CreateRuntimeDirectory(root, runtimeVersion);
        var provider = CreateProvider(root, is32Bit: true);

        var installation = GetRuntime(provider, new Version(5, 0), is32Bit: true);

        Assert.Equal(runtimeVersion, installation.Version);
        Assert.Equal(runtimeDirectory, installation.InstallDirectory);
        Assert.Null(installation.SdkLibraryDirectory);
        Assert.False(provider.TryGetCompatibleReferenceRuntime(new Version(5, 0), is32Bit: true, out _));
    }

    [Fact]
    public void MissingInstallationReturnsFalse()
    {
        string root = CreateRoot("missing");
        var provider = CreateProvider(root, is32Bit: true);

        Assert.False(provider.TryGetCompatibleRuntime(new Version(4, 0), is32Bit: true, out _));
        Assert.False(provider.TryGetCompatibleRuntime(new Version(5, 0), is32Bit: true, out _));
        Assert.False(provider.TryGetCompatibleReferenceRuntime(new Version(4, 0), is32Bit: true, out _));
        Assert.False(provider.TryGetCompatibleReferenceRuntime(new Version(5, 0), is32Bit: true, out _));
    }

    [Fact]
    public void IgnoreMalformedRuntimeDirectoryNames()
    {
        string root = CreateRoot("malformed-runtime");
        string runtimeRoot = Path.Combine(root, "Microsoft Silverlight");

        Directory.CreateDirectory(Path.Combine(runtimeRoot, "not-a-version"));
        Directory.CreateDirectory(Path.Combine(runtimeRoot, "5.x"));

        string expectedDirectory = CreateRuntimeDirectory(root, new Version(5, 1, 50918, 0));
        var provider = CreateProvider(root, is32Bit: true);

        var installation = GetRuntime(provider, new Version(5, 0), is32Bit: true);

        Assert.Equal(expectedDirectory, installation.InstallDirectory);
    }

    [Fact]
    public void RuntimeDirectorySelectionStaysWithinTargetMajorVersion()
    {
        string root = CreateRoot("runtime-major");
        string expectedVersion4 = CreateRuntimeDirectory(root, new Version(4, 1, 10329, 0));
        string expectedVersion5 = CreateRuntimeDirectory(root, new Version(5, 1, 50918, 0));
        var provider = CreateProvider(root, is32Bit: true);

        var installation4 = GetRuntime(provider, new Version(4, 0), is32Bit: true);
        var installation5 = GetRuntime(provider, new Version(5, 0), is32Bit: true);

        Assert.Equal(expectedVersion4, installation4.InstallDirectory);
        Assert.Equal(expectedVersion5, installation5.InstallDirectory);
    }

    [Fact]
    public void RejectNullProgramFilesRoots()
    {
        Assert.Throws<ArgumentNullException>(() => new MicrosoftSilverlightPathProvider(null!, []));
        Assert.Throws<ArgumentNullException>(() => new MicrosoftSilverlightPathProvider([], null!));
    }

    [Fact]
    public void DoesNotRollSilverlight40ReferenceRuntimeForwardTo50()
    {
        string root = CreateRoot("no-roll-forward");
        CreateReferenceDirectory(root, new Version(5, 0));
        var provider = CreateProvider(root, is32Bit: true);

        Assert.False(provider.TryGetCompatibleReferenceRuntime(new Version(4, 0), is32Bit: true, out _));
        Assert.True(provider.TryGetCompatibleReferenceRuntime(new Version(5, 0), is32Bit: true, out _));
    }

    [Theory]
    [InlineData(4, 1)]
    [InlineData(5, 1)]
    public void ReferenceRuntimeRequiresExactMajorAndMinorVersion(int major, int minor)
    {
        string root = CreateRoot($"exact-reference-{major}-{minor}");
        CreateReferenceDirectory(root, new Version(major, 0));
        var provider = CreateProvider(root, is32Bit: true);

        Assert.False(provider.TryGetCompatibleReferenceRuntime(new Version(major, minor), is32Bit: true, out _));
    }

    [Fact]
    public void DoesNotDetectUnsupportedSilverlightVersions()
    {
        string root = CreateRoot("unsupported");
        CreateReferenceDirectory(root, new Version(6, 0));
        CreateRuntimeDirectory(root, new Version(6, 0, 1, 0));
        var provider = CreateProvider(root, is32Bit: true);

        Assert.False(provider.TryGetCompatibleRuntime(new Version(6, 0), is32Bit: true, out _));
        Assert.False(provider.TryGetCompatibleReferenceRuntime(new Version(6, 0), is32Bit: true, out _));
    }

    [Fact]
    public void KeepsRuntimeAndReferenceInstallationsSeparate()
    {
        string root = CreateRoot("separate-installations");
        string referenceDirectory = CreateReferenceDirectory(root, new Version(5, 0));
        string runtimeDirectory = CreateRuntimeDirectory(root, new Version(5, 1, 50918, 0));
        var provider = CreateProvider(root, is32Bit: true);

        var runtime = GetRuntime(provider, new Version(5, 0), is32Bit: true);
        var referenceRuntime = GetReferenceRuntime(provider, new Version(5, 0), is32Bit: true);

        Assert.Equal(runtimeDirectory, runtime.InstallDirectory);
        Assert.Equal(referenceDirectory, referenceRuntime.InstallDirectory);
        Assert.NotSame(runtime, referenceRuntime);
    }

    [Fact]
    public void SelectNewestRuntimeDirectoryForTargetMajorVersion()
    {
        string root = CreateRoot("runtime-selection");

        CreateRuntimeDirectory(root, new Version(5, 0, 61118, 0));
        string expectedDirectory = CreateRuntimeDirectory(root, new Version(5, 1, 50918, 0));
        CreateRuntimeDirectory(root, new Version(4, 1, 10329, 0));

        var provider = CreateProvider(root, is32Bit: true);
        var installation = GetRuntime(provider, new Version(5, 0), is32Bit: true);

        Assert.Equal(new Version(5, 1, 50918, 0), installation.Version);
        Assert.Equal(expectedDirectory, installation.InstallDirectory);
    }

    [Fact]
    public void IgnoreIncompleteReferenceAssemblyDirectory()
    {
        string root1 = CreateRoot("incomplete-reference-root1");
        string root2 = CreateRoot("incomplete-reference-root2");
        string expectedDirectory = CreateReferenceDirectory(root2, new Version(5, 0));

        CreateReferenceDirectory(root1, new Version(5, 0), includeCorLib: false);

        var provider = new MicrosoftSilverlightPathProvider([root1, root2], []);
        var installation = GetReferenceRuntime(provider, new Version(5, 0), is32Bit: true);

        Assert.Equal(expectedDirectory, installation.InstallDirectory);
    }

    [Fact]
    public void IgnoreIncompleteNewerRuntimeDirectory()
    {
        string root = CreateRoot("incomplete-runtime");
        string expectedDirectory = CreateRuntimeDirectory(root, new Version(5, 0, 61118, 0));

        CreateRuntimeDirectory(root, new Version(5, 1, 50918, 0), includeCorLib: false);

        var provider = CreateProvider(root, is32Bit: true);
        var installation = GetRuntime(provider, new Version(5, 0), is32Bit: true);

        Assert.Equal(expectedDirectory, installation.InstallDirectory);
    }

    [Fact]
    public void SelectRuntimeForRequestedArchitecture()
    {
        string root32 = CreateRoot("runtime-x86");
        string root64 = CreateRoot("runtime-x64");
        string runtime32 = CreateRuntimeDirectory(root32, new Version(5, 0, 61118, 0));
        string runtime64 = CreateRuntimeDirectory(root64, new Version(5, 1, 50918, 0));
        var provider = new MicrosoftSilverlightPathProvider([root32], [root64]);

        Assert.Equal(runtime32, GetRuntime(provider, new Version(5, 0), is32Bit: true).InstallDirectory);
        Assert.Equal(runtime64, GetRuntime(provider, new Version(5, 0), is32Bit: false).InstallDirectory);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DoesNotFallBackToOtherArchitectureForRuntime(bool installedIs32Bit)
    {
        string architecture = installedIs32Bit ? "x86" : "x64";
        string root = CreateRoot($"runtime-no-fallback-{architecture}");
        var version = new Version(5, 0);
        CreateRuntimeDirectory(root, new Version(5, 1, 50918, 0));
        var provider = CreateProvider(root, installedIs32Bit);

        Assert.False(provider.TryGetCompatibleRuntime(version, !installedIs32Bit, out _));
    }

    [Fact]
    public void PreferReferenceRuntimeForRequestedArchitecture()
    {
        string root32 = CreateRoot("reference-x86");
        string root64 = CreateRoot("reference-x64");
        string reference32 = CreateReferenceDirectory(root32, new Version(5, 0));
        string reference64 = CreateReferenceDirectory(root64, new Version(5, 0));
        var provider = new MicrosoftSilverlightPathProvider([root32], [root64]);

        Assert.Equal(reference32, GetReferenceRuntime(provider, new Version(5, 0), is32Bit: true).InstallDirectory);
        Assert.Equal(reference64, GetReferenceRuntime(provider, new Version(5, 0), is32Bit: false).InstallDirectory);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FallBackToOtherArchitectureForReferenceRuntime(bool installedIs32Bit)
    {
        string architecture = installedIs32Bit ? "x86" : "x64";
        string root = CreateRoot($"reference-fallback-{architecture}");
        var version = new Version(5, 0);
        string referenceDirectory = CreateReferenceDirectory(root, version);
        var provider = CreateProvider(root, installedIs32Bit);

        var installation = GetReferenceRuntime(provider, version, is32Bit: !installedIs32Bit);

        Assert.Equal(referenceDirectory, installation.InstallDirectory);
    }

    private static MicrosoftSilverlightPathProvider CreateProvider(string root, bool is32Bit) =>
        is32Bit
            ? new MicrosoftSilverlightPathProvider([root], [])
            : new MicrosoftSilverlightPathProvider([], [root]);

    private static SilverlightInstallation GetRuntime(
        MicrosoftSilverlightPathProvider provider,
        Version version,
        bool is32Bit)
    {
        Assert.True(provider.TryGetCompatibleRuntime(version, is32Bit, out var runtime));
        return runtime!;
    }

    private static SilverlightInstallation GetReferenceRuntime(
        MicrosoftSilverlightPathProvider provider,
        Version version,
        bool is32Bit)
    {
        Assert.True(provider.TryGetCompatibleReferenceRuntime(version, is32Bit, out var runtime));
        return runtime!;
    }

    private static string CreateReferenceDirectory(string root, Version version, bool includeCorLib = true)
    {
        string directory = CreateDirectory(
            root,
            "Reference Assemblies",
            "Microsoft",
            "Framework",
            "Silverlight",
            $"v{version.Major}.{version.Minor}"
        );

        if (includeCorLib)
            CreatePlaceholder(directory, "mscorlib.dll");

        return directory;
    }

    private static string CreateSdkDirectory(string root, Version version)
    {
        return CreateDirectory(
            root,
            "Microsoft SDKs",
            "Silverlight",
            $"v{version.Major}.{version.Minor}",
            "Libraries",
            "Client"
        );
    }

    private static string CreateRuntimeDirectory(string root, Version version, bool includeCorLib = true)
    {
        string directory = CreateDirectory(root, "Microsoft Silverlight", version.ToString());

        if (includeCorLib)
            CreatePlaceholder(directory, "mscorlib.dll");

        return directory;
    }

    private static string CreateDirectory(string root, params string[] segments)
    {
        string path = root;

        for (int i = 0; i < segments.Length; i++)
            path = Path.Combine(path, segments[i]);

        return Directory.CreateDirectory(path).FullName;
    }

    private static void CreatePlaceholder(string directory, string name) => File.WriteAllBytes(Path.Combine(directory, name), []);

    private string CreateRoot(string name)
    {
        string path = Path.Combine(_fixture.BasePath, "Silverlight", name);

        Directory.CreateDirectory(path);

        return path;
    }
}
