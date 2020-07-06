using System;

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Represents a single metadata stream in the metadata directory of a managed executable file.
    /// </summary>
    public interface IMetadataStream : ISegment
    {
        /// <summary>
        /// Gets or sets the name of the metadata stream.
        /// </summary>
        string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the raw contents of the stream can be read using a binary stream reader. 
        /// </summary>
        bool CanRead
        {
            get;
        }

        /// <summary>
        /// Creates a binary reader that reads the raw contents of the metadata stream.
        /// </summary>
        /// <returns>The reader.</returns>
        /// <exception cref="InvalidOperationException">Occurs when <see cref="CanRead"/> is <c>false</c>.</exception>
        IBinaryStreamReader CreateReader();
    }
}