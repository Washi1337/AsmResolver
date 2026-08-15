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
        var provider = new MicrosoftSilverlightPathProvider([root]);

        var installation = GetInstallation(provider, version);

        Assert.Equal(version, installation.Version);
        Assert.Equal(referenceDirectory, installation.ReferenceAssemblyDirectory);
        Assert.Equal(sdkDirectory, installation.SdkLibraryDirectory);
        Assert.Null(installation.RuntimeDirectory);
    }

    [Fact]
    public void DetectReferenceOnlyInstallation()
    {
        string root = CreateRoot("reference-only");
        string referenceDirectory = CreateReferenceDirectory(root, new Version(5, 0));
        var provider = new MicrosoftSilverlightPathProvider([root]);

        var installation = GetInstallation(provider, new Version(5, 0));

        Assert.Equal(referenceDirectory, installation.ReferenceAssemblyDirectory);
        Assert.Null(installation.SdkLibraryDirectory);
        Assert.Null(installation.RuntimeDirectory);
    }

    [Fact]
    public void DetectSdkOnlyInstallation()
    {
        string root = CreateRoot("sdk-only");
        string sdkDirectory = CreateSdkDirectory(root, new Version(5, 0));
        var provider = new MicrosoftSilverlightPathProvider([root]);

        var installation = GetInstallation(provider, new Version(5, 0));

        Assert.Null(installation.ReferenceAssemblyDirectory);
        Assert.Equal(sdkDirectory, installation.SdkLibraryDirectory);
        Assert.Null(installation.RuntimeDirectory);
    }

    [Fact]
    public void DetectRuntimeOnlyInstallation()
    {
        string root = CreateRoot("runtime-only");
        string runtimeDirectory = CreateRuntimeDirectory(root, new Version(5, 1, 50918, 0));
        var provider = new MicrosoftSilverlightPathProvider([root]);

        var installation = GetInstallation(provider, new Version(5, 0));

        Assert.Null(installation.ReferenceAssemblyDirectory);
        Assert.Null(installation.SdkLibraryDirectory);
        Assert.Equal(runtimeDirectory, installation.RuntimeDirectory);
    }

    [Fact]
    public void MissingInstallationReturnsFalse()
    {
        string root = CreateRoot("missing");
        var provider = new MicrosoftSilverlightPathProvider([root]);

        Assert.False(provider.TryGetCompatibleInstallation(new Version(4, 0), out _));
        Assert.False(provider.TryGetCompatibleInstallation(new Version(5, 0), out _));
    }

    [Fact]
    public void IgnoreMalformedRuntimeDirectoryNames()
    {
        string root = CreateRoot("malformed-runtime");
        string runtimeRoot = Path.Combine(root, "Microsoft Silverlight");

        Directory.CreateDirectory(Path.Combine(runtimeRoot, "not-a-version"));
        Directory.CreateDirectory(Path.Combine(runtimeRoot, "5.x"));

        string expectedDirectory = CreateRuntimeDirectory(root, new Version(5, 1, 50918, 0));
        var provider = new MicrosoftSilverlightPathProvider([root]);

        var installation = GetInstallation(provider, new Version(5, 0));

        Assert.Equal(expectedDirectory, installation.RuntimeDirectory);
    }

    [Fact]
    public void RuntimeDirectorySelectionStaysWithinTargetMajorVersion()
    {
        string root = CreateRoot("runtime-major");
        string expectedVersion4 = CreateRuntimeDirectory(root, new Version(4, 1, 10329, 0));
        string expectedVersion5 = CreateRuntimeDirectory(root, new Version(5, 1, 50918, 0));

        var provider = new MicrosoftSilverlightPathProvider([root]);

        var installation4 = GetInstallation(provider, new Version(4, 0));
        var installation5 = GetInstallation(provider, new Version(5, 0));

        Assert.Equal(expectedVersion4, installation4.RuntimeDirectory);
        Assert.Equal(expectedVersion5, installation5.RuntimeDirectory);
    }

    [Fact]
    public void RejectNullProgramFilesRoots() => Assert.Throws<ArgumentNullException>(() => new MicrosoftSilverlightPathProvider(null!));

    [Fact]
    public void DoesNotRollSilverlight40ForwardTo50()
    {
        string root = CreateRoot("no-roll-forward");

        CreateReferenceDirectory(root, new Version(5, 0));

        var provider = new MicrosoftSilverlightPathProvider([root]);

        Assert.False(provider.TryGetCompatibleInstallation(new Version(4, 0), out _));
        Assert.True(provider.TryGetCompatibleInstallation(new Version(5, 0), out _));
    }

    [Fact]
    public void DoesNotDetectUnsupportedSilverlightVersions()
    {
        string root = CreateRoot("unsupported");

        CreateReferenceDirectory(root, new Version(6, 0));

        var provider = new MicrosoftSilverlightPathProvider([root]);

        Assert.False(provider.TryGetCompatibleInstallation(new Version(6, 0), out _));
    }

    [Fact]
    public void CombinesLocationsFromDifferentProgramFilesRoots()
    {
        string root1 = CreateRoot("root1");
        string root2 = CreateRoot("root2");

        string referenceDirectory = CreateReferenceDirectory(root1, new Version(5, 0));
        string runtimeDirectory = CreateRuntimeDirectory(root2, new Version(5, 1, 50918, 0));

        var provider = new MicrosoftSilverlightPathProvider([root1, root2]);

        var installation = GetInstallation(provider, new Version(5, 0));

        Assert.Equal(referenceDirectory, installation.ReferenceAssemblyDirectory);
        Assert.Equal(runtimeDirectory, installation.RuntimeDirectory);
    }

    [Fact]
    public void SelectNewestRuntimeDirectoryForTargetMajorVersion()
    {
        string root = CreateRoot("runtime-selection");
        string expectedDirectory = CreateRuntimeDirectory(root, new Version(5, 1, 50918, 0));

        CreateRuntimeDirectory(root, new Version(5, 0, 61118, 0));
        CreateRuntimeDirectory(root, new Version(4, 1, 10329, 0));

        var provider = new MicrosoftSilverlightPathProvider([root]);

        var installation = GetInstallation(provider, new Version(5, 0));

        Assert.Equal(expectedDirectory, installation.RuntimeDirectory);
    }

    [Fact]
    public void IgnoreIncompleteReferenceAssemblyDirectory()
    {
        string root1 = CreateRoot("incomplete-reference-root1");
        string root2 = CreateRoot("incomplete-reference-root2");
        string expectedDirectory = CreateReferenceDirectory(root2, new Version(5, 0));

        CreateReferenceDirectory(root1, new Version(5, 0), includeCorLib: false);

        var provider = new MicrosoftSilverlightPathProvider([root1, root2]);

        var installation = GetInstallation(provider, new Version(5, 0));

        Assert.Equal(expectedDirectory, installation.ReferenceAssemblyDirectory);
    }

    [Fact]
    public void IgnoreIncompleteNewerRuntimeDirectory()
    {
        string root = CreateRoot("incomplete-runtime");
        string expectedDirectory = CreateRuntimeDirectory(root, new Version(5, 0, 61118, 0));

        CreateRuntimeDirectory(root, new Version(5, 1, 50918, 0), includeCorLib: false);

        var provider = new MicrosoftSilverlightPathProvider([root]);

        var installation = GetInstallation(provider, new Version(5, 0));

        Assert.Equal(expectedDirectory, installation.RuntimeDirectory);
    }

    private static SilverlightInstallation GetInstallation(
        MicrosoftSilverlightPathProvider provider,
        Version version)
    {
        Assert.True(provider.TryGetCompatibleInstallation(version, out var installation));
        return installation!;
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
