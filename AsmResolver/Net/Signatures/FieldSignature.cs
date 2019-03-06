using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Represents a signature of a field, containing the field type.
    /// </summary>
    public class FieldSignature : MemberSignature
    {
        /// <summary>
        /// Reads a single field signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the field is defined in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="readToEnd">Determines whether any extra data after the signature should be read and
        /// put into the <see cref="ExtendableBlobSignature.ExtraData"/> property.</param>
        /// <returns>The read signature.</returns>
        public new static FieldSignature FromReader(MetadataImage image, IBinaryStreamReader reader,
            bool readToEnd = false)
        {
            return FromReader(image, reader, readToEnd, new RecursionProtection());
        }

        /// <summary>
        /// Reads a single field signature at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the field is defined in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <param name="readToEnd">Determines whether any extra data after the signature should be read and
        /// put into the <see cref="ExtendableBlobSignature.ExtraData"/> property.</param>
        /// <param name="protection">The recursion protection that is used to detect malicious loops in the metadata.</param>
        /// <returns>The read signature.</returns>
        public new static FieldSignature FromReader(
            MetadataImage image,
            IBinaryStreamReader reader,
            bool readToEnd, 
            RecursionProtection protection)
        {
            return new FieldSignature
            {
                Attributes = (CallingConventionAttributes)reader.ReadByte(),
                FieldType = TypeSignature.FromReader(image, reader, false, protection),
                ExtraData = readToEnd ? reader.ReadToEnd() : null
            };
        }

        private FieldSignature()
        {
            
        }

        public FieldSignature(TypeSignature fieldType)
        {
            Attributes = CallingConventionAttributes.Field;
            FieldType = fieldType;
        }

        /// <summary>
        /// Gets or sets the type of the field that uses the signature.
        /// </summary>
        public TypeSignature FieldType
        {
            get;
            set;
        }

        protected override TypeSignature TypeSignature => FieldType;

        /// <inheritdoc />
        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return sizeof(byte) +
                   FieldType.GetPhysicalLength(buffer) +
                   base.GetPhysicalLength(buffer);
        }

        /// <inheritdoc />
        public override void Prepare(MetadataBuffer buffer)
        {
            FieldType.Prepare(buffer);
        }

        /// <inheritdoc />
        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte(0x06);
            FieldType.Write(buffer, writer);

            base.Write(buffer, writer);
        }
    }
}
