using System;
using AsmResolver.IO;

namespace AsmResolver.PE.DotNet.Metadata.Blob
{
    /// <summary>
    /// Provides an implementation of a blob stream that obtains blobs from a readable segment in a file.
    /// </summary>
    public class SerializedBlobStream : BlobStream
    {
        private readonly BinaryStreamReader _reader;

        /// <summary>
        /// Creates a new blob stream based on a byte array.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="rawData">The raw contents of the stream.</param>
        public SerializedBlobStream(string name, byte[] rawData)
            : this(name, ByteArrayDataSource.CreateReader(rawData))
        {
        }

        /// <summary>
        /// Creates a new blob stream based on a segment in a file.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        /// <param name="reader">The raw contents of the stream.</param>
        public SerializedBlobStream(string name, in BinaryStreamReader reader)
            : base(name)
        {
            _reader = reader;
        }

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override BinaryStreamReader CreateReader() => _reader.Fork();

        /// <inheritdoc />
        public override uint GetPhysicalSize() => _reader.Length;

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer) => _reader.Fork().WriteToOutput(writer);

        /// <inheritdoc />
        public override byte[]? GetBlobByIndex(uint index) => TryGetBlobReaderByIndex(index, out var reader)
            ? reader.ReadToEnd()
            : null;

        /// <inheritdoc />
        public override bool TryGetBlobReaderByIndex(uint index, out BinaryStreamReader reader)
        {
            if (index == 0 || index >= _reader.Length)
            {
                reader = default;
                return false;
            }

            reader = _reader.ForkRelative(index);
            if (reader.TryReadCompressedUInt32(out uint length))
            {
                uint headerSize = (uint) (reader.Offset - reader.StartOffset);
                reader.ChangeSize(Math.Min(length + headerSize, reader.Length));
                return true;
            }

            return false;
        }

    }
}
