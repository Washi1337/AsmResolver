using System;
using System.Diagnostics.CodeAnalysis;

namespace AsmResolver.Shims;

/// <summary>
/// Provides compatibility shims for the <see cref="Version"/> class for builds that target older .NET framework versions.
/// </summary>
public class VersionShim
{
    /// <summary>
    /// Parses a version string.
    /// </summary>
    /// <param name="s">The version string</param>
    /// <returns>The version</returns>
    public static Version Parse(string s)
    {
        return new Version(s);
    }

    /// <summary>
    /// Tries to convert the string representation of a version number to an equivalent Version object, and
    /// returns a value that indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="s">A string that contains a version number to convert.</param>
    /// <param name="version">
    /// When this method returns, contains the Version equivalent of the number that is contained in input, if the
    /// conversion succeeded. If input is null, Empty, or if the conversion fails, result is null when the method
    /// returns.
    /// </param>
    /// <returns>true if the input parameter was converted successfully; otherwise, false.</returns>
    public static bool TryParse(string s, [NotNullWhen(true)] out Version? version)
    {
#if !NET35
        return Version.TryParse(s, out version);
#else
        try
        {
            version = new Version(s);
            return true;
        }
        catch
        {
            version = null;
            return false;
        }
#endif
    }
}
