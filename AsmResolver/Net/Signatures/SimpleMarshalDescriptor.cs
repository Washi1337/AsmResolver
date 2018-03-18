using AsmResolver.Net.Emit;

namespace AsmResolver.Net.Signatures
{
    public class SimpleMarshalDescriptor : MarshalDescriptor
    {
        private readonly NativeType _nativeType;

        public SimpleMarshalDescriptor(NativeType nativeType)
        {
            _nativeType = nativeType;
        }

        public override NativeType NativeType
        {
            get { return _nativeType; }
        }

        public override uint GetPhysicalLength()
        {
            return sizeof (byte);
        }

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte)NativeType);
        }
    }
}