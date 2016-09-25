using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class FieldSignature : MemberSignature
    {
        public new static FieldSignature FromReader(MetadataHeader header, IBinaryStreamReader reader)
        {
            return new FieldSignature
            {
                Attributes = (CallingConventionAttributes)reader.ReadByte(),
                FieldType = TypeSignature.FromReader(header, reader),
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
