using System;
using System.Collections.Generic;
using System.IO;
using AsmResolver.DotNet.Signatures;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata;

namespace AsmResolver.DotNet.Builder.Metadata
{
    /// <summary>
    /// Provides a mutable buffer for building up a blob stream in a .NET portable executable.
    /// </summary>
    public class BlobStreamBuffer : IMetadataStreamBuffer
    {
        private readonly MemoryStream _rawStream = new();
        private readonly BinaryStreamWriter _writer;
        private readonly Dictionary<byte[], uint> _blobs = new(ByteArrayEqualityComparer.Instance);

        private readonly MemoryStreamWriterPool _blobWriterPool = new();

        /// <summary>
        /// Creates a new blob stream buffer with the default blob stream name.
        /// </summary>
        public BlobStreamBuffer()
            : this(BlobStream.DefaultName)
        {
        }

        /// <summary>
        /// Creates a new blob stream buffer.
        /// </summary>
        /// <param name="name">The name of the stream.</param>
        public BlobStreamBuffer(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _writer = new BinaryStreamWriter(_rawStream);
            _writer.WriteByte(0);
        }

        /// <inheritdoc />
        public string Name
        {
            get;
        }

        /// <inheritdoc />
        public bool IsEmpty => _rawStream.Length <= 1;

        /// <summary>
        /// Imports the contents of a user strings stream and indexes all present strings.
        /// </summary>
        /// <param name="stream">The stream to import.</param>
        public void ImportStream(BlobStream stream)
        {
            MetadataStreamBufferHelper.CloneBlobHeap(stream, _writer, (index, newIndex) =>
            {
                if (stream.GetBlobByIndex(index) is { } blob)
                    _blobs[blob] = newIndex;
            });
        }

        /// <summary>
        /// Appends raw data to the stream.
        /// </summary>
        /// <param name="data">The data to append.</param>
        /// <returns>The index to the start of the data.</returns>
        /// <remarks>
        /// This method does not index the blob data. Calling <see cref="AppendRawData"/> or <see cref="GetBlobIndex(byte[])"/>
        /// on the same data will append the data a second time.
        /// </remarks>
        public uint AppendRawData(byte[] data)
        {
            uint offset = (uint) _rawStream.Length;
            _writer.WriteBytes(data, 0, data.Length);
            return offset;
        }

        private uint AppendBlob(byte[] blob)
        {
            uint offset = (uint) _rawStream.Length;
            _writer.WriteCompressedUInt32((uint) blob.Length);
            AppendRawData(blob);
            return offset;
        }

        /// <summary>
        /// Gets the index to the provided blob. If the blob is not present in the buffer, it will be appended to the end
        /// of the stream.
        /// </summary>
        /// <param name="blob">The blob to lookup or add.</param>
        /// <returns>The index of the blob.</returns>
        public uint GetBlobIndex(byte[]? blob)
        {
            if (blob is null || blob.Length == 0)
                return 0;

            if (!_blobs.TryGetValue(blob, out uint offset))
            {
                offset = AppendBlob(blob);
                _blobs.Add(blob, offset);
            }

            return offset;
        }

        /// <summary>
        /// Gets the index to the provided blob signature. If the signature is not present in the buffer, it will be
        /// appended to the end of the stream.
        /// </summary>
        /// <param name="provider">The object to use for obtaining metadata tokens for members in the tables stream.</param>
        /// <param name="signature">The signature to lookup or add.</param>
        /// <param name="errorListener">The object responsible for collecting diagnostic information.</param>
        /// <param name="diagnosticSource">The object that is reported back when serialization of the blob fails.</param>
        /// <returns>The index of the signature.</returns>
        public uint GetBlobIndex(ITypeCodedIndexProvider provider, BlobSignature? signature, IErrorListener errorListener, object? diagnosticSource = null)
        {
            if (signature is null)
                return 0u;

            // Serialize blob.

            using var rentedWriter = _blobWriterPool.Rent();
            signature.Write(new BlobSerializationContext(rentedWriter.Writer, provider, errorListener, diagnosticSource));

            return GetBlobIndex(rentedWriter.GetData());
        }

        /// <summary>
        /// Serialises the blob stream buffer to a metadata stream.
        /// </summary>
        /// <returns>The metadata stream.</returns>
        public BlobStream CreateStream()
        {
            _writer.Align(4);
            return new SerializedBlobStream(Name, _rawStream.ToArray());
        }

        /// <inheritdoc />
        IMetadataStream IMetadataStreamBuffer.CreateStream() => CreateStream();
    }
}
