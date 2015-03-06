namespace AsmResolver.Net.Signatures
{
    public abstract class MarshalDescriptor : BlobSignature
    {
        public static MarshalDescriptor FromReader(IBinaryStreamReader reader)
        {
            var type = (NativeType)reader.ReadByte();
            switch (type)
            {
                case NativeType.Array:
                    return ArrayMarshalDescriptor.FromReader(reader);
                case NativeType.FixedArray:
                    return FixedArrayMarshalDescriptor.FromReader(reader);
                case NativeType.SafeArray:
                    return SafeArrayMarshalDescriptor.FromReader(reader);
                case NativeType.CustomMarshaler:
                    return CustomMarshalDescriptor.FromReader(reader);
                default:
                    return new SimpleMarshalDescriptor(type);
            }
        }
        
        public abstract NativeType NativeType
        {
            get;
        }
    }
}
