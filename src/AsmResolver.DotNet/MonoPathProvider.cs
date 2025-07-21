using System;
using System.IO;
using System.Linq;
using AsmResolver.Shims;

namespace AsmResolver.DotNet;

/// <summary>
/// Provides a mechanism for looking up runtime libraries in a Mono installation folder.
/// </summary>
public class MonoPathProvider
{
    private static readonly string[] DefaultMonoWindowsPaths = [
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Mono")
    ];

    private static readonly string[] DefaultMonoUnixPaths = [
        "/usr/lib/mono",
        "/lib/mono"
    ];

    static MonoPathProvider()
    {
        DefaultInstallationPath = FindMonoPath();
        Default = new MonoPathProvider();
    }

    /// <summary>
    /// Creates a mono path provider using the default Mono installation path detected on the system.
    /// </summary>
    public MonoPathProvider()
        : this(DefaultInstallationPath)
    {
    }

    /// <summary>
    /// Creates a mono path provider using the provided Mono installation path.
    /// </summary>
    public MonoPathProvider(string? installDirectory)
    {
        if (installDirectory is not null && Directory.Exists(installDirectory))
            DetectInstalledRuntimes(installDirectory);
    }

    /// <summary>
    /// Gets the path to the Mono installation on the current system.
    /// </summary>
    public static string? DefaultInstallationPath
    {
        get;
    }

    /// <summary>
    /// Gets the default path provider representing the global Mono installation on the current system.
    /// </summary>
    public static MonoPathProvider Default
    {
        get;
    }

    /// <summary>
    /// When available, gets the path to the Mono GAC directory.
    /// </summary>
    public string? GacDirectory
    {
        get;
        private set;
    }

    /// <summary>
    /// When available, gets the path to the Mono reference API directory.
    /// </summary>
    public string? ApiDirectory
    {
        get;
        private set;
    }

    /// <summary>
    /// When available, gets the path to the Mono reference facades directory.
    /// </summary>
    public string? FacadesDirectory
    {
        get;
        private set;
    }

    private static string? FindMonoPath()
    {
        if (RuntimeInformationShim.IsRunningOnWindows)
            return FindWindowsMonoPath();

        if (RuntimeInformationShim.IsRunningOnUnix)
            return FindUnixMonoPath();

        return null;
    }

    private static string? FindWindowsMonoPath()
    {
        // Try common windows installs.
        foreach (string knownPath in DefaultMonoWindowsPaths)
        {
            if (Directory.Exists(knownPath))
                return knownPath;
        }

        return null;
    }

    private static string? FindUnixMonoPath()
    {
        // Try common unix installs first.
        foreach (string knownPath in DefaultMonoUnixPaths)
        {
            if (Directory.Exists(knownPath))
                return knownPath;
        }

        // If we're running on nix, we need to get it from the nix package.
        if (Directory.Exists("/nix/store") && FindNixMonoPath() is { } nixPath)
            return nixPath;

        return null;
    }

    private static string? FindNixMonoPath()
    {
        // Probe "mono" from PATH. It will be in "<nix-mono-root>/bin/mono".
        // The stdlib libraries are then located in "<nix-mono-root>/lib/mono"
        string[]? paths = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator);
        if (paths is null)
            return null;

        foreach (string path in paths)
        {
            string candidateMonoBinaryPath = Path.Combine(path, "mono");
            if (File.Exists(candidateMonoBinaryPath)
                && NativeMethods.RealPath(candidateMonoBinaryPath) is { } binaryPath
                && Path.GetDirectoryName(Path.GetDirectoryName(binaryPath)) is { } rootPath
                && PathShim.Combine(rootPath, "lib", "mono") is { } installPath
                && Directory.Exists(installPath))
            {
                return installPath;
            }
        }

        return null;
    }

    private void DetectInstalledRuntimes(string installDirectory)
    {
        string gac = Path.Combine(installDirectory, "gac");
        if (Directory.Exists(gac))
            GacDirectory = gac;

        string? mostRecentMonoDirectory = Directory
            .GetDirectories(installDirectory)
            .Where(d => d.EndsWith("-api"))
            .OrderByDescending(x => x)
            .FirstOrDefault();

        if (mostRecentMonoDirectory is not null)
        {
            ApiDirectory = mostRecentMonoDirectory;
            string facadesDirectory = Path.Combine(mostRecentMonoDirectory, "Facades");
            if (Directory.Exists(facadesDirectory))
                FacadesDirectory = facadesDirectory;
        }
    }
}
