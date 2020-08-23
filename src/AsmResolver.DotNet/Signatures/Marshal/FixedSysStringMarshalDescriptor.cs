namespace AsmResolver.DotNet.Signatures.Marshal
{
    /// <summary>
    /// Represents a description of a marshaller that marshals a given value to a fixed-length string using the system
    /// defined string encoding. 
    /// </summary>
    public class FixedSysStringMarshalDescriptor : MarshalDescriptor
    {
        /// <summary>
        /// Reads a single fixed system string marshal descriptor from the provided input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <returns>The descriptor.</returns>
        public static FixedSysStringMarshalDescriptor FromReader(IBinaryStreamReader reader)
        {
            reader.TryReadCompressedUInt32(out uint size);
            return new FixedSysStringMarshalDescriptor((int) size);
        }
        
        /// <summary>
        /// Creates a new instance of the <see cref="FixedSysStringMarshalDescriptor"/> class.
        /// </summary>
        /// <param name="size">The number of characters in the string.</param>
        public FixedSysStringMarshalDescriptor(int size)
        {
            Size = size;
        }
        
        /// <inheritdoc />
        public override NativeType NativeType => NativeType.FixedSysString;

        /// <summary>
        /// Gets or sets the number of characters in the string.
        /// </summary>
        public int Size
        {
            get;
            set;
        }

        /// <inheritdoc />
        protected override void WriteContents(BlobSerializationContext context)
        {
            var writer = context.Writer;
            
            writer.WriteByte((byte) NativeType);
            writer.WriteCompressedUInt32((uint) Size);
        }
    }
}