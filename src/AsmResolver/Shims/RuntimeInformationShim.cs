using System;
using System.Runtime.InteropServices;

namespace AsmResolver.Shims;

/// <summary>
/// Provides compatibility shims for the RuntimeInformation class for builds that target older .NET framework versions.
/// </summary>
public static class RuntimeInformationShim
{
    static RuntimeInformationShim()
    {
#if !NET35
        IsRunningOnWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        IsRunningOnUnix = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        FrameworkDescription = RuntimeInformation.FrameworkDescription;
#else
        // https://stackoverflow.com/questions/5116977/how-to-check-the-os-version-at-runtime-e-g-on-windows-or-linux-without-using
        int p = (int) Environment.OSVersion.Platform;
        IsRunningOnUnix = p is 4 or 6 or 128;

        IsRunningOnWindows = !IsRunningOnUnix;
        FrameworkDescription = $".NET Framework {Environment.Version}";
#endif
    }

    /// <summary>
    /// Gets a value indicating whether the current runtime runs on the Microsoft Windows operating system.
    /// </summary>
    public static bool IsRunningOnWindows
    {
        get;
    }

    /// <summary>
    /// Gets a value indicating whether the current runtime runs on a Unix-based operating system.
    /// </summary>
    public static bool IsRunningOnUnix
    {
        get;
    }

    /// <summary>
    /// Gets the name of the current runtime.
    /// </summary>
    public static string FrameworkDescription
    {
        get;
    }
}
