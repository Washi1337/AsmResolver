using System;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides an implementation of an assembly resolver that includes .NET or .NET Core runtime libraries.
    /// </summary>
    public class DotNetCoreAssemblyResolver : AssemblyResolverBase
    {
        private readonly Version _runtimeVersion;
        private readonly DotNetCorePathProvider _pathProvider;

        /// <summary>
        /// Creates a new .NET Core assembly resolver, by attempting to autodetect the current .NET or .NET Core
        /// installation directory.
        /// </summary>
        public DotNetCoreAssemblyResolver(Version runtimeVersion)
            : this(runtimeVersion, DotNetCorePathProvider.Default)
        {
        }

        /// <summary>
        /// Creates a new .NET Core assembly resolver.
        /// </summary>
        /// <param name="runtimeVersion">The version of .NET or .NET Core.</param>
        /// <param name="pathProvider">The installation directory of .NET Core.</param>
        public DotNetCoreAssemblyResolver(Version runtimeVersion, DotNetCorePathProvider pathProvider)
        {
            _runtimeVersion = runtimeVersion ?? throw new ArgumentNullException(nameof(runtimeVersion));
            _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
        }

        /// <inheritdoc />
        protected override AssemblyDefinition ResolveImpl(AssemblyDescriptor assembly)
        {
            // Prefer assemblies in the current directory, in case .NET libraries are shipped with the application.
            string path = ProbeSearchDirectories(assembly);

            if (string.IsNullOrEmpty(path))
            {
                // If failed, probe the runtime installation directories.
                if (assembly.GetPublicKeyToken() is not null)
                    path = ProbeRuntimeDirectories(assembly);

                // If still no suitable file was found, abort.
                if (string.IsNullOrEmpty(path))
                    return null;
            }

            // Attempt to load the file.
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
            // Avoid enumeration if there is no appropriate runtime installed.
            if (!_pathProvider.HasRuntimeInstalled(_runtimeVersion))
                return null;

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
