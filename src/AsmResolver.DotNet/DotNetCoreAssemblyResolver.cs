using System;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides an implementation of an assembly resolver that includes .NET Core runtime libraries.
    /// </summary>
    public class DotNetCoreAssemblyResolver : AssemblyResolverBase
    {
        private readonly Version _runtimeVersion;
        private readonly DotNetCorePathProvider _pathProvider;

        /// <summary>
        /// Creates a new .NET Core assembly resolver, by attempting to autodetect the current .NET Core installation
        /// directory.
        /// </summary>
        public DotNetCoreAssemblyResolver(Version runtimeVersion)
            : this(runtimeVersion, DotNetCorePathProvider.Default)
        {
        }

        /// <summary>
        /// Creates a new .NET Core assembly resolver.
        /// </summary>
        /// <param name="pathProvider">The installation directory of .NET Core.</param>
        public DotNetCoreAssemblyResolver(Version runtimeVersion, DotNetCorePathProvider pathProvider)
        {
            _runtimeVersion = runtimeVersion;
            _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
        }

        /// <inheritdoc />
        protected override AssemblyDefinition ResolveImpl(AssemblyDescriptor assembly)
        {
            string path = ProbeSearchDirectories(assembly);

            if (string.IsNullOrEmpty(path) && assembly.GetPublicKeyToken() is not null)
                path = ProbeRuntimeDirectories(assembly);

            AssemblyDefinition assemblyDef = null;
            try
            {
                assemblyDef = LoadAssemblyFromFile(path);
            }
            catch
            {
                // ignore any errors.
            }

            return assemblyDef;
        }


        private string ProbeRuntimeDirectories(AssemblyDescriptor assembly)
        {
            foreach (string candidate in _pathProvider.GetRuntimePathCandidates(_runtimeVersion))
            {
                string path = ProbeDirectory(assembly, candidate);
                if (!string.IsNullOrEmpty(path))
                    return path;
            }

            return null;
        }
    }
}
