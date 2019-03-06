using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Represents a type signature that is annotated with an optional modifier. 
    /// </summary>
    public class OptionalModifierSignature : TypeSpecificationSignature
    {
        /// <summary>
        /// Reads a single annotated type signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the signature resides in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The read signature.</returns>
        public static OptionalModifierSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return FromReader(image, reader, new RecursionProtection());
        }        
        
        /// <summary>
        /// Reads a single annotated type signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the signature resides in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="protection">The recursion protection that is used to detect malicious loops in the metadata.</param>
        /// <returns>The read signature.</returns>
        public static OptionalModifierSignature FromReader(
            MetadataImage image, 
            IBinaryStreamReader reader,
            RecursionProtection protection)
        {
            return new OptionalModifierSignature(
                ReadTypeDefOrRef(image, reader, protection),
                TypeSignature.FromReader(image, reader, false, protection));
        }

        public OptionalModifierSignature(ITypeDefOrRef modifierType, TypeSignature baseType)
            : base(baseType)
        {
            ModifierType = modifierType;
        }

        /// <inheritdoc />
        public override ElementType ElementType => ElementType.CModReqD;

        /// <summary>
        /// Gets or sets the type that is used to annotate the signature. 
        /// </summary>
        public ITypeDefOrRef ModifierType
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override string Name => BaseType.Name + $" modopt({ModifierType.FullName})";

        /// <inheritdoc />
        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            var encoder = ModifierType.Image.Header.GetStream<TableStream>()
                .GetIndexEncoder(CodedIndex.TypeDefOrRef);
            return sizeof(byte) +
                   encoder.EncodeToken(buffer.TableStreamBuffer.GetTypeToken(ModifierType)).GetCompressedSize() +
                   BaseType.GetPhysicalLength(buffer) +
                   base.GetPhysicalLength(buffer);
        }

        /// <inheritdoc />
        public override void Prepare(MetadataBuffer buffer)
        {
            base.Prepare(buffer);
            buffer.TableStreamBuffer.GetTypeToken(ModifierType);
        }

        /// <inheritdoc />
        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte)ElementType);
            WriteTypeDefOrRef(buffer, writer, ModifierType);
            BaseType.Write(buffer, writer);

            base.Write(buffer, writer);
        }
    }
}
