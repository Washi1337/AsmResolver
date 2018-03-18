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
            if (parentBuffer == null) 
                throw new ArgumentNullException("parentBuffer");
            _parentBuffer = parentBuffer;
            _length = 1;
        }

        public override string Name
        {
            get { return "#Blob"; }
        }
        
        public override uint Length
        {
            get { return FileSegment.Align(_length, 4); }
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
                uint signatureLength = signature.GetPhysicalLength();
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

                var processedSignatures = new HashSet<BlobSignature>();
                var agenda = new Queue<BlobSignature>(_signatureOffsetMapping.Keys);

                while (agenda.Count > 0)
                {
                    var signature = agenda.Dequeue();
                    if (processedSignatures.Add(signature))
                    {
                        writer.WriteCompressedUInt32(signature.GetPhysicalLength());

                        int count = _signatureOffsetMapping.Count;
                        signature.Write(_parentBuffer, writer);

                        // TODO: find more efficient way of adding newly created signatures to the queue.
                        if (count != _signatureOffsetMapping.Count)
                        {
                            foreach (var sig in _signatureOffsetMapping.Keys)
                                agenda.Enqueue(sig);
                        }
                    }
                }

                writer.WriteZeroes((int)(FileSegment.Align(_length, 4) - _length));
                return new BlobStream(new MemoryStreamReader(stream.ToArray()));
            }
        }
    }
}