using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public abstract class TypeSignature : BlobSignature, ITypeDescriptor
    {
        public static TypeSignature FromReader(MetadataHeader header, IBinaryStreamReader reader)
        {
            var elementType = (ElementType)reader.ReadByte();
            switch (elementType)
            {
                case ElementType.Array:
                    return ArrayTypeSignature.FromReader(header, reader);
                case ElementType.Boxed:
                    return BoxedTypeSignature.FromReader(header, reader);
                case ElementType.ByRef:
                    return ByReferenceTypeSignature.FromReader(header, reader);
                case ElementType.CModOpt:
                    return OptionalModifierSignature.FromReader(header, reader);
                case ElementType.CModReqD:
                    return RequiredModifierSignature.FromReader(header, reader);
                case ElementType.Class:
                    return TypeDefOrRefSignature.FromReader(header, reader);
                case ElementType.FnPtr:
                    break;
                case ElementType.GenericInst:
                    return GenericInstanceTypeSignature.FromReader(header, reader);
               case ElementType.MVar:
                     return GenericParameterSignature.FromReader(header, reader, GenericParameterType.Method);
                case ElementType.Pinned:
                    return PinnedTypeSignature.FromReader(header, reader);
                case ElementType.Ptr:
                    return PointerTypeSignature.FromReader(header, reader);
                case ElementType.Sentinel:
                    return SentinelTypeSignature.FromReader(header, reader);
                case ElementType.SzArray:
                    return SzArrayTypeSignature.FromReader(header, reader);
                case ElementType.ValueType:
                    var type = TypeDefOrRefSignature.FromReader(header, reader);
                    type.IsValueType = true;
                    return type;
                case ElementType.Var:
                    return GenericParameterSignature.FromReader(header, reader, GenericParameterType.Type);
                default:
                    return MsCorLibTypeSignature.FromElementType(header, elementType);
            }
            throw new NotSupportedException();
        }

        public static TypeSignature FromAssemblyQualifiedName(MetadataHeader header, string assemblyQualifiedName)
        {
            return TypeNameParser.ParseType(header, assemblyQualifiedName);
        }

        public static TypeSignature ReadFieldOrPropType(MetadataHeader header, IBinaryStreamReader reader)
        {
            var elementType = (ElementType)reader.ReadByte();
            switch (elementType)
            {
                case ElementType.Boxed:
                    return header.TypeSystem.Object;
                case ElementType.SzArray:
                    return new SzArrayTypeSignature(ReadFieldOrPropType(header, reader));
                case ElementType.Enum:
                    return FromAssemblyQualifiedName(header, reader.ReadSerString());
                default:
                    return MsCorLibTypeSignature.FromElementType(header, elementType);
            }
        }

        protected static ITypeDefOrRef ReadTypeDefOrRef(MetadataHeader header, IBinaryStreamReader reader)
        {
            var tableStream = header.GetStream<TableStream>();

            uint codedIndex;
            if (!reader.TryReadCompressedUInt32(out codedIndex))
                return null;

            MetadataMember type;
            tableStream.TryResolveMember(tableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef)
                .DecodeIndex(codedIndex), out type);
            
            return type as ITypeDefOrRef;
        }

        protected static void WriteTypeDefOrRef(MetadataHeader header, IBinaryStreamWriter writer, ITypeDefOrRef type)
        {
            var encoder =
                header.GetStream<TableStream>()
                    .GetIndexEncoder(CodedIndex.TypeDefOrRef);
            writer.WriteCompressedUInt32(encoder.EncodeToken(type.MetadataToken));
        }

        public abstract ElementType ElementType
        {
            get;
        }

        public abstract string Name
        {
            get;
        }

        public abstract string Namespace
        {
            get;
        }

        public ITypeDescriptor DeclaringTypeDescriptor
        {
            get { return ResolutionScope as ITypeDescriptor; }
        }

        public abstract IResolutionScope ResolutionScope
        {
            get;
        }

        public virtual bool IsValueType
        {
            get;
            set;
        }

        public virtual string FullName
        {
            get { return string.IsNullOrEmpty(Namespace) ? Name : Namespace + "." + Name; }
        }

        public virtual ITypeDescriptor GetElementType()
        {
            return this;
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
