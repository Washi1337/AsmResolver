using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides a mechanism for looking up versioned runtimes in a .NET Core / .NET 5.0+ installation folder.
    /// </summary>
    public class DotNetCorePathProvider
    {
        private static readonly string[] DefaultDotNetUnixPaths = {
            "/usr/share/dotnet/shared",
            "/opt/dotnet/shared/"
        };

        private static readonly Regex NetCoreRuntimePattern = new(@"\.NET( Core)? \d+\.\d+\.\d+");
        private readonly List<DotNetInstallationInfo> _installedRuntimes = new();

        static DotNetCorePathProvider()
        {
            DefaultInstallationPath = FindDotNetPath();
            Default = new();
        }

        /// <summary>
        /// Creates a new .NET installation path provider, using the <see cref="DefaultInstallationPath"/>
        /// as the installation path.
        /// </summary>
        public DotNetCorePathProvider()
            : this(DefaultInstallationPath)
        {
        }

        /// <summary>
        /// Creates a new .NET installation path provider, using the provided installation folder for .NET.
        /// </summary>
        /// <param name="installationDirectory">The .NET installation folder.</param>
        public DotNetCorePathProvider(string installationDirectory)
        {
            if (!string.IsNullOrEmpty(installationDirectory) && Directory.Exists(installationDirectory))
                DetectInstalledRuntimes(installationDirectory);
        }

        /// <summary>
        /// Gets the default path provider representing the global .NET installation on the current system.
        /// </summary>
        public static DotNetCorePathProvider Default
        {
            get;
        }

        /// <summary>
        /// Gets the installation path of the .NET installation on the current system.
        /// </summary>
        public static string DefaultInstallationPath
        {
            get;
        }

        /// <summary>
        /// Attempts to get the most recent version of .NET Core or .NET that is compatible with the provided
        /// .NET standard version.
        /// </summary>
        /// <param name="standardVersion">The .NET standard version.</param>
        /// <param name="coreVersion">The most recent compatible .NET or .NET Core version available.</param>
        /// <returns><c>true</c> if a compatible version was found, <c>false</c> otherwise.</returns>
        public bool TryGetLatestStandardCompatibleVersion(Version standardVersion, out Version coreVersion)
        {
            bool foundMatch = false;
            coreVersion = default;

            foreach (var runtime in _installedRuntimes)
            {
                for (int i = 0; i < runtime.InstalledVersions.Count; i++)
                {
                    var versionInfo = runtime.InstalledVersions[i];
                    if (versionInfo.IsCompatibleWithStandard(standardVersion)
                        && (coreVersion is null || versionInfo.Version > coreVersion))
                    {
                        foundMatch = true;
                        coreVersion = versionInfo.Version;
                    }
                }
            }

            return foundMatch;
        }

        /// <summary>
        /// Collects all paths to the runtimes that implement the provided .NET or .NET Core runtime version.
        /// </summary>
        /// <param name="requestedRuntimeVersion">The requested .NET or .NET Core version.</param>
        /// <returns>A collection of paths that implement the requested version.</returns>
        public IEnumerable<string> GetRuntimePathCandidates(Version requestedRuntimeVersion)
        {
            foreach (var runtime in _installedRuntimes)
            {
                if (runtime.TryFindBestMatchingVersion(requestedRuntimeVersion, out var match))
                    yield return match.FullPath;
            }
        }

        /// <summary>
        /// Collects all paths to the runtimes that implement the provided .NET or .NET Core runtime version.
        /// </summary>
        /// <returns>A collection of paths that implement the requested version.</returns>
        public IEnumerable<string> GetRuntimePathCandidates(string runtimeName, Version runtimeVersion)
        {
            foreach (var runtime in _installedRuntimes)
            {
                if (runtime.Name == runtimeName && runtime.TryFindBestMatchingVersion(runtimeVersion, out var match))
                    yield return match.FullPath;
            }
        }

        /// <summary>
        /// Determines whether a specific version of the runtime is installed or not.
        /// </summary>
        /// <param name="runtimeVersion">The runtime version.</param>
        /// <returns><c>true</c> if the version is installed, <c>false</c> otherwise.</returns>
        public bool HasRuntimeInstalled(Version runtimeVersion)
        {
            foreach (var runtime in _installedRuntimes)
            {
                if (runtime.TryFindBestMatchingVersion(runtimeVersion, out _))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether a specific version of the runtime is installed or not.
        /// </summary>
        /// <param name="runtimeName">The name of the runtime.</param>
        /// <param name="runtimeVersion">The runtime version.</param>
        /// <returns><c>true</c> if the version is installed, <c>false</c> otherwise.</returns>
        public bool HasRuntimeInstalled(string runtimeName, Version runtimeVersion)
        {
            foreach (var runtime in _installedRuntimes)
            {
                if (runtime.Name == runtimeName && runtime.TryFindBestMatchingVersion(runtimeVersion, out _))
                    return true;
            }

            return false;
        }

        private void DetectInstalledRuntimes(string installationDirectory)
        {
            installationDirectory = Path.Combine(installationDirectory, "shared");
            foreach (string directory in Directory.EnumerateDirectories(installationDirectory))
                _installedRuntimes.Add(new DotNetInstallationInfo(directory));
            _installedRuntimes.Sort();
        }

        /// <summary>
        /// Attempts to auto detect the installation directory of .NET or .NET Core.
        /// </summary>
        /// <returns>The path to the runtime, or <c>null</c> if none was found.</returns>
        private static string FindDotNetPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Probe PATH for installation folder of dotnet.
                string[] paths = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(Path.PathSeparator);
                foreach (string path in paths)
                {
                    if (File.Exists(Path.Combine(path, "dotnet.exe")))
                        return path;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // Probe default locations for installation folder of dotnet.
                foreach (string path in DefaultDotNetUnixPaths)
                {
                    if (File.Exists(Path.Combine(path, "dotnet")))
                        return path;
                }
            }

            if (NetCoreRuntimePattern.Match(RuntimeInformation.FrameworkDescription).Success)
            {
                // Fallback: if we are currently running .NET Core or newer, we can infer the installation directory
                // with the help of System.Reflection. The assembly of System.Object is either System.Runtime
                // or System.Private.CoreLib, which is located at <installation_directory>/shared/<runtime>/<version>/.

                string corlibPath = typeof(object).Assembly.Location;
                string versionPath = Path.GetDirectoryName(corlibPath);
                string runtimePath = Path.GetDirectoryName(versionPath);
                string sharedPath = Path.GetDirectoryName(runtimePath);
                return Path.GetDirectoryName(sharedPath);
            }

            return null;
        }

        /// <summary>
        /// Provides information about a .NET runtime installation.
        /// </summary>
        private readonly struct DotNetInstallationInfo : IComparable<DotNetInstallationInfo>
        {
            /// <summary>
            /// Creates a new instance of the <see cref="DotNetInstallationInfo"/> structure.
            /// </summary>
            /// <param name="path">The path to the runtime.</param>
            public DotNetInstallationInfo(string path)
            {
                string name = Path.GetFileName(path);
                var installedVersions = DetectInstalledVersionsInDirectory(name, path);

                Name = name;
                FullPath = path;
                InstalledVersions = installedVersions.AsReadOnly();
            }

            /// <summary>
            /// Gets the name of the runtime.
            /// </summary>
            public string Name
            {
                get;
            }

            /// <summary>
            /// Gets the full path to the runtime.
            /// </summary>
            public string FullPath
            {
                get;
            }

            /// <summary>
            /// Gets a list of installed versions in the directory.
            /// </summary>
            public IReadOnlyList<DotNetRuntimeVersionInfo> InstalledVersions
            {
                get;
            }

            /// <summary>
            /// Attempts to find a version that best matches the provided requested .NET or .NET Core version.
            /// </summary>
            /// <param name="requestedVersion">The requested .NET or .NET Core version.</param>
            /// <param name="versionInfo">The runtime that best matches the version.</param>
            /// <returns><c>true</c> if a match was found, <c>false</c> otherwise.</returns>
            public bool TryFindBestMatchingVersion(Version requestedVersion, out DotNetRuntimeVersionInfo versionInfo)
            {
                versionInfo = default;

                var bestMatchVersion = new Version();
                bool foundMatch = false;

                for (int i = 0; i < InstalledVersions.Count; i++)
                {
                    var candidate = InstalledVersions[i];
                    var candidateVersion = candidate.Version;

                    // Prefer exact matches of the version.
                    if (candidateVersion == requestedVersion)
                    {
                        versionInfo = candidate;
                        return true;
                    }

                    // Match the major version.
                    if (candidateVersion.Major == requestedVersion.Major && bestMatchVersion < requestedVersion)
                    {
                        versionInfo = candidate;
                        bestMatchVersion = candidateVersion;
                        foundMatch = true;
                    }
                }

                return foundMatch;
            }

            /// <summary>
            /// Finds all installed versions in the directory.
            /// </summary>
            /// <param name="name">The name of the runtime.</param>
            /// <param name="path">The path to the directory to search in.</param>
            /// <returns>The list of runtimes installed in the provided directory.</returns>
            private static List<DotNetRuntimeVersionInfo> DetectInstalledVersionsInDirectory(string name, string path)
            {
                var versions = new List<DotNetRuntimeVersionInfo>();
                foreach (string versionDirectory in Directory.EnumerateDirectories(path))
                {
                    string versionString = Path.GetFileName(versionDirectory);

                    // TODO: use semver parsing.
                    int suffixIndex = versionString.IndexOf('-');
                    if (suffixIndex >= 0)
                        versionString = versionString.Remove(suffixIndex);

                    if (!Version.TryParse(versionString, out var version))
                        continue;

                    versions.Add(new DotNetRuntimeVersionInfo(name, version, versionDirectory));
                }

                return versions;
            }

            /// <inheritdoc />
            public int CompareTo(DotNetInstallationInfo other)
            {
                // Ensure .NETCoreApp is sorted last to give other runtimes (like Microsoft.WindowsDesktop.App)
                // priority. This prevents libraries such as WindowsBase.dll to be incorrectly resolved.

                if (Name == other.Name)
                    return 0;
                if (Name == KnownRuntimeNames.NetCoreApp)
                    return 1;
                if (other.Name == KnownRuntimeNames.NetCoreApp)
                    return -1;
                return 0;
            }

#if DEBUG
            /// <inheritdoc />
            public override string ToString() => $"{Name} ({InstalledVersions.Count.ToString()} versions)";
#endif
        }

        /// <summary>
        /// Provides information about a single installation of the .NET runtime.
        /// </summary>
        private readonly struct DotNetRuntimeVersionInfo
        {
            /// <summary>
            /// Creates a new instance of the <see cref="DotNetRuntimeVersionInfo"/> structure..
            /// </summary>
            /// <param name="runtimeName">The name of the runtime.</param>
            /// <param name="version">The version of the runtime.</param>
            /// <param name="fullPath">The full path to the installation directory.</param>
            public DotNetRuntimeVersionInfo(string runtimeName, Version version, string fullPath)
            {
                RuntimeName = runtimeName;
                Version = version;
                FullPath = fullPath;
            }

            /// <summary>
            /// Gets the name of the runtime.
            /// </summary>
            public string RuntimeName
            {
                get;
            }

            /// <summary>
            /// Gets the version of the runtime.
            /// </summary>
            public Version Version
            {
                get;
            }

            /// <summary>
            /// Gets the full path to the installation directory of the runtime.
            /// </summary>
            public string FullPath
            {
                get;
            }

            /// <summary>
            /// Determines whether the runtime is compatible with the provided .NET standard version
            /// </summary>
            /// <param name="standardVersion">The .NET standard version.</param>
            /// <returns><c>true</c> if compatible, <c>false</c> otherwise.</returns>
            public bool IsCompatibleWithStandard(Version standardVersion)
            {
                // https://docs.microsoft.com/en-us/dotnet/standard/net-standard

                if (standardVersion.Major == 2)
                {
                    if (standardVersion.Minor == 0)
                        return Version.Major >= 2;
                    if (standardVersion.Minor == 1)
                        return Version.Major >= 3;
                }

                return true;
            }

#if DEBUG
            /// <inheritdoc />
            public override string ToString() => $"{RuntimeName}, v{Version}";
#endif
        }
    }


}
