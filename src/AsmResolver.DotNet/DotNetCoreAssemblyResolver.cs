using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Config.Json;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides an implementation of an assembly resolver that includes .NET or .NET Core runtime libraries.
    /// </summary>
    public class DotNetCoreAssemblyResolver : AssemblyResolverBase
    {
        private readonly List<string> _runtimeDirectories = new();

        /// <summary>
        /// Creates a new .NET Core assembly resolver, by attempting to autodetect the current .NET or .NET Core
        /// installation directory.
        /// </summary>
        public DotNetCoreAssemblyResolver(Version runtimeVersion)
            : this(runtimeVersion, DotNetCorePathProvider.Default)
        {
        }

        /// <summary>
        /// Creates a new .NET Core assembly resolver, by attempting to autodetect the current .NET or .NET Core
        /// installation directory.
        /// </summary>
        public DotNetCoreAssemblyResolver(RuntimeConfiguration configuration)
            : this(configuration, DotNetCorePathProvider.Default)
        {
        }

        /// <summary>
        /// Creates a new .NET Core assembly resolver, by attempting to autodetect the current .NET or .NET Core
        /// installation directory.
        /// </summary>
        public DotNetCoreAssemblyResolver(RuntimeConfiguration configuration, Version fallbackVersion)
            : this(configuration, fallbackVersion, DotNetCorePathProvider.Default)
        {
        }

        /// <summary>
        /// Creates a new .NET Core assembly resolver.
        /// </summary>
        /// <param name="runtimeVersion">The version of .NET or .NET Core.</param>
        /// <param name="pathProvider">The installation directory of .NET Core.</param>
        public DotNetCoreAssemblyResolver(Version runtimeVersion, DotNetCorePathProvider pathProvider)
            : this(null, runtimeVersion, pathProvider)
        {
        }

        /// <summary>
        /// Creates a new .NET Core assembly resolver.
        /// </summary>
        /// <param name="configuration">The runtime configuration to use.</param>
        /// <param name="pathProvider">The installation directory of .NET Core.</param>
        public DotNetCoreAssemblyResolver(RuntimeConfiguration configuration, DotNetCorePathProvider pathProvider)
            : this(configuration, null, pathProvider)
        {
        }

        /// <summary>
        /// Creates a new .NET Core assembly resolver.
        /// </summary>
        /// <param name="configuration">The runtime configuration to use, or <c>null</c> if no configuration is available.</param>
        /// <param name="fallbackVersion">The version of .NET or .NET Core to use when no (valid) configuration is provided.</param>
        /// <param name="pathProvider">The installation directory of .NET Core.</param>
        public DotNetCoreAssemblyResolver(RuntimeConfiguration configuration, Version fallbackVersion, DotNetCorePathProvider pathProvider)
        {
            // Prefer runtime configuration if provided.
            if (configuration?.RuntimeOptions is { } options)
            {
                // Order frameworks such that .NETCore.App is last.
                var frameworks = options
                    .GetAllFrameworks()
                    .OrderBy(framework => framework.Name, RuntimeNameComparer.Instance);

                // Get relevant framework directories.
                foreach (var framework in frameworks)
                    AddFrameworkDirectories(pathProvider, framework);
            }

            // If no directories where found, use the fallback .NET version.
            if (_runtimeDirectories.Count == 0 && pathProvider.HasRuntimeInstalled(fallbackVersion))
                _runtimeDirectories.AddRange(pathProvider.GetRuntimePathCandidates(fallbackVersion));
        }

        /// <inheritdoc />
        protected override string ProbeRuntimeDirectories(AssemblyDescriptor assembly)
        {
            foreach (string candidate in _runtimeDirectories)
            {
                string path = ProbeDirectory(assembly, candidate);
                if (!string.IsNullOrEmpty(path))
                    return path;
            }

            return null;
        }

        private void AddFrameworkDirectories(DotNetCorePathProvider pathProvider, RuntimeFramework framework)
        {
            if (TryParseVersion(framework.Version, out var version)
                && pathProvider.HasRuntimeInstalled(framework.Name, version))
            {
                _runtimeDirectories.AddRange(pathProvider.GetRuntimePathCandidates(framework.Name, version));
            }
        }

        private static bool TryParseVersion(string versionString, out Version version)
        {
            // TODO: use semver parsing.
            int suffixIndex = versionString.IndexOf('-');
            if (suffixIndex >= 0)
                versionString = versionString.Remove(suffixIndex);

            return Version.TryParse(versionString, out version); ;
        }

        private sealed class RuntimeNameComparer : IComparer<string>
        {
            public static readonly RuntimeNameComparer Instance = new();

            /// <inheritdoc />
            public int Compare(string x, string y)
            {
                if (x == y)
                    return 0;
                if (x == KnownRuntimeNames.NetCoreApp)
                    return 1;
                if (y == KnownRuntimeNames.NetCoreApp)
                    return -1;
                return 0;
            }
        }

    }
}
