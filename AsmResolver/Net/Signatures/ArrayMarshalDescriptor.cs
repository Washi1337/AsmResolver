using AsmResolver.Net.Emit;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Represents a marshal descriptor that describes how a native array should be marshaled upon calling to or from
    /// unmanaged code via P/Invoke dispatch.
    /// </summary>
    public class ArrayMarshalDescriptor : MarshalDescriptor
    {
        /// <summary>
        /// Reads a single array marshal descriptor at the current position of the binary stream reader.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The read array descriptor.</returns>
        /// <remarks>
        /// This method assumes the native type has already been read from the binary stream reader.
        /// </remarks>
        public static ArrayMarshalDescriptor FromReader(IBinaryStreamReader reader)
        {
            var descriptor = new ArrayMarshalDescriptor((NativeType) reader.ReadByte());

            if (!reader.TryReadCompressedUInt32(out uint value))
                return descriptor;
            descriptor.ParameterIndex = (int) value;
            
            if (!reader.TryReadCompressedUInt32(out value))
                return descriptor;
            descriptor.NumberOfElements = (int) value;
            
            return descriptor;
        }

        public ArrayMarshalDescriptor(NativeType elementType)
        {
            ElementType = elementType;
        }

        /// <inheritdoc />
        public override NativeType NativeType => NativeType.Array;

        /// <summary>
        /// Gets or sets the type of the elements stored in the array.
        /// </summary>
        public NativeType ElementType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the index of the parameter that is marshaled (if available).
        /// </summary>
        public int? ParameterIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the fixed number of elements the parameter contains (if available).
        /// </summary>
        public int? NumberOfElements
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return 2 * sizeof (byte) +
                   (ParameterIndex.HasValue
                       ? ParameterIndex.Value.GetCompressedSize() +
                         (NumberOfElements.HasValue ? NumberOfElements.Value.GetCompressedSize() : 0)
                       : 0)
                + base.GetPhysicalLength(buffer);
        }

        /// <inheritdoc />
        public override void Prepare(MetadataBuffer buffer)
        {
        }

        /// <inheritdoc />
        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte)NativeType);
            writer.WriteByte((byte)ElementType);
            if (ParameterIndex.HasValue)
            {
                writer.WriteCompressedUInt32((uint)ParameterIndex.Value);
                if (NumberOfElements.HasValue)
                    writer.WriteCompressedUInt32((uint)NumberOfElements.Value);
            }

            base.Write(buffer, writer);
        }
    }
}