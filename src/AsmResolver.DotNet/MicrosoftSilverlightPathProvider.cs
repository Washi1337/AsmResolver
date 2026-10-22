using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using AsmResolver.Shims;

namespace AsmResolver.DotNet;

/// <summary>
/// Provides a mechanism for locating installations of Microsoft Silverlight on a Windows machine.
/// </summary>
public sealed class MicrosoftSilverlightPathProvider : SilverlightPathProvider
{
    private static readonly Version[] KnownVersions = [
        new(5, 0),
        new(4, 0),
    ];

    private readonly SilverlightInstallation[] _runtimes32;
    private readonly SilverlightInstallation[] _runtimes64;
    private readonly SilverlightInstallation[] _referenceRuntimes32;
    private readonly SilverlightInstallation[] _referenceRuntimes64;

    /// <summary>
    /// Creates a new Silverlight path provider using the default Program Files directories.
    /// </summary>
    public MicrosoftSilverlightPathProvider()
        : this(GetDefaultProgramFiles32BitDirectories(), GetDefaultProgramFiles64BitDirectories())
    {
    }

    /// <summary>
    /// Creates a new Silverlight path provider using the provided Program Files directories.
    /// </summary>
    /// <param name="programFiles32BitDirectories">The collection of 32-bit Program Files directories to consider.</param>
    /// <param name="programFiles64BitDirectories">The collection of 64-bit Program Files directories to consider.</param>
    public MicrosoftSilverlightPathProvider(
        IEnumerable<string> programFiles32BitDirectories,
        IEnumerable<string> programFiles64BitDirectories)
    {
        if (programFiles32BitDirectories is null)
            throw new ArgumentNullException(nameof(programFiles32BitDirectories));
        if (programFiles64BitDirectories is null)
            throw new ArgumentNullException(nameof(programFiles64BitDirectories));

        string[] roots32 = programFiles32BitDirectories
            .Where(d => !string.IsNullOrEmpty(d))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        string[] roots64 = programFiles64BitDirectories
            .Where(d => !string.IsNullOrEmpty(d))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        _runtimes32 = DetectRuntimeInstallations(roots32);
        _runtimes64 = DetectRuntimeInstallations(roots64);
        _referenceRuntimes32 = DetectReferenceInstallations(roots32);
        _referenceRuntimes64 = DetectReferenceInstallations(roots64);
    }

    /// <summary>
    /// Gets the singleton instance of the <see cref="MicrosoftSilverlightPathProvider"/> class.
    /// </summary>
    public static MicrosoftSilverlightPathProvider Instance { get; } = new();

    /// <inheritdoc />
    public override bool TryGetCompatibleRuntime(
        Version version,
        bool is32Bit,
        [NotNullWhen(true)] out SilverlightInstallation? runtime)
    {
        var candidates = is32Bit ? _runtimes32 : _runtimes64;

        for (int i = 0; i < candidates.Length; i++)
        {
            var candidate = candidates[i];
            if (candidate.Version.Major != version.Major)
                continue;

            runtime = candidate;
            return true;
        }

        runtime = null;
        return false;
    }

    /// <inheritdoc />
    public override bool TryGetCompatibleReferenceRuntime(
        Version version,
        bool is32Bit,
        [NotNullWhen(true)] out SilverlightInstallation? runtime)
    {
        var preferredCandidates = is32Bit ? _referenceRuntimes32 : _referenceRuntimes64;
        var fallbackCandidates = is32Bit ? _referenceRuntimes64 : _referenceRuntimes32;

        return TryGetCompatibleReferenceRuntime(preferredCandidates, version, out runtime)
            || TryGetCompatibleReferenceRuntime(fallbackCandidates, version, out runtime);
    }

    private static bool TryGetCompatibleReferenceRuntime(
        IList<SilverlightInstallation> candidates,
        Version version,
        [NotNullWhen(true)] out SilverlightInstallation? runtime)
    {
        for (int i = 0; i < candidates.Count; i++)
        {
            var candidate = candidates[i];
            if (candidate.Version.Major != version.Major || candidate.Version.Minor != version.Minor)
                continue;

            runtime = candidate;
            return true;
        }

        runtime = null;
        return false;
    }

    private static SilverlightInstallation[] DetectRuntimeInstallations(IList<string> roots)
    {
        var result = new List<SilverlightInstallation>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < roots.Count; i++)
        {
            string silverlightDirectory = Path.Combine(roots[i], "Microsoft Silverlight");
            if (!Directory.Exists(silverlightDirectory))
                continue;

            string[] directories;

            try
            {
                directories = Directory.GetDirectories(silverlightDirectory);
            }
            catch (IOException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            for (int j = 0; j < directories.Length; j++)
            {
                string directory = directories[j];
                string directoryName = Path.GetFileName(directory);

                if (!VersionShim.TryParse(directoryName, out var version)
                    || !IsKnownVersion(version)
                    || !File.Exists(Path.Combine(directory, "mscorlib.dll"))
                    || !seen.Add(directory))
                    continue;

                result.Add(new SilverlightInstallation(version, directory));
            }
        }

        result.Sort((x, y) => y.Version.CompareTo(x.Version));
        return result.ToArray();
    }

    private static SilverlightInstallation[] DetectReferenceInstallations(IList<string> roots)
    {
        var result = new List<SilverlightInstallation>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < KnownVersions.Length; i++)
        {
            var version = KnownVersions[i];

            for (int j = 0; j < roots.Count; j++)
            {
                string? referenceAssemblyDirectory = FindReferenceAssemblyDirectory(roots[j], version);
                string? sdkLibraryDirectory = FindSdkLibraryDirectory(roots[j], version);

                string? installDirectory = referenceAssemblyDirectory ?? sdkLibraryDirectory;
                if (installDirectory is null || !seen.Add(installDirectory))
                    continue;

                result.Add(new SilverlightInstallation(
                    version,
                    installDirectory,
                    referenceAssemblyDirectory is not null ? sdkLibraryDirectory : null
                ));
            }
        }

        result.Sort((x, y) => y.Version.CompareTo(x.Version));
        return result.ToArray();
    }

    private static bool IsKnownVersion(Version version)
    {
        for (int i = 0; i < KnownVersions.Length; i++)
        {
            if (KnownVersions[i].Major == version.Major)
                return true;
        }

        return false;
    }

    private static string? FindReferenceAssemblyDirectory(string root, Version version)
    {
        string path = PathShim.Combine(
            PathShim.Combine(
                PathShim.Combine(root, "Reference Assemblies", "Microsoft"),
                "Framework",
                "Silverlight"
            ),
            $"v{version.Major}.{version.Minor}"
        );

        return File.Exists(Path.Combine(path, "mscorlib.dll")) ? path : null;
    }

    private static string? FindSdkLibraryDirectory(string root, Version version)
    {
        string path = PathShim.Combine(
            PathShim.Combine(
                PathShim.Combine(root, "Microsoft SDKs", "Silverlight"),
                $"v{version.Major}.{version.Minor}",
                "Libraries"
            ),
            "Client"
        );

        return Directory.Exists(path) ? path : null;
    }

    private static IEnumerable<string> GetDefaultProgramFiles32BitDirectories()
    {
        string? programFilesX86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
        if (!string.IsNullOrEmpty(programFilesX86))
            yield return programFilesX86;
    }

    private static IEnumerable<string> GetDefaultProgramFiles64BitDirectories()
    {
        string? programFilesX64 = Environment.GetEnvironmentVariable("ProgramW6432");
        if (!string.IsNullOrEmpty(programFilesX64))
            yield return programFilesX64;
    }
}
