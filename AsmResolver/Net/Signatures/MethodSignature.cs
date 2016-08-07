using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class MethodSignature : MemberSignature
    {
        public new static MethodSignature FromReader(MetadataHeader header, IBinaryStreamReader reader)
        {
            if (!reader.CanRead(sizeof (byte)))
                return null;

            var signature = new MethodSignature
            {
                Attributes = (CallingConventionAttributes)reader.ReadByte()
            };

            if (signature.IsGeneric)
            {
                uint genericParameterCount;
                if (!reader.TryReadCompressedUInt32(out genericParameterCount))
                    return signature;
                signature.GenericParameterCount = (int)genericParameterCount;
            }

            uint parameterCount;
            if (!reader.TryReadCompressedUInt32(out parameterCount))
                return signature;

            signature.ReturnType = TypeSignature.FromReader(header, reader);
            for (int i = 0; i < parameterCount; i++)
            {
                signature.Parameters.Add(ParameterSignature.FromReader(header, reader));
            }

            return signature;
        }

        public MethodSignature()
        {
            Parameters = new List<ParameterSignature>();
        }

        public MethodSignature(TypeSignature returnType)
            : this()
        {
            ReturnType = returnType;
        }
        
        public int GenericParameterCount
        {
            get;
            set;
        }

        public IList<ParameterSignature> Parameters
        {
            get;
            private set;
        }

        public TypeSignature ReturnType
        {
            get;
            set;
        }

        protected override TypeSignature TypeSignature
        {
            get { return ReturnType; }
        }

        public override uint GetPhysicalLength()
        {
            return (uint)(sizeof (byte) +
                          (IsGeneric ? GenericParameterCount.GetCompressedSize() : 0) +
                          Parameters.Count.GetCompressedSize() +
                          ReturnType.GetPhysicalLength() +
                          Parameters.Sum(x => x.GetPhysicalLength()));
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteByte((byte)Attributes);

            if (IsGeneric)
                writer.WriteCompressedUInt32((uint)GenericParameterCount);

            writer.WriteCompressedUInt32((uint)Parameters.Count);
            ReturnType.Write(context);
            foreach (var parameter in Parameters)
                parameter.Write(context);
        }
    }

  
}
