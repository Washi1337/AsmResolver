using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Represents the type of an object referencing a function pointer.
    /// </summary>
    public class FunctionPointerTypeSignature : TypeSignature
    {
        /// <summary>
        /// Reads a single function pointer type at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the function pointer was defined in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The read function pointer</returns>
        public static FunctionPointerTypeSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return FromReader(image, reader, new RecursionProtection());
        }
        
        /// <summary>
        /// Reads a single function pointer type at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the function pointer was defined in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="protection">The recursion protection that is used to detect malicious loops in the metadata.</param>
        /// <returns>The read function pointer</returns>
        public static FunctionPointerTypeSignature FromReader(
            MetadataImage image, 
            IBinaryStreamReader reader, 
            RecursionProtection protection)
        {
            return new FunctionPointerTypeSignature(MethodSignature.FromReader(image, reader, false, protection));
        }

        public FunctionPointerTypeSignature(MethodSignature signature)
        {
            Signature = signature;
        }

        /// <inheritdoc />
        public override ElementType ElementType => ElementType.FnPtr;

        /// <summary>
        /// Gets or sets the signature of the method referenced.
        /// </summary>
        public MethodSignature Signature
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override string Name => "*";

        /// <inheritdoc />
        public override string Namespace => string.Empty;

        /// <inheritdoc />
        public override string FullName => Signature.ToString();

        /// <inheritdoc />
        public override IResolutionScope ResolutionScope => null;

        /// <inheritdoc />
        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return sizeof(byte)
                   + Signature.GetPhysicalLength(buffer)
                   + base.GetPhysicalLength(buffer);
        }

        /// <inheritdoc />
        public override void Prepare(MetadataBuffer buffer)
        {
            Signature.Prepare(buffer);
        }

        /// <inheritdoc />
        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte) ElementType.FnPtr);
            Signature.Write(buffer, writer);

            base.Write(buffer, writer);
        }
    }
}
