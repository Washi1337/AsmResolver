using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    public class ParameterSignature : BlobSignature
    {
        public static ParameterSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return new ParameterSignature(TypeSignature.FromReader(image, reader));
        }

        public ParameterSignature(TypeSignature parameterType)
        {
            ParameterType = parameterType;
        }

        public TypeSignature ParameterType
        {
            get;
            set;
        }

        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return ParameterType.GetPhysicalLength(buffer);
        }

        public override void Prepare(MetadataBuffer buffer)
        {
            ParameterType.Prepare(buffer);
        }

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            ParameterType.Write(buffer, writer);
        }
    }
}
