using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    public class FieldSignature : MemberSignature
    {
        public new static FieldSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return new FieldSignature
            {
                StartOffset = reader.Position,
                Attributes = (CallingConventionAttributes)reader.ReadByte(),
                FieldType = TypeSignature.FromReader(image, reader),
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

        public TypeSignature FieldType
        {
            get;
            set;
        }

        protected override TypeSignature TypeSignature
        {
            get { return FieldType; }
        }

        public override uint GetPhysicalLength()
        {
            return sizeof (byte) +
                   FieldType.GetPhysicalLength();
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteByte(0x06);
            FieldType.Write(context);
        }
    }
}
