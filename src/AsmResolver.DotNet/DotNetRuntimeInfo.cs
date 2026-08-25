using System;
using System.Text.RegularExpressions;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides information about a target runtime.
    /// </summary>
    public readonly struct DotNetRuntimeInfo : IEquatable<DotNetRuntimeInfo>
    {
        /// <summary>
        /// The target framework name used by applications targeting .NET or .NET Core.
        /// </summary>
        public const string NetCoreAppName = ".NETCoreApp";

        /// <summary>
        /// A target framework name used in some legacy architectures.
        /// </summary>
        /// <remarks>
        /// This should not be confused with <see cref="NetCoreName"/>, which is the correct name for applications targeting .NET or .NET Core.
        /// </remarks>
        public const string NetCoreName = ".NETCore";

        /// <summary>
        /// The target framework name used by libraries targeting .NET Standard.
        /// </summary>
        public const string NetStandardName = ".NETStandard";

        /// <summary>
        /// The target framework name used by applications targeting legacy .NET Framework.
        /// </summary>
        public const string NetFrameworkName = ".NETFramework";

        /// <summary>
        /// The target framework name used by applications targeting legacy .NET Portable.
        /// </summary>
        public const string NetPortableName = ".NETPortable";

        /// <summary>
        /// The target framework name used by applications targeting Microsoft Silverlight.
        /// </summary>
        public const string SilverlightName = "Silverlight";

        private static readonly Regex FormatRegex = new(@"([a-zA-Z.]+)\s*,\s*Version=v(\d+\.\d+)");

        private static readonly Regex NetFxMonikerRegex = new(@"net(\d)(\d)(\d?)");

        private static readonly Regex NetCoreAppMonikerRegex = new(@"netcoreapp(\d)\.(\d)");

        private static readonly Regex NetStandardMonikerRegex = new(@"netstandard(\d)\.(\d)");

        private static readonly Regex NetMonikerRegex = new(@"net(\d+)\.(\d+)");

        /// <summary>
        /// Creates a new instance of the <see cref="DotNetRuntimeInfo"/> structure.
        /// </summary>
        /// <param name="name">The name of the runtime.</param>
        /// <param name="version">The version of the runtime.</param>
        public DotNetRuntimeInfo(string name, Version version)
            : this(name, version, null)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DotNetRuntimeInfo"/> structure.
        /// </summary>
        /// <param name="name">The name of the runtime.</param>
        /// <param name="version">The version of the runtime.</param>
        /// <param name="profile">The optional target framework profile.</param>
        public DotNetRuntimeInfo(string name, Version version, string? profile)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Version = version ?? throw new ArgumentNullException(nameof(version));
            Profile = profile;
        }

        /// <summary>
        /// Gets the name of the runtime.
        /// </summary>
        public string Name
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
        /// Gets the target framework profile, if available.
        /// </summary>
        public string? Profile
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the application targets the .NET or .NET Core runtime or not.
        /// </summary>
        public bool IsNetCoreApp => Name == NetCoreAppName;

        /// <summary>
        /// Gets a value indicating whether or not the application is targeting a legacy architecture that uses .NET Core (.NET Native).
        /// </summary>
        /// <remarks>
        /// This should not be confused with <see cref="IsNetCoreApp"/>, which is the correct property for whether or not an application is targeting .NET or .NET Core.
        /// </remarks>
        public bool IsNetCore => Name == NetCoreName;

        /// <summary>
        /// Gets a value indicating whether the application targets the .NET Framework runtime or not.
        /// </summary>
        public bool IsNetFramework => Name == NetFrameworkName;

        /// <summary>
        /// Gets a value indicating whether the application targets the .NET standard specification or not.
        /// </summary>
        public bool IsNetStandard => Name == NetStandardName;

        /// <summary>
        /// Gets a value indicating whether the application targets the .NET Portable runtime or not.
        /// </summary>
        public bool IsNetPortable => Name == NetPortableName;

        /// <summary>
        /// Gets a value indicating whether the application targets the Microsoft Silverlight runtime or not.
        /// </summary>
        public bool IsSilverlight => Name == SilverlightName;

        /// <summary>
        /// Constructs a runtime info record referencing legacy .NET Framework.
        /// </summary>
        /// <param name="major">The major version</param>
        /// <param name="minor">The minor version</param>
        /// <returns>The runtime info record.</returns>
        public static DotNetRuntimeInfo NetFramework(int major, int minor) => NetFramework(new Version(major, minor));

        /// <summary>
        /// Constructs a runtime info record referencing legacy .NET Framework.
        /// </summary>
        /// <param name="major">The major version</param>
        /// <param name="minor">The minor version</param>
        /// <param name="patch">The patch version</param>
        /// <returns>The runtime info record.</returns>
        public static DotNetRuntimeInfo NetFramework(int major, int minor, int patch) => NetFramework(new Version(major, minor, patch));

        /// <summary>
        /// Constructs a runtime info record referencing legacy .NET Framework.
        /// </summary>
        /// <param name="version">The version</param>
        /// <returns>The runtime info record.</returns>
        public static DotNetRuntimeInfo NetFramework(Version version) => new(NetFrameworkName, version);

        /// <summary>
        /// Constructs a runtime info record referencing .NET Standard.
        /// </summary>
        /// <param name="major">The major version</param>
        /// <param name="minor">The minor version</param>
        /// <returns>The runtime info record.</returns>
        public static DotNetRuntimeInfo NetStandard(int major, int minor) => NetStandard(new Version(major, minor));

        /// <summary>
        /// Constructs a runtime info record referencing .NET Standard.
        /// </summary>
        /// <param name="version">The version</param>
        /// <returns>The runtime info record.</returns>
        public static DotNetRuntimeInfo NetStandard(Version version) => new(NetStandardName, version);

        /// <summary>
        /// Constructs a runtime info record referencing .NET Standard.
        /// </summary>
        /// <param name="major">The major version</param>
        /// <param name="minor">The minor version</param>
        /// <returns>The runtime info record.</returns>
        public static DotNetRuntimeInfo NetCoreApp(int major, int minor) => NetCoreApp(new Version(major, minor));

        /// <summary>
        /// Constructs a runtime info record referencing .NET Standard.
        /// </summary>
        /// <param name="major">The major version</param>
        /// <param name="minor">The minor version</param>
        /// <param name="patch">The patch version</param>
        /// <returns>The runtime info record.</returns>
        public static DotNetRuntimeInfo NetCoreApp(int major, int minor, int patch) => NetCoreApp(new Version(major, minor, patch));

        /// <summary>
        /// Constructs a runtime info record referencing .NET Core and .NET.
        /// </summary>
        /// <param name="version">The version</param>
        /// <returns>The runtime info record.</returns>
        public static DotNetRuntimeInfo NetCoreApp(Version version) => new(NetCoreAppName, version);

        /// <summary>
        /// Constructs a runtime info record referencing Microsoft Silverlight.
        /// </summary>
        /// <param name="major">The major version</param>
        /// <param name="minor">The minor version</param>
        /// <returns>The runtime info record.</returns>
        public static DotNetRuntimeInfo Silverlight(int major, int minor) => Silverlight(new Version(major, minor));

        /// <summary>
        /// Constructs a runtime info record referencing Microsoft Silverlight.
        /// </summary>
        /// <param name="version">The version</param>
        /// <returns>The runtime info record.</returns>
        public static DotNetRuntimeInfo Silverlight(Version version) => new(SilverlightName, version);

        /// <summary>
        /// Parses the framework name as provided in a <c>System.Runtime.Versioning.TargetFrameworkAttribute</c> attribute.
        /// </summary>
        /// <param name="frameworkName">The full runtime name.</param>
        /// <returns>The parsed version info.</returns>
        public static DotNetRuntimeInfo Parse(string frameworkName)
        {
            return TryParse(frameworkName, out var info) ? info : throw new FormatException();
        }

        /// <summary>
        /// Attempts to parse the framework name as provided in a <c>System.Runtime.Versioning.TargetFrameworkAttribute</c> attribute.
        /// </summary>
        /// <param name="frameworkName">The full runtime name.</param>
        /// <param name="info">The parsed version info.</param>
        /// <returns><c>true</c> if the provided name was in the correct format, <c>false</c> otherwise.</returns>
        public static bool TryParse(string frameworkName, out DotNetRuntimeInfo info)
        {
            var match = FormatRegex.Match(frameworkName);
            if (!match.Success)
            {
                info = default;
                return false;
            }

            string name = match.Groups[1].Value;
            var version = new Version(match.Groups[2].Value);
            info = new DotNetRuntimeInfo(name, version, ParseProfile(frameworkName));
            return true;
        }

        private static string? ParseProfile(string frameworkName)
        {
            string[] components = frameworkName.Split(',');

            for (int i = 1; i < components.Length; i++)
            {
                string component = components[i].Trim();
                int separatorIndex = component.IndexOf('=');

                if (separatorIndex < 0)
                    continue;

                string key = component.Substring(0, separatorIndex).Trim();
                if (!string.Equals(key, "Profile", StringComparison.OrdinalIgnoreCase))
                    continue;

                string profile = component.Substring(separatorIndex + 1).Trim();
                if (profile.Length > 0)
                    return profile;
            }

            return null;
        }

        /// <summary>
        /// Parses the target framework moniker as provided in a <c>.runtimeconfig.json</c> file.
        /// </summary>
        /// <param name="moniker">The moniker</param>
        /// <returns>The parsed version info.</returns>
        public static DotNetRuntimeInfo ParseMoniker(string moniker)
        {
            return TryParseMoniker(moniker, out var info) ? info : throw new FormatException();
        }

        /// <summary>
        /// Attempts to parse the target framework moniker as provided in a <c>.runtimeconfig.json</c> file.
        /// </summary>
        /// <param name="moniker">The moniker</param>
        /// <param name="info">The parsed version info.</param>
        /// <returns><c>true</c> if the provided name was in the correct format, <c>false</c> otherwise.</returns>
        public static bool TryParseMoniker(string moniker, out DotNetRuntimeInfo info)
        {
            info = default;
            string runtime;

            Match match;
            if ((match = NetMonikerRegex.Match(moniker)).Success)
                runtime = NetCoreAppName;
            else if ((match = NetCoreAppMonikerRegex.Match(moniker)).Success)
                runtime = NetCoreAppName;
            else if ((match = NetStandardMonikerRegex.Match(moniker)).Success)
                runtime = NetStandardName;
            else if ((match = NetFxMonikerRegex.Match(moniker)).Success)
                runtime = NetFrameworkName;
            else
                return false;

            var version = new Version(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));

            info = new DotNetRuntimeInfo(runtime, version);
            return true;
        }

        /// <summary>
        /// Obtains a reference to the default core lib of this runtime.
        /// </summary>
        /// <returns>The reference to the default core lib.</returns>
        /// <exception cref="ArgumentException">The runtime information is invalid or unsupported.</exception>
        public AssemblyReference GetDefaultCorLib() => KnownCorLibs.FromRuntimeInfo(this);

        /// <summary>
        /// Obtains a reference to the assumed implementation core lib for this runtime.
        /// </summary>
        /// <returns>The reference to the assumed implementation core lib.</returns>
        /// <exception cref="ArgumentException">The runtime information is invalid or unsupported.</exception>
        public AssemblyReference? GetAssumedImplCorLib() => KnownCorLibs.TryImplFromRuntimeInfo(this);

        /// <inheritdoc />
        public override string ToString()
        {
            return Profile is null
                ? $"{Name},Version=v{Version}"
                : $"{Name},Version=v{Version},Profile={Profile}";
        }

        /// <inheritdoc />
        public bool Equals(DotNetRuntimeInfo other)
        {
            return Name == other.Name && Version.Equals(other.Version) && Profile == other.Profile;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is DotNetRuntimeInfo other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name.GetHashCode() * 397) ^ Version.GetHashCode()) * 397 ^ (Profile?.GetHashCode() ?? 0);
            }
        }
    }
}
