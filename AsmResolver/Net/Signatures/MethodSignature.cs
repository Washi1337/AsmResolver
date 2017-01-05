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
                StartOffset = reader.Position,
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
            : this(Enumerable.Empty<ParameterSignature>(), null)
        {
        }

        public MethodSignature(TypeSignature returnType)
            : this(Enumerable.Empty<ParameterSignature>(), returnType)
        {
            ReturnType = returnType;
        }

        public MethodSignature(IEnumerable<TypeSignature> parameters, TypeSignature returnType)
            : this(parameters.Select(x => new ParameterSignature(x)), returnType)
        {
        }

        public MethodSignature(IEnumerable<ParameterSignature> parameters, TypeSignature returnType)
        {
            Parameters = new List<ParameterSignature>(parameters);
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

        public override string ToString()
        {
            return (HasThis ? "instance " : "") 
                + ReturnType.FullName 
                + " *(" + Parameters.Select(x => x.ParameterType).GetTypeArrayString() + ")";
        }
    }

  
}
