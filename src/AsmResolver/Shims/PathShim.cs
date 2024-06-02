using System.IO;

namespace AsmResolver.Shims;

/// <summary>
/// Provides compatibility shims for the <see cref="Path"/> class for builds that target older .NET framework versions.
/// </summary>
public static class PathShim
{
#if !NET35

    /// <summary>
    /// Combines two strings into a path.
    /// </summary>
    /// <param name="path1">The first path.</param>
    /// <param name="path2">The second path.</param>
    /// <returns>The combined paths.</returns>
    public static string Combine(string path1, string path2) => Path.Combine(path1, path2);

    /// <summary>
    /// Combines three strings into a path.
    /// </summary>
    /// <param name="path1">The first path.</param>
    /// <param name="path2">The second path.</param>
    /// <param name="path3">The third path.</param>
    /// <returns>The combined paths.</returns>
    public static string Combine(string path1, string path2, string path3) => Path.Combine(path1, path2, path3);

#else

    /// <summary>
    /// Combines two strings into a path.
    /// </summary>
    /// <param name="path1">The first path.</param>
    /// <param name="path2">The second path.</param>
    /// <returns>The combined paths.</returns>
    public static string Combine(string path1, string path2) => Path.Combine(path1, path2);

    /// <summary>
    /// Combines three strings into a path.
    /// </summary>
    /// <param name="path1">The first path.</param>
    /// <param name="path2">The second path.</param>
    /// <param name="path3">The third path.</param>
    /// <returns>The combined paths.</returns>
    public static string Combine(string path1, string path2, string path3) => Path.Combine(Path.Combine(path1, path2), path3);

#endif
}
