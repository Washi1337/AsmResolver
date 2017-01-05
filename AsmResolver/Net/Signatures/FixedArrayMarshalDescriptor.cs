using System;

namespace AsmResolver.Net.Signatures
{
    public class FixedArrayMarshalDescriptor : MarshalDescriptor
    {
        public new static FixedArrayMarshalDescriptor FromReader(IBinaryStreamReader reader)
        {
            var descriptor = new FixedArrayMarshalDescriptor()
            {
                StartOffset = reader.Position,
            };

            uint value;
            if (!reader.TryReadCompressedUInt32(out value))
                return descriptor;
            descriptor.NumberOfElements = (int)value;

            if (reader.CanRead(sizeof(byte)))
                descriptor.ElementType = (NativeType)reader.ReadByte();
            return descriptor;
        }

        public override NativeType NativeType
        {
            get { return NativeType.FixedArray; }
        }

        public int NumberOfElements
        {
            get;
            set;
        }

        public NativeType ElementType
        {
            get;
            set;
        }

        public override uint GetPhysicalLength()
        {
            return sizeof (byte) +
                   NumberOfElements.GetCompressedSize() +
                   sizeof (byte);
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteByte((byte)NativeType);
            writer.WriteCompressedUInt32((uint)NumberOfElements);
            writer.WriteByte((byte)ElementType);
        }
    }
}