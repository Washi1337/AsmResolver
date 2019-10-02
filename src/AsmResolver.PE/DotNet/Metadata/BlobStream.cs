// AsmResolver - Executable file format inspection library 
// Copyright (C) 2016-2019 Washi
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA

namespace AsmResolver.PE.DotNet.Metadata
{
    /// <summary>
    /// Represents the metadata stream containing blob signatures referenced by entries in the tables stream. 
    /// </summary>
    /// <remarks>
    /// Like most metadata streams, the blob stream does not necessarily contain just valid strings. It can contain
    /// (garbage) data that is never referenced by any of the tables in the tables stream. The only guarantee that the
    /// blob heap provides, is that any blob index in the tables stream is the start address (relative to the start of
    /// the blob stream) of a blob signature that is prefixed by a length.
    /// </remarks>
    public abstract class BlobStream : IMetadataStream
    {
        public const string DefaultName = "#Blob";

        /// <inheritdoc />
        public string Name
        {
            get;
            set;
        } = DefaultName;

        /// <inheritdoc />
        public abstract bool CanRead
        {
            get;
        }

        /// <inheritdoc />
        public abstract IBinaryStreamReader CreateReader();

        /// <summary>
        /// Gets a blob by its blob index.
        /// </summary>
        /// <param name="index">The offset into the heap to start reading.</param>
        /// <returns>
        /// The blob, excluding the bytes encoding the length of the blob, or <c>null</c> if the index was invalid.
        /// </returns>
        public abstract byte[] GetBlobByIndex(int index);

        /// <summary>
        /// Gets a blob binary reader by its blob index.
        /// </summary>
        /// <param name="index">The offset into the heap to start reading.</param>
        /// <returns>
        /// The blob reader, starting at the first byte after the length of the blob, or <c>null</c> if the index
        /// was invalid.
        /// </returns>
        public abstract IBinaryStreamReader GetBlobReaderByIndex(int index);
    }
}