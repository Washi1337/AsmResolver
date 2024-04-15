using System.Collections.Concurrent;
using System.IO;
using AsmResolver.DotNet.Serialized;
using AsmResolver.DotNet.Signatures;

namespace AsmResolver.DotNet.Bundles;

/// <summary>
/// Provides an implementation of an assembly resolver that prefers assemblies embedded in single-file-host executable.
/// </summary>
public class BundleAssemblyResolver : IAssemblyResolver
{
    private readonly BundleManifest _manifest;
    private readonly DotNetCoreAssemblyResolver _baseResolver;
    private readonly ConcurrentDictionary<AssemblyDescriptor, AssemblyDefinition> _embeddedFilesCache = new(SignatureComparer.Default);

    internal BundleAssemblyResolver(BundleManifest manifest, ModuleReaderParameters readerParameters)
    {
        _manifest = manifest;

        // Bundles are .NET core 3.1+ only -> we can always default to .NET Core assembly resolution.
        _baseResolver = new DotNetCoreAssemblyResolver(readerParameters, manifest.GetTargetRuntime().Version);
    }

    /// <inheritdoc />
    public AssemblyDefinition? Resolve(AssemblyDescriptor assembly)
    {
        // Prefer embedded files before we forward to the default assembly resolution algorithm.
        if (TryResolveFromEmbeddedFiles(assembly, out var resolved))
            return resolved;

        return _baseResolver.Resolve(assembly);
    }

    private bool TryResolveFromEmbeddedFiles(AssemblyDescriptor assembly, out AssemblyDefinition? resolved)
    {
        if (_embeddedFilesCache.TryGetValue(assembly, out resolved))
            return true;

        try
        {
            for (int i = 0; i < _manifest.Files.Count; i++)
            {
                var file = _manifest.Files[i];
                if (file.Type != BundleFileType.Assembly)
                    continue;

                if (Path.GetFileNameWithoutExtension(file.RelativePath) == assembly.Name)
                {
                    resolved = AssemblyDefinition.FromBytes(file.GetData(), _baseResolver.ReaderParameters);
                    _embeddedFilesCache.TryAdd(assembly, resolved);
                    return true;
                }
            }
        }
        catch
        {
            // Ignore any reader errors.
        }

        resolved = null;
        return false;
    }

    /// <inheritdoc />
    public void AddToCache(AssemblyDescriptor descriptor, AssemblyDefinition definition)
    {
        _baseResolver.AddToCache(descriptor, definition);
    }

    /// <inheritdoc />
    public bool RemoveFromCache(AssemblyDescriptor descriptor)
    {
        // Note: This is intentionally not an or-else (||) construction.
        return _embeddedFilesCache.TryRemove(descriptor, out _) | _baseResolver.RemoveFromCache(descriptor);
    }

    /// <inheritdoc />
    public bool HasCached(AssemblyDescriptor descriptor)
    {
        return _embeddedFilesCache.ContainsKey(descriptor) || _baseResolver.HasCached(descriptor);
    }

    /// <inheritdoc />
    public void ClearCache()
    {
        _embeddedFilesCache.Clear();
        _baseResolver.ClearCache();
    }
}
