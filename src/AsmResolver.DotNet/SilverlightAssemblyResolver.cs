using System;
using AsmResolver.DotNet.Serialized;
using AsmResolver.IO;

namespace AsmResolver.DotNet;

/// <summary>
/// Provides an implementation of an assembly resolver that includes Microsoft Silverlight runtime or reference
/// libraries, as well as any custom search directories.
/// </summary>
public class SilverlightAssemblyResolver : AssemblyResolverBase
{
    private readonly SilverlightInstallation? _installation;

    /// <summary>
    /// Creates a new Silverlight assembly resolver.
    /// </summary>
    public SilverlightAssemblyResolver(
        Version runtimeVersion,
        bool is32Bit,
        SilverlightPathProvider? pathProvider = null,
        ModuleReaderParameters? readerParameters = null)
        : base(readerParameters ?? new ModuleReaderParameters(UncachedFileService.Instance))
    {
        pathProvider ??= SilverlightPathProvider.Default;
        if (!pathProvider.TryGetCompatibleRuntime(runtimeVersion, is32Bit, out _installation))
            pathProvider.TryGetCompatibleReferenceRuntime(runtimeVersion, is32Bit, out _installation);
    }

    /// <summary>
    /// Creates a new Silverlight assembly resolver.
    /// </summary>
    public SilverlightAssemblyResolver(
        SilverlightInstallation installation,
        ModuleReaderParameters? readerParameters = null)
        : base(readerParameters ?? new ModuleReaderParameters(UncachedFileService.Instance))
    {
        _installation = installation ?? throw new ArgumentNullException(nameof(installation));
    }

    /// <inheritdoc />
    public override string? ProbeAssemblyFilePath(AssemblyDescriptor assembly, ModuleDefinition? originModule)
    {
        return ProbeInstallationDirectories(assembly)
            ?? ProbeSearchDirectories(assembly, originModule);
    }

    private string? ProbeInstallationDirectories(AssemblyDescriptor assembly)
    {
        if (_installation is null)
            return null;

        if (ProbeDirectory(assembly, _installation.InstallDirectory) is { } installationPath)
            return installationPath;

        if (_installation.SdkLibraryDirectory is { } sdkLibraryDirectory
            && ProbeDirectory(assembly, sdkLibraryDirectory) is { } sdkLibraryPath)
            return sdkLibraryPath;

        return null;
    }
}
