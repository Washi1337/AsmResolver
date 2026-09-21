using System.Collections.Generic;

namespace AsmResolver.PE.Relocations;

/// <summary>
/// Provides members for collecting base relocations for a specific PE structure or object.
/// </summary>
public interface IRelocatable
{
    /// <summary>
    /// Obtains a collection of base address relocations that need to be applied to the object after the image was
    /// loaded into memory.
    /// </summary>
    /// <returns>The required base relocations.</returns>
    IEnumerable<BaseRelocation> GetRequiredBaseRelocations();
}
