using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AsmResolver.Collections;
using AsmResolver.DotNet.Config.Json;
using AsmResolver.IO;
using AsmResolver.Shims;

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
        /// <param name="runtimeVersion">The version of .NET to target.</param>
        public DotNetCoreAssemblyResolver(Version runtimeVersion)
            : this(UncachedFileService.Instance, null, runtimeVersion, DotNetCorePathProvider.Default)
        {
        }

        /// <summary>
        /// Creates a new .NET Core assembly resolver, by attempting to autodetect the current .NET or .NET Core
        /// installation directory.
        /// </summary>
        /// <param name="fileService">The service to use for reading files from the disk.</param>
        /// <param name="runtimeVersion">The version of .NET to target.</param>
        public DotNetCoreAssemblyResolver(IFileService fileService, Version runtimeVersion)
            : this(fileService, null, runtimeVersion, DotNetCorePathProvider.Default)
        {
        }

        /// <summary>
        /// Creates a new .NET Core assembly resolver, by attempting to autodetect the current .NET or .NET Core
        /// installation directory.
        /// </summary>
        /// <param name="configuration">The runtime configuration as specified by the *.runtimeconfig.json file.</param>
        /// <param name="fallbackVersion">The version of .NET to fallback on if the runtime configuration is insufficient.</param>
        public DotNetCoreAssemblyResolver(RuntimeConfiguration? configuration, Version fallbackVersion)
            : this(UncachedFileService.Instance, configuration, fallbackVersion, DotNetCorePathProvider.Default)
        {
        }

        /// <summary>
        /// Creates a new .NET Core assembly resolver, by attempting to autodetect the current .NET or .NET Core
        /// installation directory.
        /// </summary>
        /// <param name="fileService">The service to use for reading files from the disk.</param>
        /// <param name="configuration">The runtime configuration as specified by the *.runtimeconfig.json file.</param>
        /// <param name="fallbackVersion">The version of .NET to fallback on if the runtime configuration is insufficient.</param>
        public DotNetCoreAssemblyResolver(IFileService fileService, RuntimeConfiguration? configuration, Version fallbackVersion)
            : this(fileService, configuration, fallbackVersion, DotNetCorePathProvider.Default)
        {
        }

        /// <summary>
        /// Creates a new .NET Core assembly resolver.
        /// </summary>
        /// <param name="fileService">The service to use for reading files from the disk.</param>
        /// <param name="configuration">The runtime configuration to use.</param>
        /// <param name="pathProvider">The installation directory of .NET Core.</param>
        public DotNetCoreAssemblyResolver(IFileService fileService, RuntimeConfiguration? configuration, DotNetCorePathProvider pathProvider)
            : this(fileService, configuration, null, pathProvider)
        {
        }

        /// <summary>
        /// Creates a new .NET Core assembly resolver.
        /// </summary>
        /// <param name="fileService">The service to use for reading files from the disk.</param>
        /// <param name="configuration">The runtime configuration to use, or <c>null</c> if no configuration is available.</param>
        /// <param name="fallbackVersion">The version of .NET or .NET Core to use when no (valid) configuration is provided.</param>
        /// <param name="pathProvider">The installation directory of .NET Core.</param>
        public DotNetCoreAssemblyResolver(
            IFileService fileService,
            RuntimeConfiguration? configuration,
            Version? fallbackVersion,
            DotNetCorePathProvider pathProvider)
            : base(fileService)
        {
            if (fallbackVersion is null)
                throw new ArgumentNullException(nameof(fallbackVersion));
            if (pathProvider is null)
                throw new ArgumentNullException(nameof(pathProvider));

            bool hasNetCoreApp = false;

            // Prefer runtime configuration if provided.
            if (configuration?.RuntimeOptions is { } options)
            {
                // Order frameworks such that .NETCore.App is last.
                var frameworks = options
                    .GetAllFrameworks()
                    .OrderBy(framework => framework.Name!, RuntimeNameComparer.Instance);

                // Get relevant framework directories.
                foreach (var framework in frameworks)
                {
                    string[] directories = GetFrameworkDirectories(pathProvider, framework);
                    if (directories.Length > 0)
                    {
                        hasNetCoreApp |= framework.Name == KnownRuntimeNames.NetCoreApp;
                        _runtimeDirectories.AddRange(directories);
                    }
                }
            }

            // If no directories where found, use the fallback .NET version.
            if (pathProvider.HasRuntimeInstalled(fallbackVersion))
            {
                if (_runtimeDirectories.Count == 0)
                {
                    _runtimeDirectories.AddRange(pathProvider.GetRuntimePathCandidates(fallbackVersion));
                }
                else if (!hasNetCoreApp)
                {
                    // Ensure that Microsoft.NETCore.App is at least present. This is required since some runtimes
                    // do not define corlib libraries such as System.Private.CoreLib or System.Runtime.
                    _runtimeDirectories.AddRange(pathProvider.GetRuntimePathCandidates(
                        KnownRuntimeNames.NetCoreApp, fallbackVersion));
                }
            }
        }

        private static string[] GetFrameworkDirectories(DotNetCorePathProvider pathProvider, RuntimeFramework framework)
        {
            if (TryParseVersion(framework.Version, out var version)
                && pathProvider.HasRuntimeInstalled(framework.Name!, version))
            {
                return pathProvider.GetRuntimePathCandidates(framework.Name!, version).ToArray();
            }

            return ArrayShim.Empty<string>();
        }

        /// <inheritdoc />
        protected override string? ProbeRuntimeDirectories(AssemblyDescriptor assembly)
        {
            foreach (string candidate in _runtimeDirectories)
            {
                string? path = ProbeDirectory(assembly, candidate);
                if (!string.IsNullOrEmpty(path))
                    return path;
            }

            return null;
        }

        private static bool TryParseVersion(string? versionString, [NotNullWhen(true)] out Version? version)
        {
            if (string.IsNullOrEmpty(versionString))
            {
                version = null;
                return false;
            }

            // TODO: use semver parsing.
            int suffixIndex = versionString!.IndexOf('-');
            if (suffixIndex >= 0)
                versionString = versionString.Remove(suffixIndex);

            return VersionShim.TryParse(versionString, out version);
        }

        private sealed class RuntimeNameComparer : IComparer<string>
        {
            public static readonly RuntimeNameComparer Instance = new();

            /// <inheritdoc />
            public int Compare(string? x, string? y)
            {
                if (x == y)
                    return 0;
                if (x is null)
                    return -1;
                if (y is null)
                    return 1;
                if (x == KnownRuntimeNames.NetCoreApp)
                    return 1;
                if (y == KnownRuntimeNames.NetCoreApp)
                    return -1;
                return 0;
            }
        }

    }
}
