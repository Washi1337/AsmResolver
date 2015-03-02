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
                Attributes = (MethodSignatureAttributes)reader.ReadByte()
            };

            if (signature.Attributes.HasFlag(MethodSignatureAttributes.Generic))
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

        public override bool IsMethod
        {
            get { return true; }
        }

        public MethodSignatureAttributes Attributes
        {
            get;
            set;
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
                          (Attributes.HasFlag(MethodSignatureAttributes.Generic) ? sizeof (byte) : 0) +
                          Parameters.Count.GetCompressedSize() +
                          ReturnType.GetPhysicalLength() +
                          Parameters.Sum(x => x.GetPhysicalLength()));
        }

        public override void Write(WritingContext context)
        {
            var writer = context.Writer;
            writer.WriteByte((byte)Attributes);

            if (Attributes.HasFlag(MethodSignatureAttributes.Generic))
                writer.WriteCompressedUInt32((uint)GenericParameterCount);

            writer.WriteCompressedUInt32((uint)Parameters.Count);
            ReturnType.Write(context);
            foreach (var parameter in Parameters)
                parameter.Write(context);
        }
    }

    [Flags]
    public enum MethodSignatureAttributes
    {
        HasThis = 0x20,
        ExplicitThis = 0x40,
        
        Default = 0x0,
        C = 0x1,
        StdCall = 0x2,
        ThisCall = 0x3,
        FastCall = 0x4,
        VarArg = 0x5,

        Generic = 0x10,

        Sentinel = 0x41, // TODO: support sentinel types.
    }
}
