using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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

    private readonly SilverlightInstallation[] _installations;

    /// <summary>
    /// Creates a new Silverlight path provider using the default Program Files locations on the current system.
    /// </summary>
    public MicrosoftSilverlightPathProvider()
        : this(GetDefaultProgramFilesDirectories())
    {
    }

    /// <summary>
    /// Creates a new Silverlight path provider using the provided Program Files roots.
    /// </summary>
    /// <param name="programFilesDirectories">The Program Files roots to inspect.</param>
    public MicrosoftSilverlightPathProvider(IEnumerable<string> programFilesDirectories)
    {
        if (programFilesDirectories is null)
            throw new ArgumentNullException(nameof(programFilesDirectories));

        _installations = DetectInstallations(programFilesDirectories);
    }

    /// <summary>
    /// Gets the singleton instance of the <see cref="MicrosoftSilverlightPathProvider"/> class.
    /// </summary>
    public static MicrosoftSilverlightPathProvider Instance { get; } = new();

    /// <inheritdoc />
    public override bool TryGetCompatibleInstallation(
        Version version,
        [NotNullWhen(true)] out SilverlightInstallation? installation)
    {
        for (int i = 0; i < _installations.Length; i++)
        {
            var candidate = _installations[i];

            if (candidate.Version.Major != version.Major || candidate.Version.Minor != version.Minor)
                continue;

            installation = candidate;
            return true;
        }

        installation = null;
        return false;
    }

    private static SilverlightInstallation[] DetectInstallations(IEnumerable<string> programFilesDirectories)
    {
        string[] roots = GetDistinctDirectories(programFilesDirectories);
        var result = new List<SilverlightInstallation>();

        for (int i = 0; i < KnownVersions.Length; i++)
        {
            var version = KnownVersions[i];
            string? referenceAssemblyDirectory = FindReferenceAssemblyDirectory(roots, version);
            string? sdkLibraryDirectory = FindSdkLibraryDirectory(roots, version);
            string? runtimeDirectory = FindRuntimeDirectory(roots, version);

            if (referenceAssemblyDirectory is null
                && sdkLibraryDirectory is null
                && runtimeDirectory is null)
                continue;

            result.Add(new SilverlightInstallation(
                version,
                referenceAssemblyDirectory,
                sdkLibraryDirectory,
                runtimeDirectory
            ));
        }

        return result.ToArray();
    }

    private static string[] GetDistinctDirectories(IEnumerable<string> directories)
    {
        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string directory in directories)
        {
            if (!string.IsNullOrEmpty(directory) && seen.Add(directory))
                result.Add(directory);
        }

        return result.ToArray();
    }

    private static string? FindReferenceAssemblyDirectory(IList<string> roots, Version version)
    {
        string versionDirectory = $"v{version.Major}.{version.Minor}";

        for (int i = 0; i < roots.Count; i++)
        {
            string path = PathShim.Combine(
                PathShim.Combine(
                    PathShim.Combine(roots[i], "Reference Assemblies", "Microsoft"),
                    "Framework",
                    "Silverlight"
                ),
                versionDirectory
            );

            if (File.Exists(Path.Combine(path, "mscorlib.dll")))
                return path;
        }

        return null;
    }

    private static string? FindSdkLibraryDirectory(IList<string> roots, Version version)
    {
        string versionDirectory = $"v{version.Major}.{version.Minor}";

        for (int i = 0; i < roots.Count; i++)
        {
            string path = PathShim.Combine(
                PathShim.Combine(
                    PathShim.Combine(roots[i], "Microsoft SDKs", "Silverlight"),
                    versionDirectory,
                    "Libraries"
                ),
                "Client"
            );

            if (Directory.Exists(path))
                return path;
        }

        return null;
    }

    private static string? FindRuntimeDirectory(IList<string> roots, Version version)
    {
        Version? bestVersion = null;
        string? bestDirectory = null;

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

                if (!VersionShim.TryParse(directoryName, out var candidateVersion))
                    continue;

                if (candidateVersion.Major != version.Major)
                    continue;

                if (!File.Exists(Path.Combine(directory, "mscorlib.dll")))
                    continue;

                if (bestVersion is not null && candidateVersion <= bestVersion)
                    continue;

                bestVersion = candidateVersion;
                bestDirectory = directory;
            }
        }

        return bestDirectory;
    }

    private static IEnumerable<string> GetDefaultProgramFilesDirectories()
    {
        string? programFilesX86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
        if (!string.IsNullOrEmpty(programFilesX86))
            yield return programFilesX86;

        string? programFilesNative = Environment.GetEnvironmentVariable("ProgramW6432");
        if (!string.IsNullOrEmpty(programFilesNative))
            yield return programFilesNative;

        string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        if (!string.IsNullOrEmpty(programFiles))
            yield return programFiles;

        string? programFilesEnvironment = Environment.GetEnvironmentVariable("ProgramFiles");
        if (!string.IsNullOrEmpty(programFilesEnvironment))
            yield return programFilesEnvironment;
    }
}
