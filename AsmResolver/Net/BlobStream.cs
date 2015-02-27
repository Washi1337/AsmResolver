using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices;
using AsmResolver.Builder;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net
{
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

        public byte[] GetBlobByOffset(uint offset)
        {
            if (offset == 0)
                return new byte[0];

            var reader = CreateBlobReader(offset);
            return reader.ReadBytes((int)reader.Length);
        }

        public IBinaryStreamReader CreateBlobReader(uint offset)
        {
            var reader = _reader.CreateSubReader(_reader.StartPosition + offset);
            var length = reader.ReadCompressedUInt32();
            return reader.CreateSubReader(reader.Position, (int)length);
        }

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

    public class BlobStreamBuffer : FileSegment
    {
        private readonly Dictionary<BlobSignature, uint> _signatureOffsetMapping = new Dictionary<BlobSignature, uint>();
        private uint _length;

        public BlobStreamBuffer()
        {
            _length = 1;
        }

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
