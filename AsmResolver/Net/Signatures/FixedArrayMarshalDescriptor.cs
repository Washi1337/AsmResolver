using AsmResolver.Net.Emit;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Represents a marshal descriptor that describes how a native array with a fixed length should be marshaled upon
    /// calling to or from unmanaged code via P/Invoke dispatch.
    /// </summary>
    public class FixedArrayMarshalDescriptor : MarshalDescriptor
    {
        /// <summary>
        /// Reads a single array marshal descriptor at the current position of the binary stream reader.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The read array descriptor.</returns>
        /// <remarks>
        /// This method assumes the native type has already been read from the binary stream reader.
        /// </remarks>
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

        /// <inheritdoc />
        public override NativeType NativeType => NativeType.FixedArray;

        /// <summary>
        /// Gets or sets the number of elements the array has.
        /// </summary>
        public int NumberOfElements
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of the elements stored in the array.
        /// </summary>
        public NativeType ElementType
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return sizeof(byte) +
                   NumberOfElements.GetCompressedSize() +
                   sizeof(byte) +
                   base.GetPhysicalLength(buffer);
        }

        /// <inheritdoc />
        public override void Prepare(MetadataBuffer buffer)
        {
        }

        /// <inheritdoc />
        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte)NativeType);
            writer.WriteCompressedUInt32((uint)NumberOfElements);
            writer.WriteByte((byte)ElementType);

            base.Write(buffer, writer);
        }
    }
}