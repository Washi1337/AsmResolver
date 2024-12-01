using System;
using System.Collections.Generic;

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Represents the metadata stream containing GUIDs referenced by entries in the tables stream.
    /// </summary>
    /// <remarks>
    /// Like most metadata streams, the GUID stream does not necessarily contain just valid strings. It can contain
    /// (garbage) data that is never referenced by any of the tables in the tables stream. The only guarantee that the
    /// GUID heap provides, is that any blob index in the tables stream is the start address (relative to the start of
    /// the GUID stream) of a GUID.
    /// </remarks>
    public abstract class GuidStream : MetadataHeap
    {
        /// <summary>
        /// The size of a single GUID in the GUID stream.
        /// </summary>
        public const int GuidSize = 16;

        /// <summary>
        /// The default name of a GUID stream, as described in the specification provided by ECMA-335.
        /// </summary>
        public const string DefaultName = "#GUID";

        /// <summary>
        /// Initializes the GUID stream with its default name.
        /// </summary>
        protected GuidStream()
            : base(DefaultName)
        {
        }

        /// <summary>
        /// Initializes the GUID stream with a custom name.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        protected GuidStream(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets a GUID by its GUID index.
        /// </summary>
        /// <param name="index">The offset into the heap to start reading.</param>
        /// <returns>The GUID.</returns>
        public abstract System.Guid GetGuidByIndex(uint index);

        /// <summary>
        /// Searches the stream for the provided GUID.
        /// </summary>
        /// <param name="guid">The GUID to search for.</param>
        /// <param name="index">When the function returns <c>true</c>, contains the index at which the GUID was found.</param>
        /// <returns><c>true</c> if the GUID index was found, <c>false</c> otherwise.</returns>
        public abstract bool TryFindGuidIndex(System.Guid guid, out uint index);

        /// <summary>
        /// Performs a linear sweep on the stream and yields all GUIDs that are stored.
        /// </summary>
        /// <returns>The GUID enumerator.</returns>
        public abstract IEnumerable<Guid> EnumerateGuids();
    }
}
