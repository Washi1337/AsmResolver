using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    public class VariableSignature : BlobSignature
    {
        public static VariableSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return new VariableSignature(TypeSignature.FromReader(image, reader));
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

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            VariableType.Write(buffer, writer);
        }
    }
}
