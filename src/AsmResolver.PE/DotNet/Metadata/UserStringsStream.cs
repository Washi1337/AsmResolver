using System.Collections.Generic;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Represents the metadata streams containing the user strings referenced by CIL method bodies.
    /// </summary>
    /// <remarks>
    /// Like most metadata streams, the user strings stream does not necessarily contain just valid strings. It can contain
    /// (garbage) data that is never referenced by any of the tables in the tables stream. The only guarantee that the
    /// strings heap provides, is that any string index in a CIL method body is the start address (relative to the start
    /// of the #US stream) of a unicode string, prefixed by a length, and suffixed by one extra terminator byte.
    /// </remarks>
    public abstract class UserStringsStream : MetadataHeap
    {
        /// <summary>
        /// The default name of a user-strings stream, as described in the specification provided by ECMA-335.
        /// </summary>
        public const string DefaultName = "#US";

        /// <summary>
        /// Initializes the user-strings stream with its default name.
        /// </summary>
        protected UserStringsStream()
            : base(DefaultName)
        {
        }

        /// <summary>
        /// Initializes the user-strings stream with a custom name.
        /// </summary>
        protected UserStringsStream(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets a string by its string index.
        /// </summary>
        /// <param name="index">The offset into the heap to start reading.</param>
        /// <returns>The string, or <c>null</c> if the index was invalid.</returns>
        public abstract string? GetStringByIndex(uint index);

        /// <summary>
        /// Searches the stream for the provided string.
        /// </summary>
        /// <param name="value">The string to search for.</param>
        /// <param name="index">When the function returns <c>true</c>, contains the index at which the string was found.</param>
        /// <returns><c>true</c> if the string index was found, <c>false</c> otherwise.</returns>
        public abstract bool TryFindStringIndex(string value, out uint index);

        /// <summary>
        /// Searches the stream for the provided string.
        /// </summary>
        /// <param name="value">The string to search for.</param>
        /// <param name="token">When the function returns <c>true</c>, contains the token for which the string was found.</param>
        /// <returns><c>true</c> if the string token was found, <c>false</c> otherwise.</returns>
        public bool TryFindStringToken(string value, out MetadataToken token)
        {
            if (TryFindStringIndex(value, out uint index))
            {
                token = new MetadataToken(TableIndex.String, index);
                return true;
            }

            token = MetadataToken.Zero;
            return false;
        }

        /// <summary>
        /// Performs a linear sweep on the stream and yields all strings that are stored.
        /// </summary>
        /// <returns>The strings enumerator.</returns>
        /// <remarks>
        /// As strings can be referenced at any offset within the heap, the heap is technically allowed to contain
        /// garbage data in between string entries. As such, this linear sweep enumerator may not be an accurate
        /// depiction of all strings that are used by the module.
        /// </remarks>
        public abstract IEnumerable<(uint Index, string String)> EnumerateStrings();
    }
}
