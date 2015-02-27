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
            if (reader.ReadByte() != 0x06)
                throw new ArgumentException("Signature doesn't refer to a valid field signature.");
            return new FieldSignature
            {
                FieldType = TypeSignature.FromReader(header, reader),
            };
        }

        private FieldSignature()
        {
            
        }

        public FieldSignature(TypeSignature fieldType)
        {
            FieldType = fieldType;
        }

        public TypeSignature FieldType
        {
            get;
            set;
        }

        public override bool IsMethod
        {
            get { return false; }
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
