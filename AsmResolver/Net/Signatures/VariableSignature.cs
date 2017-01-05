using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver.Net.Signatures
{
    public class VariableSignature : BlobSignature
    {
        public static VariableSignature FromReader(MetadataHeader header, IBinaryStreamReader reader)
        {
            long position = reader.Position;
            return new VariableSignature(TypeSignature.FromReader(header, reader))
            {
                StartOffset = position
            };
        }

        public VariableSignature(TypeSignature variableType)
        {
            VariableType = variableType;
        }

        public TypeSignature VariableType
        {
            get;
            set;
        }

        public override uint GetPhysicalLength()
        {
            return VariableType.GetPhysicalLength();
        }

        public override void Write(WritingContext context)
        {
            VariableType.Write(context);
        }
    }
}
