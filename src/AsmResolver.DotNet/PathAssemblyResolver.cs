using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.DotNet.Serialized;

namespace AsmResolver.DotNet;

/// <summary>
/// Provides an implementation of an assembly resolver that resolves assemblies from a provided set of file paths.
/// </summary>
public sealed class PathAssemblyResolver : IAssemblyResolver
{
    private readonly Dictionary<string, string> _simpleNameMap = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Creates a new path assembly resolver.
    /// </summary>
    /// <param name="referencePath">The set of file paths used when resolving assemblies.</param>
    /// <param name="readerParameters">The reader parameters used for reading new resolved assemblies.</param>
    public PathAssemblyResolver(IEnumerable<string> referencePath, ModuleReaderParameters? readerParameters = null)
    {
        var paths = referencePath as ICollection<string> ?? referencePath.ToArray();

        foreach (string extension in AssemblyResolverBase.BinaryFileExtensions)
        {
            foreach (string path in paths)
            {
                if (!path.EndsWith(extension, StringComparison.OrdinalIgnoreCase) || !File.Exists(path))
                    continue;

                string simpleName = Path.GetFileNameWithoutExtension(path);
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                _simpleNameMap.TryAdd(simpleName, path);
#else
                if (!_simpleNameMap.ContainsKey(simpleName))
                {
                    _simpleNameMap.Add(simpleName, path);
                }
#endif
            }
        }

        ReaderParameters = readerParameters;
    }

    /// <summary>
    /// Creates a new path assembly resolver from provided <paramref name="searchDirectories"/>.
    /// </summary>
    /// <param name="searchDirectories">The search directories used to construct the reference path.</param>
    /// <param name="readerParameters">The reader parameters used for reading new resolved assemblies.</param>
    /// <remarks>The directories are traversed at the time of this call, so any later filesystem changes won't be reflected.</remarks>
    public static PathAssemblyResolver FromSearchDirectories(IEnumerable<string> searchDirectories, ModuleReaderParameters? readerParameters = null)
    {
        var referencePath = new List<string>();
        foreach (string directory in searchDirectories)
        {
            if (!Directory.Exists(directory))
                continue;

            referencePath.AddRange(Directory.GetFiles(directory));
        }

        return new PathAssemblyResolver(referencePath, readerParameters);
    }

    /// <summary>
    /// Gets the reader parameters used for reading new resolved assemblies.
    /// </summary>
    public ModuleReaderParameters? ReaderParameters { get; }

    /// <inheritdoc />
    public ResolutionStatus Resolve(AssemblyDescriptor assembly, ModuleDefinition? originModule, out AssemblyDefinition? result)
    {
        if (assembly.Name is null || !_simpleNameMap.TryGetValue(assembly.Name, out string? path))
        {
            result = null;
            return ResolutionStatus.AssemblyNotFound;
        }

        try
        {
            result = AssemblyDefinition.FromFile(path, readerParameters: ReaderParameters, createRuntimeContext: false);
            return ResolutionStatus.Success;
        }
        catch (BadImageFormatException)
        {
            result = null;
            return ResolutionStatus.AssemblyBadImage;
        }
    }
}
