using System;

namespace AsmResolver.DotNet;

/// <summary>
/// Describes a Microsoft Silverlight installation.
/// </summary>
/// <param name="version">The version of the target framework.</param>
/// <param name="referenceAssemblyDirectory">The directory containing the Silverlight reference assemblies.</param>
/// <param name="sdkLibraryDirectory">The directory containing the Silverlight SDK client libraries.</param>
/// <param name="runtimeDirectory">The directory containing the installed Silverlight runtime assemblies.</param>
public sealed class SilverlightInstallation(
    Version version,
    string? referenceAssemblyDirectory,
    string? sdkLibraryDirectory,
    string? runtimeDirectory
)
{
    /// <summary>
    /// Gets the version of the target framework.
    /// </summary>
    public Version Version { get; } = version;

    /// <summary>
    /// When available, gets the directory containing the Silverlight reference assemblies.
    /// </summary>
    public string? ReferenceAssemblyDirectory { get; } = referenceAssemblyDirectory;

    /// <summary>
    /// When available, gets the directory containing the Silverlight SDK client libraries.
    /// </summary>
    public string? SdkLibraryDirectory { get; } = sdkLibraryDirectory;

    /// <summary>
    /// When available, gets the directory containing the installed Silverlight runtime assemblies.
    /// </summary>
    public string? RuntimeDirectory { get; } = runtimeDirectory;
}
