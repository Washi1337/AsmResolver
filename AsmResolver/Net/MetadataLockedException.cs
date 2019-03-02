using System;

namespace AsmResolver.Net
{
    /// <summary>
    /// Occurs when an operation that requires low-level (unlocked) mode access was attempted but high-level (locked)
    /// mode was active.
    /// </summary>
    public class MetadataLockedException : Exception
    {
        public MetadataLockedException(string operation)
            : base($"Cannot {operation} while metadata is locked.")
        {
        }
    }
}