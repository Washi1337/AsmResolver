using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace AsmResolver.DotNet
{
    public class DotNetCorePathProvider
    {
        private static readonly string[] DefaultDotNetUnixPaths = {
            "/usr/share/dotnet/shared",
            "/opt/dotnet/shared/"
        };

        private static readonly Regex NetCoreRuntimePattern = new(@"\.NET( Core)? \d+\.\d+\.\d+");

        static DotNetCorePathProvider()
        {
            DefaultInstallationPath = FindDotNetPath();
            Default = new();
        }

        public static DotNetCorePathProvider Default
        {
            get;
        }

        public static string DefaultInstallationPath
        {
            get;
        }

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

        private readonly List<DotNetCoreRuntimeInfo> _installedRuntimes = new();

        public DotNetCorePathProvider()
            : this(DefaultInstallationPath)
        {
        }

        public DotNetCorePathProvider(string installationDirectory)
        {
            if (!string.IsNullOrEmpty(installationDirectory) && Directory.Exists(installationDirectory))
                DetectInstalledRuntimes(installationDirectory);
        }

        public bool TryGetLatestCompatibleVersion(Version netstandardVersion, out Version coreVersion)
        {
            bool foundMatch = false;
            coreVersion = default;

            foreach (var runtime in _installedRuntimes)
            {
                for (int i = 0; i < runtime.InstalledVersions.Count; i++)
                {
                    var versionInfo = runtime.InstalledVersions[i];
                    if (versionInfo.IsCompatibleWithStandard(netstandardVersion) && versionInfo.Version > coreVersion)
                    {
                        foundMatch = true;
                        coreVersion = versionInfo.Version;
                    }
                }
            }

            return foundMatch;
        }

        public IEnumerable<string> GetRuntimePathCandidates(Version version)
        {
            foreach (var runtime in _installedRuntimes)
            {
                if (runtime.TryFindBestMatchingVersion(version, out var match))
                    yield return match.FullPath;
            }
        }

        private void DetectInstalledRuntimes(string installationDirectory)
        {
            installationDirectory = Path.Combine(installationDirectory, "shared");
            foreach (string directory in Directory.EnumerateDirectories(installationDirectory))
                _installedRuntimes.Add(new DotNetCoreRuntimeInfo(directory));
            _installedRuntimes.Sort();
        }

        private readonly struct DotNetCoreRuntimeInfo : IComparable<DotNetCoreRuntimeInfo>
        {
            public DotNetCoreRuntimeInfo(string path)
            {
                string name = Path.GetFileName(path);
                var installedVersions = DetectInstalledVersionsInDirectory(name, path);

                Name = name;
                FullPath = path;
                InstalledVersions = installedVersions.AsReadOnly();
            }

            public string Name
            {
                get;
            }

            public string FullPath
            {
                get;
            }

            public IReadOnlyList<DotNetCoreRuntimeVersionInfo> InstalledVersions
            {
                get;
            }

            public bool TryFindBestMatchingVersion(Version requestedVersion, out DotNetCoreRuntimeVersionInfo versionInfo)
            {
                versionInfo = default;

                var bestMatchVersion = new Version();
                bool foundMatch = false;

                for (int i = 0; i < InstalledVersions.Count; i++)
                {
                    var candidate = InstalledVersions[i];

                    var candidateVersion = candidate.Version;
                    if (candidateVersion == requestedVersion)
                    {
                        versionInfo = candidate;
                        return true;
                    }

                    if (candidateVersion.Major == requestedVersion.Major && bestMatchVersion < requestedVersion)
                    {
                        versionInfo = candidate;
                        bestMatchVersion = candidateVersion;
                        foundMatch = true;
                    }
                }

                return foundMatch;
            }

            private static List<DotNetCoreRuntimeVersionInfo> DetectInstalledVersionsInDirectory(string name, string path)
            {
                var versions = new List<DotNetCoreRuntimeVersionInfo>();
                foreach (string versionDirectory in Directory.EnumerateDirectories(path))
                {
                    string versionString = Path.GetFileName(versionDirectory);

                    // TODO: use semver parsing.
                    int suffixIndex = versionString.IndexOf('-');
                    if (suffixIndex >= 0)
                        versionString = versionString.Remove(suffixIndex);

                    if (!Version.TryParse(versionString, out var version))
                        continue;

                    versions.Add(new DotNetCoreRuntimeVersionInfo(name, version, versionDirectory));
                }

                return versions;
            }

            /// <inheritdoc />
            public int CompareTo(DotNetCoreRuntimeInfo other)
            {
                // Ensure .NETCoreApp is sorted last to give other runtimes (like Microsoft.WindowsDesktop.App)
                // priority. This prevents libraries such as WindowsBase.dll to be incorrectly resolved.

                if (Name == KnownRuntimeNames.NetCoreApp)
                    return 1;
                return 0;
            }

#if DEBUG
            /// <inheritdoc />
            public override string ToString() => $"{Name} ({InstalledVersions.Count.ToString()} versions)";
#endif
        }

        private readonly struct DotNetCoreRuntimeVersionInfo
        {
            public DotNetCoreRuntimeVersionInfo(string runtimeName, Version version, string fullPath)
            {
                RuntimeName = runtimeName;
                Version = version;
                FullPath = fullPath;
            }

            public string RuntimeName
            {
                get;
            }

            public Version Version
            {
                get;
            }

            public string FullPath
            {
                get;
            }

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
