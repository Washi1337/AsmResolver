using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata;

namespace AsmResolver.DotNet.Builder.Metadata
{
    /// <summary>
    /// Provides a mutable buffer for building up a strings stream in a .NET portable executable.
    /// </summary>
    public class StringsStreamBuffer : IMetadataStreamBuffer
    {
        private Dictionary<Utf8String, StringIndex> _index = new();
        private List<StringsStreamBlob> _blobs = new();
        private uint _currentOffset = 1;
        private int _fixedBlobCount = 0;

        /// <summary>
        /// Creates a new strings stream buffer with the default strings stream name.
        /// </summary>
        public StringsStreamBuffer()
            : this(StringsStream.DefaultName)
        {
        }

        /// <summary>
        /// Creates a new strings stream buffer.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        public StringsStreamBuffer(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <inheritdoc />
        public string Name
        {
            get;
        }

        /// <inheritdoc />
        public bool IsEmpty => _blobs.Count == 0;

        /// <summary>
        /// Imports the contents of a strings stream and indexes all present strings.
        /// </summary>
        /// <param name="stream">The stream to import.</param>
        /// <exception cref="InvalidOperationException">Occurs when the stream buffer is not empty.</exception>
        public void ImportStream(StringsStream stream)
        {
            if (!IsEmpty)
                throw new InvalidOperationException("Cannot import a stream if the buffer is not empty.");

            uint offset = 1;
            uint size = stream.GetPhysicalSize();

            while (offset < size)
            {
                var value = stream.GetStringByIndex(offset)!;

                uint newOffset = AppendString(value, true);
                _index[value] = new StringIndex(_blobs.Count - 1, newOffset);

                offset += (uint) value.ByteCount + 1;
                _fixedBlobCount++;
            }
        }

        private uint AppendString(Utf8String value, bool isFixed)
        {
            uint offset = _currentOffset;
            _blobs.Add(new StringsStreamBlob(value, isFixed));
            _currentOffset += (uint)value.ByteCount + 1;
            return offset;
        }

        /// <summary>
        /// Gets the index to the provided string. If the string is not present in the buffer, it will be appended to
        /// the end of the stream.
        /// </summary>
        /// <param name="value">The string to lookup or add.</param>
        /// <returns>The index of the string.</returns>
        public uint GetStringIndex(Utf8String? value)
        {
            if (Utf8String.IsNullOrEmpty(value))
                return 0;

            if (Array.IndexOf(value.GetBytesUnsafe(), (byte) 0x00) >= 0)
                throw new ArgumentException("String contains a zero byte.");

            if (!_index.TryGetValue(value, out var offsetId))
            {
                uint offset = AppendString(value, false);
                offsetId = new StringIndex(_blobs.Count - 1, offset);
                _index.Add(value, offsetId);
            }

            return offsetId.Offset;
        }

        /// <summary>
        /// Optimizes the buffer by removing string entries that have a common suffix with another.
        /// </summary>
        /// <returns>A translation table that maps old offsets to the new ones after optimizing.</returns>
        /// <remarks>
        /// This method might invalidate all offsets obtained by <see cref="GetStringIndex"/>.
        /// </remarks>
        public IDictionary<uint, uint> Optimize()
        {
            uint finalOffset = 1;
            var newIndex = new Dictionary<Utf8String, StringIndex>();
            var newBlobs = new List<StringsStreamBlob>(_fixedBlobCount);
            var translationTable = new Dictionary<uint, uint>
            {
                [0] = 0
            };

            // Import fixed blobs.
            for (int i = 0; i < _fixedBlobCount; i++)
                AppendBlob(_blobs[i]);

            // Sort all blobs based on common suffix.
            var sortedEntries = _index.ToList();
            sortedEntries.Sort(StringsStreamBlobSuffixComparer.Instance);

            for (int i = 0; i < sortedEntries.Count; i++)
            {
                var currentEntry = sortedEntries[i];
                var currentBlob = _blobs[currentEntry.Value.BlobIndex];

                // Ignore blobs that are already added.
                if (currentBlob.IsFixed)
                    continue;

                if (i == 0)
                {
                    // First blob should always be added, since it has no common prefix with anything else.
                    AppendBlob(currentBlob);
                }
                else
                {
                    // Check if any blobs have a common suffix.
                    int reusedIndex = i;
                    while (reusedIndex > 0 && BytesEndsWith(sortedEntries[reusedIndex - 1].Key.GetBytesUnsafe(), currentEntry.Key.GetBytesUnsafe()))
                        reusedIndex--;

                    // Reuse blob if blob had a common suffix.
                    if (reusedIndex != i)
                        ReuseBlob(currentEntry, reusedIndex);
                    else
                        AppendBlob(currentBlob);
                }
            }

            // Replace contents of current buffer with the newly constructed buffer.
            _blobs = newBlobs;
            _index = newIndex;
            _currentOffset = finalOffset;
            return translationTable;

            void AppendBlob(in StringsStreamBlob blob)
            {
                newBlobs.Add(blob);

                var oldIndex = _index[blob.Blob];
                translationTable[oldIndex.Offset] = finalOffset;
                newIndex[blob.Blob] = new StringIndex(newBlobs.Count - 1, finalOffset);

                finalOffset += blob.GetPhysicalSize();
            }

            void ReuseBlob(in KeyValuePair<Utf8String, StringIndex> currentEntry, int reusedIndex)
            {
                var reusedEntry = sortedEntries[reusedIndex];
                uint reusedEntryNewOffset = translationTable[reusedEntry.Value.Offset];
                int relativeOffset = reusedEntry.Key.ByteCount - currentEntry.Key.ByteCount;
                uint newOffset = (uint)(reusedEntryNewOffset + relativeOffset);

                translationTable[currentEntry.Value.Offset] = newOffset;
                newIndex[currentEntry.Key] = new StringIndex(reusedEntry.Value.BlobIndex, newOffset);
            }
        }

        private static bool BytesEndsWith(byte[] x, byte[] y)
        {
            if (x.Length < y.Length)
                return false;

            for (int i = x.Length - 1, j = y.Length - 1; i >= 0 && j >= 0; i--, j--)
            {
                if (x[i] != y[j])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Serializes the strings stream buffer to a metadata stream.
        /// </summary>
        /// <returns>The metadata stream.</returns>
        public StringsStream CreateStream()
        {
            using var outputStream = new MemoryStream();

            var writer = new BinaryStreamWriter(outputStream);
            writer.WriteByte(0);

            foreach (var blob in _blobs)
                blob.Write(writer);

            writer.Align(4);

            return new SerializedStringsStream(Name, outputStream.ToArray());
        }

        /// <inheritdoc />
        IMetadataStream IMetadataStreamBuffer.CreateStream() => CreateStream();
    }
}
