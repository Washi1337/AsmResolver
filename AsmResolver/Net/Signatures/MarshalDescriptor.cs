namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// When derived from this class, provides a description on how a specific type needs to be marshaled upon
    /// calling to or from unmanaged code via P/Invoke dispatch.
    /// </summary>
    public abstract class MarshalDescriptor : ExtendableBlobSignature 
    {
        /// <summary>
        /// Reads a single marshal descriptor at the current position of the provided binary stream reader.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <param name="readToEnd">
        /// Determines whether any extra data appearing after the signature should be read and 
        /// put in the <see cref="ExtendableBlobSignature.ExtraData"/> property.
        /// </param>
        /// <returns>The read marshal descriptor.</returns>
        public static MarshalDescriptor FromReader(IBinaryStreamReader reader, bool readToEnd = false)
        {
            var descriptor = ReadMarshalDescriptor(reader);
            if (readToEnd)
                descriptor.ExtraData = reader.ReadToEnd();
            return descriptor;
        }

        private static MarshalDescriptor ReadMarshalDescriptor(IBinaryStreamReader reader)
        {
            var type = (NativeType) reader.ReadByte();
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

        /// <summary>
        /// Gets the native type of the marshal descriptor. This is the byte any descriptor starts with. 
        /// </summary>
        public abstract NativeType NativeType
        {
            get;
        }
    }
}
