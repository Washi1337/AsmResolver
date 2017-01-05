using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.Net.Signatures
{
    public class CustomAttributeNamedArgument : BlobSignature
    {
        public static CustomAttributeNamedArgument FromReader(MetadataHeader header, IBinaryStreamReader reader)
        {
            var signature = new CustomAttributeNamedArgument
            {
                StartOffset = reader.Position,
                ArgumentMemberType =
                    (reader.CanRead(sizeof (byte))
                        ? (CustomAttributeArgumentMemberType)reader.ReadByte()
                        : CustomAttributeArgumentMemberType.Field),
                ArgumentType = TypeSignature.ReadFieldOrPropType(header, reader),
                MemberName = reader.ReadSerString(),
            };
            signature.Argument = CustomAttributeArgument.FromReader(header, signature.ArgumentType, reader);
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

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteByte((byte)ArgumentMemberType);
            ArgumentType.Write(context); // TODO: write FieldOrPropType instead.
            writer.WriteSerString(MemberName);
            Argument.Write(context);
        }
    }

    public enum CustomAttributeArgumentMemberType
    {
        Field = 0x53,
        Property = 0x54,
    }
}
