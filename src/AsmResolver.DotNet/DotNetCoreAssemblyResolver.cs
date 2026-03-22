using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AsmResolver.DotNet.Config.Json;
using AsmResolver.DotNet.Serialized;
using AsmResolver.IO;
using AsmResolver.Shims;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides an implementation of an assembly resolver that includes .NET or .NET Core runtime libraries.
    /// </summary>
    public class DotNetCoreAssemblyResolver : AssemblyResolverBase
    {
        private readonly List<string> _runtimeDirectories = [];

        /// <summary>
        /// Creates a new .NET Core assembly resolver.
        /// </summary>
        /// <param name="runtimeVersion">The version of .NET to target.</param>
        /// <param name="pathProvider">The assumed system installation provider of .NET Core, or <c>null</c> to use the default path provider.</param>
        /// <param name="readerParameters">The parameters to use while reading assemblies, or <c>null</c> to use the default reader parameters.</param>
        public DotNetCoreAssemblyResolver(
            Version runtimeVersion,
            DotNetCorePathProvider? pathProvider = null,
            ModuleReaderParameters? readerParameters = null)
            : this(
                null,
                null,
                runtimeVersion,
                pathProvider,
                readerParameters
            )
        {
        }

        /// <summary>
        /// Creates a new .NET Core assembly resolver.
        /// </summary>
        /// <param name="configuration">The runtime configuration to use, or <c>null</c> if no configuration is available.</param>
        /// <param name="sourceDirectory">The directory of the main assembly.</param>
        /// <param name="fallbackVersion">The version of .NET or .NET Core to use when no (valid) configuration is provided.</param>
        /// <param name="pathProvider">The assumed system installation provider of .NET Core, or <c>null</c> to use the default path provider.</param>
        /// <param name="readerParameters">The parameters to use while reading assemblies, or <c>null</c> to use the default reader parameters.</param>
        public DotNetCoreAssemblyResolver(
            RuntimeConfiguration? configuration,
            string? sourceDirectory = null,
            Version? fallbackVersion = null,
            DotNetCorePathProvider? pathProvider = null,
            ModuleReaderParameters? readerParameters = null)
            : base(readerParameters ?? new ModuleReaderParameters(UncachedFileService.Instance))
        {
            var options = configuration?.RuntimeOptions;
            if (fallbackVersion is null && (options is null || !options.GetAllFrameworks().Any()))
                throw new ArgumentException("One of configuration or fallback version must be non-null.");

            pathProvider ??= DotNetCorePathProvider.Default;

            bool hasNetCoreApp = false;

            // Prefer runtime configuration if provided.
            if (options is not null)
            {
                // Self-contained -> prefer the source directory.
                if (options.Framework is null && options.Frameworks is null && !string.IsNullOrEmpty(sourceDirectory))
                    _runtimeDirectories.Add(sourceDirectory!);

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

                // Add additional probing paths
                if (options.AdditionalProbingPaths is not null)
                {
                    foreach (string? path in options.AdditionalProbingPaths)
                    {
                        if (path is not null)
                            SearchDirectories.Add(path);
                    }
                }

                // Infer fallback version from runtimeconfig when there is no fallback specified.
                // This is necessary because some runtimeconfig.json files may only specify one
                // framework (e.g., "Microsoft.WindowsDesktop.App") but still implicitly use
                // assemblies from the base "Microsoft.NETCore.App" framework which we want to include.
                if (fallbackVersion is null && configuration!.TryGetTargetRuntime(out var runtime))
                    fallbackVersion = runtime.Version;
            }

            // Include the fallback .NET version.
            if (fallbackVersion is not null && pathProvider.HasRuntimeInstalled(fallbackVersion))
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
        public override string? ProbeAssemblyFilePath(AssemblyDescriptor assembly, ModuleDefinition? originModule)
        {
            string? path = null;

            path ??= ProbeRuntimeDirectories(assembly);
            path ??= ProbeSearchDirectories(assembly, originModule);

            return path;
        }

        private string? ProbeRuntimeDirectories(AssemblyDescriptor assembly)
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
