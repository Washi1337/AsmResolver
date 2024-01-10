using AsmResolver.IO;

namespace AsmResolver.DotNet
{
    public class RuntimeContext
    {
        public RuntimeContext(DotNetRuntimeInfo targetRuntime)
        {
            TargetRuntime = targetRuntime;
            AssemblyResolver = CreateAssemblyResolver(targetRuntime, new ByteArrayFileService());
        }

        public RuntimeContext(DotNetRuntimeInfo targetRuntime, IFileService fileService)
        {
            TargetRuntime = targetRuntime;
            AssemblyResolver = CreateAssemblyResolver(targetRuntime, fileService);
        }

        public RuntimeContext(DotNetRuntimeInfo targetRuntime, IAssemblyResolver assemblyResolver)
        {
            TargetRuntime = targetRuntime;
            AssemblyResolver = assemblyResolver;
        }

        public DotNetRuntimeInfo TargetRuntime
        {
            get;
        }

        public IAssemblyResolver AssemblyResolver
        {
            get;
        }

        private static IAssemblyResolver CreateAssemblyResolver(DotNetRuntimeInfo runtime, IFileService fileService)
        {
            AssemblyResolverBase resolver;
            switch (runtime.Name)
            {
                case DotNetRuntimeInfo.NetFramework:
                case DotNetRuntimeInfo.NetStandard
                    when string.IsNullOrEmpty(DotNetCorePathProvider.DefaultInstallationPath):
                    resolver = new DotNetFrameworkAssemblyResolver(fileService);
                    break;

                case DotNetRuntimeInfo.NetStandard
                    when DotNetCorePathProvider.Default.TryGetLatestStandardCompatibleVersion(
                        runtime.Version, out var coreVersion):
                    resolver = new DotNetCoreAssemblyResolver(fileService, coreVersion);
                    break;

                case DotNetRuntimeInfo.NetCoreApp:
                    resolver = new DotNetCoreAssemblyResolver(fileService, runtime.Version);
                    break;

                default:
                    resolver = new DotNetFrameworkAssemblyResolver(fileService);
                    break;
            }

            return resolver;
        }
    }
}
