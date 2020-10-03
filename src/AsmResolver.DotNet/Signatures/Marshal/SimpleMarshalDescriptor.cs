namespace AsmResolver.DotNet.Signatures.Marshal
{
    /// <summary>
    /// Represents a marshal descriptor that requires no further parameters.
    /// </summary>
    public class SimpleMarshalDescriptor : MarshalDescriptor
    {
        /// <summary>
        /// Creates a new instance of the <see cref="SimpleMarshalDescriptor"/> class.
        /// </summary>
        /// <param name="nativeType"></param>
        public SimpleMarshalDescriptor(NativeType nativeType)
        {
            NativeType = nativeType;
        }

        /// <inheritdoc />
        public override NativeType NativeType
        {
            get;
        }

        /// <inheritdoc />
        protected override void WriteContents(BlobSerializationContext context)
        {
            context.Writer.WriteByte((byte) NativeType);
        }
    }
}