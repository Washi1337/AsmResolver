using System;
using AsmResolver.DotNet.Serialized;
using AsmResolver.IO;

namespace AsmResolver.DotNet;

/// <summary>
/// Provides an implementation of an assembly resolver that includes Microsoft Silverlight runtime and reference
/// libraries, as well as any custom search directories.
/// </summary>
public class SilverlightAssemblyResolver : AssemblyResolverBase
{
    private readonly SilverlightInstallation? _installation;

    /// <summary>
    /// Creates a new Silverlight assembly resolver.
    /// </summary>
    /// <param name="runtimeVersion">The version of Silverlight to target.</param>
    /// <param name="pathProvider">
    /// The assumed system installation provider of Silverlight, or <c>null</c> to use the default path provider.
    /// </param>
    /// <param name="readerParameters">
    /// The parameters to use while reading assemblies, or <c>null</c> to use the default reader parameters.
    /// </param>
    public SilverlightAssemblyResolver(
        Version runtimeVersion,
        SilverlightPathProvider? pathProvider = null,
        ModuleReaderParameters? readerParameters = null)
        : base(readerParameters ?? new ModuleReaderParameters(UncachedFileService.Instance))
    {
        pathProvider ??= SilverlightPathProvider.Default;
        pathProvider.TryGetCompatibleInstallation(runtimeVersion, out _installation);
    }

    /// <summary>
    /// Creates a new Silverlight assembly resolver using the provided installation.
    /// </summary>
    /// <param name="installation">The Silverlight installation to use.</param>
    /// <param name="readerParameters">
    /// The parameters to use while reading assemblies, or <c>null</c> to use the default reader parameters.
    /// </param>
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

        if (_installation.ReferenceAssemblyDirectory is { } referenceAssemblyDirectory
            && ProbeDirectory(assembly, referenceAssemblyDirectory) is { } referenceAssemblyPath)
            return referenceAssemblyPath;

        if (_installation.SdkLibraryDirectory is { } sdkLibraryDirectory
            && ProbeDirectory(assembly, sdkLibraryDirectory) is { } sdkLibraryPath)
            return sdkLibraryPath;

        if (_installation.RuntimeDirectory is { } runtimeDirectory
            && ProbeDirectory(assembly, runtimeDirectory) is { } runtimePath)
            return runtimePath;

        return null;
    }
}
