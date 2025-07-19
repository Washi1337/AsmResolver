using System;
using System.Reflection;
using AsmResolver.DotNet.Bundles;
using AsmResolver.DotNet.Serialized;
using AsmResolver.IO;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Describes a context in which a .NET runtime is active.
    /// </summary>
    public class RuntimeContext
    {
        /// <summary>
        /// Creates a new runtime context.
        /// </summary>
        /// <param name="targetRuntime">The target runtime version.</param>
        public RuntimeContext(DotNetRuntimeInfo targetRuntime)
            : this(targetRuntime, null, null, null)
        {
        }

        /// <summary>
        /// Creates a new runtime context.
        /// </summary>
        /// <param name="targetRuntime">The target runtime version.</param>
        /// <param name="readerParameters">The parameters to use when reading modules in this context.</param>
        public RuntimeContext(DotNetRuntimeInfo targetRuntime, ModuleReaderParameters readerParameters) : this(targetRuntime, null, null, readerParameters)
        {
        }

        /// <summary>
        /// Creates a new runtime context.
        /// </summary>
        /// <param name="targetRuntime">The target runtime version.</param>
        /// <param name="assemblyResolver">The assembly resolver to use when resolving assemblies into this context.</param>
        public RuntimeContext(DotNetRuntimeInfo targetRuntime, IAssemblyResolver assemblyResolver) : this(targetRuntime, assemblyResolver, null, null)
        {
        }

        /// <summary>
        /// Creates a new runtime context.
        /// </summary>
        /// <param name="targetRuntime">The target runtime version.</param>
        /// <param name="assemblyResolver">The assembly resolver to use when resolving assemblies into this context, or the default resolver if null.</param>
        /// <param name="corLibReference">The core library for this runtime context, or the assumed one from the version if null.</param>
        /// <param name="readerParameters">The parameters to use when reading modules in this context, or the default ones if null.</param>
        public RuntimeContext(DotNetRuntimeInfo targetRuntime, IAssemblyResolver? assemblyResolver, AssemblyDescriptor? corLibReference, ModuleReaderParameters? readerParameters)
        {
            DefaultReaderParameters = readerParameters is not null
                ? new ModuleReaderParameters(readerParameters) { RuntimeContext = this }
                : new ModuleReaderParameters(new ByteArrayFileService()) { RuntimeContext = this };

            TargetRuntime = targetRuntime;
            AssemblyResolver = assemblyResolver ?? CreateAssemblyResolver(targetRuntime, DefaultReaderParameters);
            RuntimeCorLib = corLibReference ?? targetRuntime.GetAssumedImplCorLib();
        }

        /// <summary>
        /// Creates a new runtime context for the provided bundled application.
        /// </summary>
        /// <param name="manifest">The bundle to create the runtime context for.</param>
        public RuntimeContext(BundleManifest manifest)
            : this(manifest, new ModuleReaderParameters(new ByteArrayFileService()))
        {
        }

        /// <summary>
        /// Creates a new runtime context.
        /// </summary>
        /// <param name="manifest">The bundle to create the runtime context for.</param>
        /// <param name="readerParameters">The parameters to use when reading modules in this context.</param>
        public RuntimeContext(BundleManifest manifest, ModuleReaderParameters readerParameters)
        {
            TargetRuntime = manifest.GetTargetRuntime();
            DefaultReaderParameters = new ModuleReaderParameters(readerParameters) {RuntimeContext = this};
            AssemblyResolver = new BundleAssemblyResolver(manifest, readerParameters);
            RuntimeCorLib = AssemblyResolver.Resolve(TargetRuntime.GetDefaultCorLib())?.ManifestModule?.CorLibTypeFactory.Object.Resolve()?.Module?.Assembly;
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

        private static IAssemblyResolver CreateAssemblyResolver(
            DotNetRuntimeInfo runtime,
            ModuleReaderParameters readerParameters)
        {
            switch (runtime.Name)
            {
                case DotNetRuntimeInfo.NetFramework:
                case DotNetRuntimeInfo.NetStandard when string.IsNullOrEmpty(DotNetCorePathProvider.DefaultInstallationPath):
                    return new DotNetFrameworkAssemblyResolver(readerParameters, MonoPathProvider.Default);

                case DotNetRuntimeInfo.NetStandard when DotNetCorePathProvider.Default.TryGetLatestStandardCompatibleVersion(runtime.Version, out var coreVersion):
                    return new DotNetCoreAssemblyResolver(readerParameters, coreVersion);

                case DotNetRuntimeInfo.NetCoreApp:
                    return new DotNetCoreAssemblyResolver(readerParameters, runtime.Version);

                default:
                    return new DotNetFrameworkAssemblyResolver(readerParameters, MonoPathProvider.Default);
            }
        }
    }
}
