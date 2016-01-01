using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices;
using AsmResolver.Builder;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net
{
    /// <summary>
    /// Represents a blob storage stream (#Blob) in a .NET assembly image.
    /// </summary>
    public class BlobStream : MetadataStream<BlobStreamBuffer>
    {
        internal static BlobStream FromReadingContext(ReadingContext context)
        {
            return new BlobStream(context.Reader);
        }

        private readonly IBinaryStreamReader _reader;

        public BlobStream()
        {
        }

        internal BlobStream(IBinaryStreamReader reader)
        {
            _reader = reader;
        }

        /// <summary>
        /// Gets the blob at the given index.
        /// </summary>
        /// <param name="offset">The index of the blob to get.</param>
        /// <returns>The raw blob data.</returns>
        public byte[] GetBlobByOffset(uint offset)
        {
            if (offset == 0)
                return new byte[0];

            var reader = CreateBlobReader(offset);
            return reader.ReadBytes((int)reader.Length);
        }
        
        /// <summary>
        /// Tries to create a new blob reader starting at the given offset.
        /// </summary>
        /// <param name="offset">The index of the blob to read.</param>
        /// <param name="reader">The reader that was created.</param>
        /// <returns><c>True</c> if the reader was created successfully, false otherwise.</returns>
        public bool TryCreateBlobReader(uint offset, out IBinaryStreamReader reader)
        {
            try
            {
                reader = CreateBlobReader(offset);
                return true;
            }
            catch
            {
                reader = null;
                return false;
            }
        }

        /// <summary>
        /// Creates a new blob reader starting at the given offset.
        /// </summary>
        /// <param name="offset">The index of the blob to read.</param>
        /// <returns>The blob reader.</returns>
        public IBinaryStreamReader CreateBlobReader(uint offset)
        {
            var reader = _reader.CreateSubReader(_reader.StartPosition + offset);
            uint length;
            reader.TryReadCompressedUInt32(out length);
            return reader.CreateSubReader(reader.Position, (int)length);
        }

        /// <summary>
        /// Creates a new buffer for constructing a new blob storage stream.
        /// </summary>
        /// <returns></returns>
        public override BlobStreamBuffer CreateBuffer()
        {
            return new BlobStreamBuffer();
        }

        public override uint GetPhysicalLength()
        {
            return (uint)_reader.Length;
        }

        public override void Write(WritingContext context)
        {
            context.Writer.WriteZeroes((int)_reader.Length);
        }

    }

    /// <summary>
    /// Represents a buffer for constructing new blob metadata streams.
    /// </summary>
    public class BlobStreamBuffer : FileSegment
    {
        private readonly Dictionary<BlobSignature, uint> _signatureOffsetMapping = new Dictionary<BlobSignature, uint>();
        private uint _length;

        public BlobStreamBuffer()
        {
            _length = 1;
        }

        /// <summary>
        /// Gets or creates a new index for the given blob signature.
        /// </summary>
        /// <param name="signature">The blob signature to get the index from.</param>
        /// <returns>The index.</returns>
        public uint GetBlobOffset(BlobSignature signature)
        {
            if (signature == null)
                return 0;

            uint offset;
            if (!_signatureOffsetMapping.TryGetValue(signature, out offset))
            {
                _signatureOffsetMapping.Add(signature, offset = _length);
                var signatureLength = signature.GetPhysicalLength();
                _length += signatureLength.GetCompressedSize() + signatureLength;
            }
            return offset;
        }

        public override uint GetPhysicalLength()
        {
            return Align(_length, 4);
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteByte(0);

            foreach (var signature in _signatureOffsetMapping.Keys)
            {
                writer.WriteCompressedUInt32(signature.GetPhysicalLength());
                signature.Write(context);
            }


        }
    }
}
