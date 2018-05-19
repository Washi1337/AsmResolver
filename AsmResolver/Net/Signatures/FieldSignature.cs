using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    public class FieldSignature : MemberSignature
    {
        public new static FieldSignature FromReader(MetadataImage image, IBinaryStreamReader reader, bool readToEnd = false)
        {
            return new FieldSignature
            {
                Attributes = (CallingConventionAttributes)reader.ReadByte(),
                FieldType = TypeSignature.FromReader(image, reader),
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

        public TypeSignature FieldType
        {
            get;
            set;
        }

        protected override TypeSignature TypeSignature
        {
            get { return FieldType; }
        }

        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return sizeof(byte) +
                   FieldType.GetPhysicalLength(buffer) +
                   base.GetPhysicalLength(buffer);
        }

        public override void Prepare(MetadataBuffer buffer)
        {
            FieldType.Prepare(buffer);
        }

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte(0x06);
            FieldType.Write(buffer, writer);

            base.Write(buffer, writer);
        }
    }
}
