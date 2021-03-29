using System.Collections.Generic;

namespace AsmResolver.PE.Exceptions
{
    /// <summary>
    /// Represents the data directory containing a table of functions in a portable executable
    /// that use Structured Exception Handling (SEH).
    /// </summary>
    public interface IExceptionDirectory
    {
        /// <summary>
        /// Gets a collection of functions defined by the exception handler directory.
        /// </summary>
        /// <returns>The functions.</returns>
        IEnumerable<IRuntimeFunction> GetEntries();
    }
}
