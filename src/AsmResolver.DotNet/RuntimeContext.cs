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
            : this(targetRuntime, new ModuleReaderParameters
            {
                PEReaderParameters = {FileService = new ByteArrayFileService()}
            })
        {
        }

        /// <summary>
        /// Creates a new runtime context.
        /// </summary>
        /// <param name="targetRuntime">The target runtime version.</param>
        /// <param name="readerParameters">The parameters to use when reading modules in this context.</param>
        public RuntimeContext(DotNetRuntimeInfo targetRuntime, ModuleReaderParameters readerParameters)
        {
            TargetRuntime = targetRuntime;
            AssemblyResolver = CreateAssemblyResolver(targetRuntime, new ModuleReaderParameters(readerParameters)
            {
                RuntimeContext = this
            });
        }

        /// <summary>
        /// Creates a new runtime context.
        /// </summary>
        /// <param name="targetRuntime">The target runtime version.</param>
        /// <param name="assemblyResolver">The assembly resolver to use when resolving assemblies into this context.</param>
        public RuntimeContext(DotNetRuntimeInfo targetRuntime, IAssemblyResolver assemblyResolver)
        {
            TargetRuntime = targetRuntime;
            AssemblyResolver = assemblyResolver;
        }

        /// <summary>
        /// Gets the runtime version this context is targeting.
        /// </summary>
        public DotNetRuntimeInfo TargetRuntime
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

        private static IAssemblyResolver CreateAssemblyResolver(
            DotNetRuntimeInfo runtime,
            ModuleReaderParameters readerParameters)
        {
            switch (runtime.Name)
            {
                case DotNetRuntimeInfo.NetFramework:
                case DotNetRuntimeInfo.NetStandard when string.IsNullOrEmpty(DotNetCorePathProvider.DefaultInstallationPath):
                    return new DotNetFrameworkAssemblyResolver(readerParameters);

                case DotNetRuntimeInfo.NetStandard when DotNetCorePathProvider.Default.TryGetLatestStandardCompatibleVersion(runtime.Version, out var coreVersion):
                    return new DotNetCoreAssemblyResolver(readerParameters, coreVersion);

                case DotNetRuntimeInfo.NetCoreApp:
                    return new DotNetCoreAssemblyResolver(readerParameters, runtime.Version);

                default:
                    return new DotNetFrameworkAssemblyResolver(readerParameters);
            }
        }
    }
}
