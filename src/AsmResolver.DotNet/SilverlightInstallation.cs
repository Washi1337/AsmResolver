using System;

namespace AsmResolver.DotNet;

/// <summary>
/// Describes a Microsoft Silverlight installation.
/// </summary>
/// <param name="version">The version of the runtime.</param>
/// <param name="installDirectory">The base directory all libraries are installed in.</param>
/// <param name="sdkLibraryDirectory">The base directory all SDK libraries are installed in (when available).</param>
public sealed class SilverlightInstallation(
    Version version,
    string installDirectory,
    string? sdkLibraryDirectory = null
)
{
    /// <summary>
    /// Gets the version of the runtime.
    /// </summary>
    public Version Version { get; } = version;

    /// <summary>
    /// Gets the base directory all libraries are installed in.
    /// </summary>
    public string InstallDirectory { get; } = installDirectory;

    /// <summary>
    /// When available, gets the base directory all SDK libraries are installed in.
    /// </summary>
    public string? SdkLibraryDirectory { get; } = sdkLibraryDirectory;
}
