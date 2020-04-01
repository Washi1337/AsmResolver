using System;

namespace AsmResolver.DotNet.Analysis
{
    /// <summary>
    /// Gets thrown when a calculation error occurs in <see cref="SizeCalculator"/>
    /// </summary>
    public sealed class SizeCalculationException : Exception
    {
        internal SizeCalculationException(string message)
            : base(message) { }
    }
}