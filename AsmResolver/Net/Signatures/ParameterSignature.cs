using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class ParameterSignature : BlobSignature
    {
        public static ParameterSignature FromReader(MetadataHeader header, IBinaryStreamReader reader)
        {
            return new ParameterSignature
            {
                StartOffset = reader.Position,
                ParameterType = TypeSignature.FromReader(header, reader),
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
