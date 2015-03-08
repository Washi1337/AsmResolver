using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Signatures
{
    public sealed class MsCorLibTypeSignature : TypeSignature
    {
        public static MsCorLibTypeSignature FromElementType(MetadataHeader header, ElementType elementType)
        {
            switch (elementType)
            {
                case ElementType.Boolean:
                    return header.TypeSystem.Boolean;
                case ElementType.Char:
                    return header.TypeSystem.Char;
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
                case ElementType.Object:
                    return header.TypeSystem.Object;
                case ElementType.R4:
                    return header.TypeSystem.Single;
                case ElementType.R8:
                    return header.TypeSystem.Double;
                case ElementType.String:
                    return header.TypeSystem.String;
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
                case ElementType.Void:
                    return header.TypeSystem.Void;
            }
            throw new NotSupportedException();
        }

        private readonly ElementType _elementType;

        internal MsCorLibTypeSignature(ITypeDefOrRef type, ElementType elementType, bool isValueType)
        {
            Type = type;
            _elementType = elementType;
            IsValueType = isValueType;
        }

        public ITypeDefOrRef Type
        {
            get;
            private set;
        }

        public override ElementType ElementType
        {
            get { return _elementType; }
        }

        public override string Name
        {
            get { return Type.Name; }
        }

        public override string Namespace
        {
            get { return Type.Namespace; }
        }

        public override IResolutionScope ResolutionScope
        {
            get { return Type.ResolutionScope; }
        }

        public override uint GetPhysicalLength()
        {
            return sizeof (byte);
        }

        public override void Write(WritingContext context)
        {
            context.Writer.WriteByte((byte)ElementType);
        }
    }
}
