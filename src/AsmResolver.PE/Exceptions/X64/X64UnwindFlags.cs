using System;

namespace AsmResolver.PE.Exceptions.X64
{
    /// <summary>
    /// Defines all possible flags that can be assigned to an instance of <see cref="X64UnwindInfo"/>.
    /// </summary>
    [Flags]
    public enum X64UnwindFlags : byte
    {
        /// <summary>
        /// Indicates no handlers.
        /// </summary>
        NoHandler = 0,

        /// <summary>
        /// Indicates the function has an exception handler that should be called when looking for functions
        /// that need to examine exceptions.
        /// </summary>
        ExceptionHandler = 1,

        /// <summary>
        /// Indicates the function has a termination handler that should be called when unwinding an exception.
        /// </summary>
        TerminationHandler = 2,

        /// <summary>
        /// Indicates This unwind info structure is not the primary one for the procedure. Instead, the chained
        /// unwind info entry is the contents of a previous entry.
        /// </summary>
        ChainedUnwindInfo = 4
    }
}
