using System;
using System.Collections.Generic;
using System.IO;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Emit
{
    /// <summary>
    /// Represents a buffer for constructing new blob metadata streams.
    /// </summary>
    public class BlobStreamBuffer : MetadataStreamBuffer
    {
        private readonly IDictionary<BlobSignature, uint> _signatureOffsetMapping = new Dictionary<BlobSignature, uint>();
        private readonly MetadataBuffer _parentBuffer;
        private uint _length;

        public BlobStreamBuffer(MetadataBuffer parentBuffer)
        {
            _parentBuffer = parentBuffer ?? throw new ArgumentNullException(nameof(parentBuffer));
            _length = 1;
        }

        public override string Name => "#Blob";

        public override uint Length => FileSegment.Align(_length, 4);

        /// <summary>
        /// Gets or creates a new index for the given blob signature.
        /// </summary>
        /// <param name="signature">The blob signature to get the index from.</param>
        /// <returns>The index.</returns>
        public uint GetBlobOffset(BlobSignature signature)
        {
            if (signature == null)
                return 0;

            if (!_signatureOffsetMapping.TryGetValue(signature, out uint offset))
            {
                _signatureOffsetMapping.Add(signature, offset = _length);
                uint signatureLength = signature.GetPhysicalLength(_parentBuffer);
                _length += signatureLength.GetCompressedSize() + signatureLength;
            }
            return offset;
        }

        public override MetadataStream CreateStream()
        {
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryStreamWriter(stream);
                writer.WriteByte(0);

                foreach (var signature in _signatureOffsetMapping.Keys)
                {
                    writer.WriteCompressedUInt32(signature.GetPhysicalLength(_parentBuffer));
                    signature.Write(_parentBuffer, writer);
                }

                writer.WriteZeroes((int) (FileSegment.Align(_length, 4) - _length));
                return new BlobStream(new MemoryStreamReader(stream.ToArray()));
            }
        }
    }
}