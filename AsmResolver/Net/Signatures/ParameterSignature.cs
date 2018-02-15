using AsmResolver.Net.Cts;

namespace AsmResolver.Net.Signatures
{
    public class ParameterSignature : BlobSignature
    {
        public static ParameterSignature FromReader(MetadataImage image, IBinaryStreamReader reader)
        {
            return new ParameterSignature
            {
                StartOffset = reader.Position,
                ParameterType = TypeSignature.FromReader(image, reader),
            };
        }

        private ParameterSignature()
        {
            
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

        public override uint GetPhysicalLength()
        {
            return ParameterType.GetPhysicalLength();
        }

        public override void Write(WritingContext context)
        {
            ParameterType.Write(context);
        }
    }
}
