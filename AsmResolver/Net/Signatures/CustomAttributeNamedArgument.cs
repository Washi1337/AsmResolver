using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    /// <summary>
    /// Represents a single named argument of a custom attribute associated to a member.
    /// </summary>
    public class CustomAttributeNamedArgument : BlobSignature
    {
        /// <summary>
        /// Reads a single custom attribute argument at the current position of the provided stream reader.
        /// </summary>
        /// <param name="image">The image the argument was defined in.</param>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The read argument.</returns>
        public static CustomAttributeNamedArgument FromReader(
            MetadataImage image,
            IBinaryStreamReader reader)
        {
            var signature = new CustomAttributeNamedArgument
            {
                ArgumentMemberType =
                    (reader.CanRead(sizeof (byte))
                        ? (CustomAttributeArgumentMemberType)reader.ReadByte()
                        : CustomAttributeArgumentMemberType.Field),
                ArgumentType = TypeSignature.ReadFieldOrPropType(image, reader),
                MemberName = reader.ReadSerString(),
            };
            signature.Argument = CustomAttributeArgument.FromReader(image, signature.ArgumentType, reader);
            return signature;
        }

        public CustomAttributeNamedArgument()
        {
        }

        public CustomAttributeNamedArgument(CustomAttributeArgumentMemberType argumentMemberType,
            TypeSignature argumentType, string memberName, CustomAttributeArgument argument)
        {
            ArgumentMemberType = argumentMemberType;
            ArgumentType = argumentType;
            MemberName = memberName;
            Argument = argument;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the named argument is assigned to a field or a property of the
        /// underlying attribute class.
        /// </summary>
        public CustomAttributeArgumentMemberType ArgumentMemberType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of the argument.
        /// </summary>
        public TypeSignature ArgumentType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the field or property referenced in the argument.
        /// </summary>
        public string MemberName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value assigned to the field or property.
        /// </summary>
        public CustomAttributeArgument Argument
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return sizeof (byte) +
                   ArgumentType.GetPhysicalLength(buffer) +
                   (MemberName?.GetSerStringSize() ?? sizeof (byte)) +
                   Argument.GetPhysicalLength(buffer);
        }

        /// <inheritdoc />
        public override void Prepare(MetadataBuffer buffer)
        {
            ArgumentType.Prepare(buffer);
            Argument.Prepare(buffer);
        }

        /// <inheritdoc />
        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte)ArgumentMemberType);
            ArgumentType.Write(buffer, writer); // TODO: write FieldOrPropType instead.
            writer.WriteSerString(MemberName);
            Argument.Write(buffer, writer);
        }
    }
}
