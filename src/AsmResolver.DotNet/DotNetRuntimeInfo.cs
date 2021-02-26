using System;
using System.Text.RegularExpressions;

namespace AsmResolver.DotNet
{
    public struct DotNetRuntimeInfo
    {
        public const string NetCoreApp = ".NETCoreApp";
        public const string NetStandard = ".NETStandard";
        public const string NetFramework = ".NETFramework";

        private static readonly Regex FormatRegex = new(@"([a-zA-Z.]+)\s*,\s*Version=v(\d+\.\d+)");

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
            info = new DotNetRuntimeInfo(name, version);
            return true;
        }

        public DotNetRuntimeInfo(string name, Version version)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Version = version ?? throw new ArgumentNullException(nameof(version));
        }

        public string Name
        {
            get;
        }

        public Version Version
        {
            get;
        }

        /// <inheritdoc />
        public override string ToString() => $"{Name},Version=v{Version}";
    }
}
