using System;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.ReadyToRun
{
    /// <summary>
    /// Represents a single section within a ReadyToRun directory of a .NET module.
    /// </summary>
    public interface IReadyToRunSection : ISegment
    {
        /// <summary>
        /// Gets the type of the ReadyToRun section.
        /// </summary>
        ReadyToRunSectionType Type
        {
            get;
        }

        /// <summary>
        /// Indicates whether the raw contents of the section can be read using a <see cref="BinaryStreamReader"/>.
        /// </summary>
        public bool CanRead
        {
            get;
        }

        /// <summary>
        /// Creates a binary reader that reads the raw contents of the ReadyToRun section.
        /// </summary>
        /// <returns>The reader.</returns>
        /// <exception cref="InvalidOperationException">Occurs when <see cref="CanRead"/> is <c>false</c>.</exception>
        BinaryStreamReader CreateReader();
    }
}
