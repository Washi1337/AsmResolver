namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides strings of known runtime names used in .NET Core, .NET 5.0 and later.
    /// </summary>
    public static class KnownRuntimeNames
    {
        /// <summary>
        /// Indicates an application targeting the default .NET Core runtime.
        /// </summary>
        public const string NetCoreApp = "Microsoft.NETCore.App";

        /// <summary>
        /// Indicates an application targeting the Windows Desktop environment runtime.
        /// </summary>
        public const string WindowsDesktopApp = "Microsoft.WindowsDesktop.App";
    }
}
