using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.DotNet.Bundles;
using AsmResolver.DotNet.Config.Json;
using AsmResolver.DotNet.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.IO;
using AsmResolver.PE;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.File;
using AsmResolver.PE.Platforms;

namespace AsmResolver.DotNet;

/// <summary>
/// Describes a context in which a .NET runtime is active.
/// </summary>
public partial class RuntimeContext
{
    private readonly Dictionary<AssemblyDescriptor, AssemblyDefinition> _loadedAssemblies;
    private readonly ConcurrentDictionary<ITypeDescriptor, TypeDefinition> _typeCache = new();

    /// <summary>
    /// Creates a new .NET (Core) runtime context.
    /// </summary>
    /// <param name="configuration">The configuration file to base the runtime context on.</param>
    /// <param name="sourceDirectory">The source directory to assume the main assemblies are stored in.</param>
    /// <param name="readerParameters">The parameters to use when reading modules in this context.</param>
    /// <param name="searchDirectories">Additional search directories to be added to the assembly resolution system.</param>
    public RuntimeContext(
        RuntimeConfiguration configuration,
        string? sourceDirectory = null,
        ModuleReaderParameters? readerParameters = null,
        IEnumerable<string>? searchDirectories = null)
    {
        if (!configuration.TryGetTargetRuntime(out var runtime))
            throw new ArgumentException("Could not infer target runtime from the runtime configuration.");
        if (!runtime.IsNetCoreApp)
            throw new ArgumentException("Runtime specified by the configuration is not a .NET or .NET Core runtime.");

        DefaultReaderParameters = readerParameters ?? new ModuleReaderParameters(new ByteArrayFileService());
        TargetRuntime = runtime;

        var resolver = new DotNetCoreAssemblyResolver(
            configuration,
            sourceDirectory: sourceDirectory,
            readerParameters: DefaultReaderParameters
        );
        AddSearchDirectories(resolver, searchDirectories);
        AssemblyResolver = resolver;

        RuntimeCorLib = TargetRuntime.GetAssumedImplCorLib();
        SignatureComparer = CreateSignatureComparer(TargetRuntime);
        _loadedAssemblies = new Dictionary<AssemblyDescriptor, AssemblyDefinition>(SignatureComparer);
    }

    /// <summary>
    /// Creates a new runtime context based on a PE image.
    /// </summary>
    /// <param name="image">The executable file to base the runtime context on.</param>
    /// <param name="readerParameters">The parameters to use when reading modules in this context.</param>
    /// <param name="searchDirectories">Additional search directories to be added to the assembly resolution system.</param>
    public RuntimeContext(
        PEImage image,
        ModuleReaderParameters? readerParameters = null,
        IEnumerable<string>? searchDirectories = null)
    {
        if (image.DotNetDirectory is null)
            throw new ArgumentException("PE does not have a .NET data directory.");

        DefaultReaderParameters = readerParameters ?? new ModuleReaderParameters(new ByteArrayFileService())
        {
            PEReaderParameters = image is SerializedPEImage serializedImage
                ? serializedImage.ReaderContext.Parameters
                : new PEReaderParameters(),
            WorkingDirectory = !string.IsNullOrEmpty(image.FilePath) ? Path.GetDirectoryName(image.FilePath) : null,
        };

        // If we cannot determine the target runtime the image was compiled against, assume .netfx 4.0 since it's
        // the most common case for standalone binaries.
        TargetRuntime = TargetRuntimeProber.TryGetLikelyTargetRuntime(image, out var originalTargetRuntime)
            ? originalTargetRuntime
            : DotNetRuntimeInfo.NetFramework(4, 0);

        // If this image was created from a file, check if we have a runtimeconfig.json file in the same directory
        // that we can use to determine the intended target runtime for.
        if (image.FilePath is { Length: > 0 } path
            && Path.ChangeExtension(path, ".runtimeconfig.json") is { } runtimeConfigPath
            && File.Exists(runtimeConfigPath))
        {
            try
            {
                var config = RuntimeConfiguration.FromFile(runtimeConfigPath);
                if (config?.TryGetTargetRuntime(out var runtime) is true)
                {
                    // This image is loaded using .NET (Core).
                    var resolver = new DotNetCoreAssemblyResolver(
                        config,
                        sourceDirectory: Path.GetFullPath(Path.GetDirectoryName(path)!),
                        fallbackVersion: TargetRuntime.IsNetCoreApp ? TargetRuntime.Version : null, // In case config is incomplete.
                        readerParameters: DefaultReaderParameters
                    );
                    AssemblyResolver = resolver;

                    // ReSharper disable once PossibleMultipleEnumeration
                    AddSearchDirectories(resolver, searchDirectories);

                    // Assume the runtimeconfig.json file is leading.
                    TargetRuntime = runtime;
                }
            }
            catch
            {
                // Assume config is corrupted.
            }
        }

        // If we failed to determine the target runtime and resolver, make a best effort guess.
        if (AssemblyResolver is null)
        {
            // 32-bit, 64-bit or AnyCPU?
            bool? is32Bit = null;
            if ((image.DotNetDirectory.Flags & DotNetDirectoryFlags.Bit32Required) != 0)
                is32Bit = true;
            else if ((image.DotNetDirectory.Flags & DotNetDirectoryFlags.ILOnly) == 0 && Platform.TryGet(image.MachineType, out var platform))
                is32Bit = platform.Is32Bit;

            // ReSharper disable once PossibleMultipleEnumeration
            AssemblyResolver = CreateAssemblyResolver(TargetRuntime, is32Bit, DefaultReaderParameters, searchDirectories);
        }

        RuntimeCorLib = TargetRuntime.GetAssumedImplCorLib();
        SignatureComparer = CreateSignatureComparer(TargetRuntime);
        _loadedAssemblies = new Dictionary<AssemblyDescriptor, AssemblyDefinition>(SignatureComparer);
    }

    /// <summary>
    /// Creates a new runtime context.
    /// </summary>
    /// <param name="targetRuntime">The target runtime version.</param>
    /// <param name="is32Bit"><c>true</c> if a 32-bit architecture is to be assumed, <c>false</c> if 64-bit, <c>null</c> if platform independent.</param>
    /// <param name="corLibReference">The core library for this runtime context, or the assumed one from the version if null.</param>
    /// <param name="readerParameters">The parameters to use when reading modules in this context.</param>
    /// <param name="searchDirectories">Additional search directories to be added to the assembly resolution system.</param>
    public RuntimeContext(
        DotNetRuntimeInfo targetRuntime,
        bool? is32Bit = null,
        AssemblyDescriptor? corLibReference = null,
        ModuleReaderParameters? readerParameters = null,
        IEnumerable<string>? searchDirectories = null)
    {
        DefaultReaderParameters = readerParameters ?? new ModuleReaderParameters(new ByteArrayFileService());

        TargetRuntime = targetRuntime;
        AssemblyResolver = CreateAssemblyResolver(TargetRuntime, is32Bit, DefaultReaderParameters, searchDirectories);
        RuntimeCorLib = corLibReference ?? targetRuntime.GetAssumedImplCorLib();

        SignatureComparer = CreateSignatureComparer(targetRuntime);
        _loadedAssemblies = new Dictionary<AssemblyDescriptor, AssemblyDefinition>(SignatureComparer);
    }

    /// <summary>
    /// Creates a new runtime context.
    /// </summary>
    /// <param name="targetRuntime">The target runtime version.</param>
    /// <param name="assemblyResolver">The assembly resolver to use when resolving assemblies into this context, or the default resolver if null.</param>
    /// <param name="corLibReference">The core library for this runtime context, or the assumed one from the version if null.</param>
    /// <param name="readerParameters">The parameters to use when reading modules in this context, or the default ones if null.</param>
    public RuntimeContext(
        DotNetRuntimeInfo targetRuntime,
        IAssemblyResolver assemblyResolver,
        AssemblyDescriptor? corLibReference = null,
        ModuleReaderParameters? readerParameters = null)
    {
        DefaultReaderParameters = readerParameters ?? new ModuleReaderParameters(new ByteArrayFileService());
        TargetRuntime = targetRuntime;
        AssemblyResolver = assemblyResolver;
        RuntimeCorLib = corLibReference ?? targetRuntime.GetAssumedImplCorLib();
        SignatureComparer = CreateSignatureComparer(targetRuntime);
        _loadedAssemblies = new Dictionary<AssemblyDescriptor, AssemblyDefinition>(SignatureComparer);
    }

    /// <summary>
    /// Creates a new runtime context.
    /// </summary>
    /// <param name="manifest">The bundle to create the runtime context for.</param>
    /// <param name="assemblyResolver">The assembly resolver to use when resolving assemblies into this context, or the default resolver if null.</param>
    /// <param name="readerParameters">The parameters to use when reading modules in this context.</param>
    public RuntimeContext(
        BundleManifest manifest,
        IAssemblyResolver? assemblyResolver = null,
        ModuleReaderParameters? readerParameters = null)
    {
        DefaultReaderParameters = readerParameters ?? new ModuleReaderParameters(new ByteArrayFileService());
        TargetRuntime = manifest.GetTargetRuntime();
        AssemblyResolver = new BundleAssemblyResolver(manifest, DefaultReaderParameters, assemblyResolver);
        SignatureComparer = CreateSignatureComparer(TargetRuntime);
        _loadedAssemblies = new Dictionary<AssemblyDescriptor, AssemblyDefinition>(SignatureComparer);

        if (ResolveAssembly(TargetRuntime.GetDefaultCorLib(), null, out var corlib) == ResolutionStatus.Success
            && corlib!.ManifestModule?.CorLibTypeFactory.Object.TryResolve(this, out var type) is true)
        {
            RuntimeCorLib = type.DeclaringModule?.Assembly;
        }
    }

    /// <summary>
    /// Gets the runtime version this context is targeting.
    /// </summary>
    public DotNetRuntimeInfo TargetRuntime
    {
        get;
    }

    /// <summary>
    /// Gets the default parameters that are used for reading .NET modules in the context.
    /// </summary>
    public ModuleReaderParameters DefaultReaderParameters
    {
        get;
    }

    /// <summary>
    /// Gets the assembly resolver that the context uses to resolve assemblies.
    /// </summary>
    public IAssemblyResolver AssemblyResolver
    {
        get;
    }

    /// <summary>
    /// Gets the corlib for this runtime
    /// </summary>
    public AssemblyDescriptor? RuntimeCorLib
    {
        get;
    }

    /// <summary>
    /// Gets the default signature comparer to use in this runtime context.
    /// </summary>
    public SignatureComparer SignatureComparer
    {
        get;
    }

    private static AssemblyResolverBase CreateAssemblyResolver(
        DotNetRuntimeInfo runtime,
        bool? is32Bit,
        ModuleReaderParameters readerParameters,
        IEnumerable<string>? searchDirectories)
    {
        AssemblyResolverBase resolver;
        switch (runtime.Name)
        {
            case DotNetRuntimeInfo.NetFrameworkName:
            case DotNetRuntimeInfo.NetStandardName when string.IsNullOrEmpty(DotNetCorePathProvider.DefaultInstallationPath):
                resolver = new DotNetFxAssemblyResolver(
                    runtime.Version,
                    is32Bit ?? IntPtr.Size == sizeof(uint),
                    DotNetFxPathProvider.Default,
                    readerParameters
                );
                break;

            case DotNetRuntimeInfo.NetStandardName when DotNetCorePathProvider.Default.TryGetLatestStandardCompatibleVersion(runtime.Version, out var coreVersion):
                resolver = new DotNetCoreAssemblyResolver(
                    coreVersion,
                    readerParameters: readerParameters
                );
                break;

            case DotNetRuntimeInfo.NetCoreAppName:
                resolver = new DotNetCoreAssemblyResolver(
                    runtime.Version,
                    readerParameters: readerParameters
                );
                break;

            case DotNetRuntimeInfo.SilverlightName:
                resolver = new SilverlightAssemblyResolver(
                    runtime.Version,
                    readerParameters: readerParameters
                );
                break;

            default:
                resolver = new DotNetFxAssemblyResolver(
                    runtime.Version,
                    is32Bit ?? IntPtr.Size == sizeof(uint),
                    DotNetFxPathProvider.Default,
                    readerParameters
                );
                break;
        }

        AddSearchDirectories(resolver, searchDirectories);

        return resolver;
    }

    private static void AddSearchDirectories(AssemblyResolverBase resolver, IEnumerable<string>? searchDirectories)
    {
        if (searchDirectories is not null)
        {
            foreach (string path in searchDirectories)
                resolver.SearchDirectories.Add(path);
        }
    }

    private SignatureComparer CreateSignatureComparer(DotNetRuntimeInfo targetRuntime)
    {
        var flags = SignatureComparisonFlags.VersionAgnostic;
        if (targetRuntime.IsNetCoreApp)
            flags |= SignatureComparisonFlags.IgnoreStrongNames;
        return new SignatureComparer(this, flags);
    }

    /// <summary>
    /// Loads a .NET assembly into the context from the provided input buffer.
    /// </summary>
    /// <param name="buffer">The raw contents of the executable file to load.</param>
    /// <returns>The assembly.</returns>
    /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET metadata directory.</exception>
    public AssemblyDefinition LoadAssembly(byte[] buffer)
        => GetOrAddAssembly(AssemblyDefinition.FromBytes(buffer, readerParameters: DefaultReaderParameters, createRuntimeContext: false));

    /// <summary>
    /// Loads a .NET assembly into the context from the provided input stream.
    /// </summary>
    /// <param name="stream">The raw contents of the executable file to load.</param>
    /// <returns>The assembly.</returns>
    /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET metadata directory.</exception>
    public AssemblyDefinition LoadAssembly(Stream stream)
        => GetOrAddAssembly(AssemblyDefinition.FromStream(stream, readerParameters: DefaultReaderParameters, createRuntimeContext: false));

    /// <summary>
    /// Loads a .NET assembly into the context from the provided input file.
    /// </summary>
    /// <param name="filePath">The file path to the input executable to load.</param>
    /// <returns>The assembly.</returns>
    /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET metadata directory.</exception>
    public AssemblyDefinition LoadAssembly(string filePath)
        => GetOrAddAssembly(AssemblyDefinition.FromFile(filePath, readerParameters: DefaultReaderParameters, createRuntimeContext: false));

    /// <summary>
    /// Loads a .NET assembly into the context from the provided input file.
    /// </summary>
    /// <param name="file">The portable executable file to load.</param>
    /// <returns>The assembly.</returns>
    /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET metadata directory.</exception>
    public AssemblyDefinition LoadAssembly(PEFile file)
        => GetOrAddAssembly(AssemblyDefinition.FromFile(file, readerParameters: DefaultReaderParameters, createRuntimeContext: false));

    /// <summary>
    /// Loads a .NET assembly into the context from the provided input file.
    /// </summary>
    /// <param name="file">The portable executable file to load.</param>
    /// <returns>The assembly.</returns>
    /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET metadata directory.</exception>
    public AssemblyDefinition LoadAssembly(IInputFile file)
        => GetOrAddAssembly(AssemblyDefinition.FromFile(file, readerParameters: DefaultReaderParameters, createRuntimeContext: false));

    /// <summary>
    /// Loads a .NET assembly into the context from an input stream.
    /// </summary>
    /// <param name="reader">The input stream pointing at the beginning of the executable to load.</param>
    /// <param name="mode">Indicates the input PE is mapped or unmapped.</param>
    /// <returns>The assembly.</returns>
    /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET metadata directory.</exception>
    public AssemblyDefinition LoadAssembly(in BinaryStreamReader reader, PEMappingMode mode = PEMappingMode.Unmapped)
        => GetOrAddAssembly(AssemblyDefinition.FromReader(reader, mode: mode, readerParameters: DefaultReaderParameters, createRuntimeContext: false));

    /// <summary>
    /// Loads a .NET assembly into the context from a PE image.
    /// </summary>
    /// <param name="peImage">The image containing the .NET metadata.</param>
    /// <returns>The assembly.</returns>
    /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET metadata directory.</exception>
    public AssemblyDefinition LoadAssembly(PEImage peImage)
        => GetOrAddAssembly(AssemblyDefinition.FromImage(peImage, readerParameters: DefaultReaderParameters, createRuntimeContext: false));

    /// <summary>
    /// Loads a .NET assembly into the context from a file in an app-host bundle.
    /// </summary>
    /// <param name="file">The file containing the .NET metadata.</param>
    /// <returns>The assembly.</returns>
    /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET metadata directory.</exception>
    public AssemblyDefinition LoadAssembly(BundleFile file)
        => GetOrAddAssembly(AssemblyDefinition.FromBytes(file.GetData(), readerParameters: DefaultReaderParameters, createRuntimeContext: false));

    private static void AssertNoOwner(AssemblyDefinition assembly)
    {
        if (assembly.RuntimeContext is not null)
            throw new ArgumentException($"Assembly {assembly.SafeToString()} is already added to another context.");
    }

    /// <summary>
    /// Registers an assembly in the context.
    /// </summary>
    /// <param name="assembly">The assembly to add</param>
    /// <exception cref="ArgumentException">
    /// Occurs when the assembly was already added to another context, or when there already exists an assembly with
    /// the same name in the context.
    /// </exception>
    public void AddAssembly(AssemblyDefinition assembly)
    {
        lock (_loadedAssemblies)
        {
            AssertNoOwner(assembly);

            if (_loadedAssemblies.ContainsKey(assembly))
                throw new ArgumentException($"Assembly '{assembly.SafeToString()}' is already present in this context.", nameof(assembly));

            _loadedAssemblies.Add(assembly, assembly);
            assembly.RuntimeContext = this;
        }
    }

    private AssemblyDefinition GetOrAddAssembly(AssemblyDefinition assembly)
    {
        lock (_loadedAssemblies)
        {
            if (_loadedAssemblies.TryGetValue(assembly, out var resolved))
                return resolved;

            AddAssembly(assembly);
            return assembly;
        }
    }

    /// <summary>
    /// Enumerates all assemblies that were loaded in the context.
    /// </summary>
    /// <returns>The assemblies.</returns>
    public IEnumerable<AssemblyDefinition> GetLoadedAssemblies()
    {
        lock (_loadedAssemblies)
        {
            return _loadedAssemblies.Values.ToArray();
        }
    }
}
