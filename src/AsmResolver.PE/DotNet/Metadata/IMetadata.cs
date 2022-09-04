using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Represents a data directory containing metadata for a managed executable, including fields from the metadata
    /// header, as well as the streams containing metadata tables and blob signatures.
    /// </summary>
    public interface IMetadata : ISegment
    {
        /// <summary>
        /// Gets or sets the major version of the metadata directory format.
        /// </summary>
        /// <remarks>
        /// This field is usually set to 1.
        /// </remarks>
        ushort MajorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minor version of the metadata directory format.
        /// </summary>
        /// <remarks>
        /// This field is usually set to 1.
        /// </remarks>
        ushort MinorVersion
        {
            get;
            set;
        }

        /// <summary>
        /// Reserved for future use.
        /// </summary>
        uint Reserved
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the string containing the runtime version that the .NET binary was built for.
        /// </summary>
        string VersionString
        {
            get;
            set;
        }

        /// <summary>
        /// Reserved for future use.
        /// </summary>
        ushort Flags
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the metadata directory is loaded as Edit-and-Continue metadata.
        /// </summary>
        public bool IsEnCMetadata
        {
            get;
        }

        /// <summary>
        /// Gets a collection of metadata streams that are defined in the metadata header.
        /// </summary>
        IList<IMetadataStream> Streams
        {
            get;
        }

        /// <summary>
        /// Gets a stream by its name.
        /// </summary>
        /// <param name="name">The name of the stream to search.</param>
        /// <returns>The stream</returns>
        /// <exception cref="KeyNotFoundException">Occurs when the stream is not present in the metadata directory.</exception>
        IMetadataStream GetStream(string name);

        /// <summary>
        /// Gets a stream by its type.
        /// </summary>
        /// <typeparam name="TStream">The type of the stream.</typeparam>
        /// <returns>The stream</returns>
        /// <exception cref="KeyNotFoundException">Occurs when the stream is not present in the metadata directory.</exception>
        TStream GetStream<TStream>()
            where TStream : class, IMetadataStream;

        /// <summary>
        /// Gets a stream by its name.
        /// </summary>
        /// <param name="name">The name of the stream to search.</param>
        /// <param name="stream">The found stream, or <c>null</c> if no match was found.</param>
        /// <returns><c>true</c> if a match was found, <c>false</c> otherwise.</returns>
        bool TryGetStream(string name, [NotNullWhen(true)] out IMetadataStream? stream);

        /// <summary>
        /// Gets a stream by its name.
        /// </summary>
        /// <typeparam name="TStream">The type of the stream.</typeparam>
        /// <param name="stream">The found stream, or <c>null</c> if no match was found.</param>
        /// <returns><c>true</c> if a match was found, <c>false</c> otherwise.</returns>
        bool TryGetStream<TStream>([NotNullWhen(true)] out TStream? stream)
            where TStream : class, IMetadataStream;
    }
}
