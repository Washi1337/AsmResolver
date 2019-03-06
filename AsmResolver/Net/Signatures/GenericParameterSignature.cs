using System;
using AsmResolver.Net.Emit;
using AsmResolver.Net.Cts;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public class GenericParameterSignature : TypeSignature
    {
        public static GenericParameterSignature FromReader(MetadataImage image, IBinaryStreamReader reader, GenericParameterType parameterType)
        {
            return reader.TryReadCompressedUInt32(out uint index) 
                ? new GenericParameterSignature(parameterType, (int) index)
                : null;
        }

        public GenericParameterSignature(GenericParameterType parameterType, int index)
        {
            ParameterType = parameterType;
            Index = index;
        }

        public override ElementType ElementType
        {
            get
            {
                switch (ParameterType)
                {
                    case GenericParameterType.Type:
                        return ElementType.Var;
                    case GenericParameterType.Method:
                        return ElementType.MVar;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        public GenericParameterType ParameterType
        {
            get;
            set;
        }

        public int Index
        {
            get;
            set;
        }

        public override string Name
        {
            get
            {
                switch (ParameterType)
                {
                    case GenericParameterType.Method:
                        return "!!" + Index.ToString();
                    case GenericParameterType.Type:
                        return "!" + Index.ToString();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public override string Namespace => string.Empty;

        public override IResolutionScope ResolutionScope => null;

        public override uint GetPhysicalLength(MetadataBuffer buffer)
        {
            return sizeof(byte) +
                   Index.GetCompressedSize() +
                   base.GetPhysicalLength(buffer);
        }

        public override void Prepare(MetadataBuffer buffer)
        {
        }

        public override void Write(MetadataBuffer buffer, IBinaryStreamWriter writer)
        {
            writer.WriteByte((byte)ElementType);
            writer.WriteCompressedUInt32((uint)Index);

            base.Write(buffer, writer);
        }
    }
}
