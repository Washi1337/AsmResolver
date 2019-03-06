using AsmResolver.Net.Emit;

namespace AsmResolver.Net.Signatures
{
    public class FixedArrayMarshalDescriptor : MarshalDescriptor
    {
        public static FixedArrayMarshalDescriptor FromReader(IBinaryStreamReader reader)
        {
            var descriptor = new FixedArrayMarshalDescriptor();

            if (!reader.TryReadCompressedUInt32(out uint value))
                return descriptor;
            descriptor.NumberOfElements = (int)value;

            if (reader.CanRead(sizeof(byte)))
                descriptor.ElementType = (NativeType)reader.ReadByte();
            
            return descriptor;
        }

        public override NativeType NativeType => NativeType.FixedArray;

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

        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return sizeof(byte) +
                   NumberOfElements.GetCompressedSize() +
                   sizeof(byte) +
                   base.GetPhysicalLength(buffer);
        }

        public override void Prepare(MetadataBuffer buffer)
        {
        }

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte)NativeType);
            writer.WriteCompressedUInt32((uint)NumberOfElements);
            writer.WriteByte((byte)ElementType);

            base.Write(buffer, writer);
        }
    }
}