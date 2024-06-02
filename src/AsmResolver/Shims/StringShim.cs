using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.Shims;

/// <summary>
/// Provides compatibility shims for the <see cref="string"/> class for builds that target older .NET framework versions.
/// </summary>
public static class StringShim
{
    /// <summary>
    /// Concatenates the members of a collection, using the specified separator between each member.
    /// </summary>
    /// <param name="separator">
    /// The string to use as a separator. separator is included in the returned string only if values has more than one
    /// element.
    /// </param>
    /// <param name="items">A collection that contains the objects to concatenate.</param>
    /// <typeparam name="T">The type of the members of values.</typeparam>
    /// <returns>
    /// A string that consists of the elements of values delimited by the separator string. -or-
    /// <see cref="string.Empty" /> if values has no elements.
    /// </returns>
    public static string Join<T>(string separator, IEnumerable<T> items)
    {
#if !NET35
        return string.Join(separator, items);
#else
        return string.Join(separator, items.Select(x => x?.ToString()).ToArray());
#endif
    }
}
