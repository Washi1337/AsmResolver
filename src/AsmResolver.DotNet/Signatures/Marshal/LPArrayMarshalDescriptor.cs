using System.Runtime.InteropServices;

namespace AsmResolver.DotNet.Signatures.Marshal
{
    /// <summary>
    /// Represents the marshal descriptor for a pointer to the first element of a C-style array.
    /// </summary>
    public class LPArrayMarshalDescriptor : MarshalDescriptor
    {
        /// <summary>
        /// Reads a single array marshal descriptor at the current position of the binary stream reader.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The read array descriptor.</returns>
        /// <remarks>
        /// This method assumes the native type has already been read from the binary stream reader.
        /// </remarks>
        public static LPArrayMarshalDescriptor FromReader(IBinaryStreamReader reader)
        {
            var descriptor = new LPArrayMarshalDescriptor((NativeType) reader.ReadByte());

            if (!reader.TryReadCompressedUInt32(out uint value))
                return descriptor;
            descriptor.ParameterIndex = (int) value;

            if (!reader.TryReadCompressedUInt32(out value))
                return descriptor;
            descriptor.NumberOfElements = (int) value;

            if (!reader.TryReadCompressedUInt32(out value))
                return descriptor;
            descriptor.Flags = (LPArrayFlags) value;

            return descriptor;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LPArrayMarshalDescriptor"/> class.
        /// </summary>
        /// <param name="arrayElementType">The type of elements stored in the array.</param>
        public LPArrayMarshalDescriptor(NativeType arrayElementType)
        {
            ArrayElementType = arrayElementType;
        }
        
        /// <inheritdoc />
        public override NativeType NativeType => NativeType.LPArray;

        /// <summary>
        /// Gets the type of elements stored in the array.
        /// </summary>
        public NativeType ArrayElementType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the index of the parameter that is marshalled (if available).
        /// </summary>
        public int? ParameterIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the number of elements in the parameter 
        /// </summary>
        public int? NumberOfElements
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the attributes assigned to this marshal descriptor.
        /// </summary>
        public LPArrayFlags? Flags
        {
            get;
            set;
        }

        /// <inheritdoc />
        protected override void WriteContents(BlobSerializationContext context)
        {
            var writer = context.Writer;
            
            writer.WriteByte((byte) NativeType);
            writer.WriteByte((byte) ArrayElementType);

            if (ParameterIndex.HasValue)
            {
                writer.WriteCompressedUInt32((uint) ParameterIndex.Value);
                if (NumberOfElements.HasValue)
                {
                    writer.WriteCompressedUInt32((uint) NumberOfElements.Value);
                    if (Flags.HasValue)
                        writer.WriteCompressedUInt32((uint) Flags.Value);
                }
            }
        }
    }
}