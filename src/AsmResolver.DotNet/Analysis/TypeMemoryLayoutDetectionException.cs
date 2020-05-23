using System;

namespace AsmResolver.DotNet.Analysis
{
    /// <summary>
    /// Gets thrown when the layout detection fails
    /// </summary>
    public sealed class TypeMemoryLayoutDetectionException : Exception
    {
        internal TypeMemoryLayoutDetectionException(string message)
            : base(message) { }
    }
}