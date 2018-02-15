using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    public class VariableSignature : BlobSignature
    {
        public static VariableSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            long position = reader.Position;
            return new VariableSignature(TypeSignature.FromReader(image, reader))
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
