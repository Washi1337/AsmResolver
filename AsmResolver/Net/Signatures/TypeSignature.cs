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
                case ElementType.Boolean:
                    return header.TypeSystem.Boolean;
                case ElementType.Boxed:
                    return BoxedTypeSignature.FromReader(header, reader);
                case ElementType.ByRef:
                    return ByReferenceTypeSignature.FromReader(header, reader);
                case ElementType.CModOpt:
                    break;
                case ElementType.CModReqD:
                    break;
                case ElementType.Char:
                    return header.TypeSystem.Char;
                case ElementType.Class:
                    return TypeDefOrRefSignature.FromReader(header, reader);
                case ElementType.Enum:
                    break;
                case ElementType.FnPtr:
                    break;
                case ElementType.GenericInst:
                    return GenericInstanceTypeSignature.FromReader(header, reader);
                case ElementType.I:
                    return header.TypeSystem.IntPtr;
                case ElementType.I1:
                    return header.TypeSystem.SByte;
                case ElementType.I2:
                    return header.TypeSystem.Int16;
                case ElementType.I4:
                    return header.TypeSystem.Int32;
                case ElementType.I8:
                    return header.TypeSystem.Int64;
                case ElementType.Internal:
                    break;
                case ElementType.MVar:
                    return GenericParameterSignature.FromReader(header, reader, GenericParameterType.Method);
                case ElementType.Modifier:
                    break;
                case ElementType.None:
                    break;
                case ElementType.Object:
                    return header.TypeSystem.Object;
                case ElementType.Pinned:
                    break;
                case ElementType.Ptr:
                    return PointerTypeSignature.FromReader(header, reader);
                case ElementType.R4:
                    return header.TypeSystem.Single;
                case ElementType.R8:
                    return header.TypeSystem.Double;
                case ElementType.Sentinel:
                    break;
                case ElementType.String:
                    return header.TypeSystem.String;
                case ElementType.SzArray:
                    return SzArrayTypeSignature.FromReader(header, reader);
                case ElementType.Type:
                    return header.TypeSystem.Type;
                case ElementType.TypedByRef:
                    return header.TypeSystem.TypedReference;
                case ElementType.U:
                    return header.TypeSystem.UIntPtr;
                case ElementType.U1:
                    return header.TypeSystem.Byte;
                case ElementType.U2:
                    return header.TypeSystem.UInt16;
                case ElementType.U4:
                    return header.TypeSystem.UInt64;
                case ElementType.U8:
                    return header.TypeSystem.UInt64;
                case ElementType.ValueType:
                    var type = TypeDefOrRefSignature.FromReader(header, reader);
                    type.IsValueType = true;
                    return type;
                case ElementType.Var:
                    return GenericParameterSignature.FromReader(header, reader, GenericParameterType.Type);
                case ElementType.Void:
                    return header.TypeSystem.Void;
            }
            throw new NotSupportedException();
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
