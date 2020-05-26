using System;

namespace AsmResolver.DotNet.Extensions.Memory
{
    /// <summary>
    /// Gets thrown when the layout of a type cannot be inferred
    /// </summary>
    public sealed class TypeMemoryLayoutDetectionException : Exception
    {
        internal TypeMemoryLayoutDetectionException(string message)
            : base(message) { }
    }
}