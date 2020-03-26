namespace AsmResolver.DotNet.Signatures
{
    public class FixedArrayMarshalDescriptor : MarshalDescriptor
    {
        public static FixedArrayMarshalDescriptor FromReader(IBinaryStreamReader reader)
        {
            var result = new FixedArrayMarshalDescriptor();

            if (reader.TryReadCompressedUInt32(out uint value))
            {
                result.Size = (int) value;
                if (reader.TryReadCompressedUInt32(out value))
                    result.ArrayElementType = (NativeType) value;
            }

            return result;
        }
        
        /// <summary>
        /// Creates a new instance of the <see cref="FixedArrayMarshalDescriptor"/> class.
        /// </summary>
        public FixedArrayMarshalDescriptor()
        {
        }
        
        /// <summary>
        /// Creates a new instance of the <see cref="FixedArrayMarshalDescriptor"/> class.
        /// </summary>
        /// <param name="size">The fixed size of the array.</param>
        public FixedArrayMarshalDescriptor(int size)
        {
            Size = size;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FixedArrayMarshalDescriptor"/> class.
        /// </summary>
        /// <param name="size">The fixed size of the array.</param>
        /// <param name="arrayElementType">The type each element in the array should be marshalled as.</param>
        public FixedArrayMarshalDescriptor(int size, NativeType arrayElementType)
        {
            Size = size;
            ArrayElementType = arrayElementType;
        }
        
        /// <inheritdoc />
        public override NativeType NativeType => NativeType.FixedArray;

        /// <summary>
        /// Gets or sets the fixed size of the array.
        /// </summary>
        public int? Size
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type each element in the array should be marshalled as.
        /// </summary>
        public NativeType? ArrayElementType
        {
            get;
            set;
        }

        /// <inheritdoc />
        protected override void WriteContents(IBinaryStreamWriter writer, ITypeCodedIndexProvider provider)
        {
            writer.WriteByte((byte) NativeType);
            if (Size.HasValue)
            {
                writer.WriteCompressedUInt32((uint) Size.Value);
                if (ArrayElementType.HasValue)
                    writer.WriteCompressedUInt32((uint) ArrayElementType);
            }
        }
    }
}