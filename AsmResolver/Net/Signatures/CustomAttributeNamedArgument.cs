using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    public class CustomAttributeNamedArgument : BlobSignature
    {
        public static CustomAttributeNamedArgument FromReader(MetadataImage image, IBinaryStreamReader reader)
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

        public CustomAttributeNamedArgument(CustomAttributeArgumentMemberType argumentMemberType, TypeSignature argumentType, string memberName, CustomAttributeArgument argument)
        {
            ArgumentMemberType = argumentMemberType;
            ArgumentType = argumentType;
            MemberName = memberName;
            Argument = argument;
        }

        public CustomAttributeArgumentMemberType ArgumentMemberType
        {
            get;
            set;
        }

        public TypeSignature ArgumentType
        {
            get;
            set;
        }

        public string MemberName
        {
            get;
            set;
        }

        public CustomAttributeArgument Argument
        {
            get;
            set;
        }

        public override uint GetPhysicalLength()
        {
            return sizeof (byte) +
                   ArgumentType.GetPhysicalLength() +
                   (MemberName == null ? sizeof (byte) : MemberName.GetSerStringSize()) +
                   Argument.GetPhysicalLength();
        }

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte)ArgumentMemberType);
            ArgumentType.Write(buffer, writer); // TODO: write FieldOrPropType instead.
            writer.WriteSerString(MemberName);
            Argument.Write(buffer, writer);
        }
    }

    public enum CustomAttributeArgumentMemberType
    {
        Field = 0x53,
        Property = 0x54,
    }
}
