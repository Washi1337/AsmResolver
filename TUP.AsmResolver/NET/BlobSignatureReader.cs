using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TUP.AsmResolver.NET.Specialized;
namespace TUP.AsmResolver.NET
{
    public class BlobSignatureReader : BinaryReader
    {
        NETHeader _netHeader;

        public BlobSignatureReader(Stream input, NETHeader netHeader)
            : base(input)
        {
            _netHeader = netHeader;
        }

        public IGenericContext GenericContext { get; set; }

        public TypeReference ReadTypeReference()
        {
            return ReadTypeReference((ElementType)this.ReadByte());
        }

        public TypeReference ReadTypeReference(ElementType type)
        {
            switch (type)
            {
                case ElementType.Void:
                    return _netHeader.TypeSystem.Void;
                case ElementType.I:
                    return _netHeader.TypeSystem.IntPtr;
                case ElementType.I1:
                    return _netHeader.TypeSystem.Int8;
                case ElementType.I2:
                    return _netHeader.TypeSystem.Int16;
                case ElementType.I4:
                    return _netHeader.TypeSystem.Int32;
                case ElementType.I8:
                    return _netHeader.TypeSystem.Int64;
                case ElementType.U:
                    return _netHeader.TypeSystem.UIntPtr;
                case ElementType.U1:
                    return _netHeader.TypeSystem.UInt8;
                case ElementType.U2:
                    return _netHeader.TypeSystem.UInt16;
                case ElementType.U4:
                    return _netHeader.TypeSystem.UInt32;
                case ElementType.U8:
                    return _netHeader.TypeSystem.UInt64;
                case ElementType.Object:
                    return _netHeader.TypeSystem.Object;
                case ElementType.R4:
                    return _netHeader.TypeSystem.Single;
                case ElementType.R8:
                    return _netHeader.TypeSystem.Double;
                case ElementType.String:
                    return _netHeader.TypeSystem.String;
                case ElementType.Char:
                    return _netHeader.TypeSystem.Char;
                case ElementType.Type:
                    return _netHeader.TypeSystem.Type;
                case ElementType.Boolean:
                    return _netHeader.TypeSystem.Boolean;
                case ElementType.Ptr:
                    return new PointerType(ReadTypeReference((ElementType)this.ReadByte()));
                case ElementType.MVar:
                    return GetGenericParameter(GenericParamType.Method, NETGlobals.ReadCompressedInt32(this));
                case ElementType.Var:
                    return GetGenericParameter(GenericParamType.Type, NETGlobals.ReadCompressedInt32(this));
                case ElementType.Array:
                    return ReadArrayType();
                case ElementType.SzArray:
                    return new ArrayType(ReadTypeReference((ElementType)this.ReadByte()));
                case ElementType.Class:
                    TypeReference typeRef;
                    if (_netHeader.TablesHeap.TypeDefOrRef.TryGetMember((int)NETGlobals.ReadCompressedUInt32(this), out typeRef))
                    {
                        return typeRef;
                    }
                    break;
                case ElementType.ValueType:
                    if (_netHeader.TablesHeap.TypeDefOrRef.TryGetMember((int)NETGlobals.ReadCompressedUInt32(this), out typeRef))
                    {
                        typeRef.IsValueType = true;
                        return typeRef;
                    }
                    break;
                case ElementType.ByRef:
                    return new ByReferenceType(ReadTypeReference( (ElementType)this.ReadByte()));
                case ElementType.Pinned:
                    return new PinnedType(ReadTypeReference((ElementType)this.ReadByte()));
                case ElementType.GenericInst:
                    bool isValueType = this.ReadByte() == 0x11;
                    TypeReference reference2 = ReadTypeToken();
                    GenericInstanceType instance = new GenericInstanceType(reference2);
                    instance._genericArguments = ReadGenericArguments();
                    instance.IsValueType = isValueType;

                    return instance;
            }
            return new TypeReference(string.Empty, type.ToString(), null) { _netheader = this._netHeader };

        }

        private TypeReference GetGenericParameter(GenericParamType type, int index)
        {
            if (GenericContext != null)
            {
                IGenericParamProvider paramProvider = null;
                if (type == GenericParamType.Method)
                    paramProvider = GenericContext.Method;
                else
                    paramProvider = GenericContext.Type;
                AddMissingGenericParameters(paramProvider, index);
                return paramProvider.GenericParameters[index];
            }

            return new GenericParameter(string.Format("{0}{1}", type == GenericParamType.Method ? "!!" : "!", index), (ushort)index, GenericParameterAttributes.NonVariant, null);
        }

        private void AddMissingGenericParameters(IGenericParamProvider provider, int index)
        { 
            for (int i = provider.GenericParameters.Length; i <= index; i++)
                provider.AddGenericParameter(new GenericParameter(provider, i));
        }

        public TypeReference[] ReadGenericArguments()
        {
            uint number = NETGlobals.ReadCompressedUInt32(this);

            var genericArguments = new TypeReference[number];

            for (int i = 0; i < number; i++)
               genericArguments[i] = ReadTypeReference((ElementType)this.ReadByte());

            return genericArguments;
        }

        public TypeReference ReadTypeToken()
        {
            TypeReference typeRef;

            if (_netHeader.TablesHeap.TypeDefOrRef.TryGetMember((int)NETGlobals.ReadCompressedUInt32(this), out typeRef))
            {
                if (typeRef is ISpecification)
                    typeRef = (typeRef as TypeSpecification).TransformWith(GenericContext) as TypeReference;
            }

            return typeRef;
        }

        public ArrayType ReadArrayType()
        {
            TypeReference arrayType = ReadTypeReference((ElementType)this.ReadByte());
            uint rank = NETGlobals.ReadCompressedUInt32(this);
            uint[] upperbounds = new uint[NETGlobals.ReadCompressedUInt32(this)];

            for (int i = 0; i < upperbounds.Length; i++)
                upperbounds[i] = NETGlobals.ReadCompressedUInt32(this);

            int[] lowerbounds = new int[NETGlobals.ReadCompressedUInt32(this)];

            for (int i = 0; i < lowerbounds.Length; i++)
                lowerbounds[i] = NETGlobals.ReadCompressedInt32(this);


            ArrayDimension[] dimensions = new ArrayDimension[rank];

            for (int i = 0; i < rank; i++)
            {
                int? lower = null;
                int? upper = null;

                if (i < lowerbounds.Length)
                    lower = new int?(lowerbounds[i]);

                if (i < upperbounds.Length)
                {
                    int x = (int)upperbounds[i];
                    upper = (lower.HasValue ? new int?(lower.GetValueOrDefault() + x) : 0) - 1;
                }
                ArrayDimension dimension = new ArrayDimension(lower, upper);
                dimensions[i] = dimension;

            }



            return new ArrayType(arrayType, (int)rank, dimensions);

        }
        
        public object ReadArgumentValue(TypeReference paramType)
        {
            if (!paramType.IsArray || !(paramType as ArrayType).IsVector)
                return ReadElement(paramType);

            // throw new NotImplementedException("Array constructor values are not supported yet.");

            ushort elementcount = this.ReadUInt16();
            object[] elements = new object[elementcount];
            for (int i = 0; i < elementcount; i++)
                elements[i] = ReadElement((paramType as ArrayType).OriginalType);

            return elements;
        }

        public object ReadElement(TypeReference paramType)
        {
            if (paramType.FullName == "System.Type")
                return ReadUtf8String();

            switch (paramType._elementType)
            {
                case ElementType.I1:
                    return this.ReadSByte();
                case ElementType.I2:
                    return this.ReadInt16();
                case ElementType.I4:
                    return this.ReadInt32();
                case ElementType.I8:
                    return this.ReadInt64();
                case ElementType.U1:
                    return this.ReadByte();
                case ElementType.U2:
                    return this.ReadInt16();
                case ElementType.U4:
                    return this.ReadInt32();
                case ElementType.U8:
                    return this.ReadInt64();
                case ElementType.R4:
                    return this.ReadSingle();
                case ElementType.R8:
                    return this.ReadDouble();
                case ElementType.Type:
                    return ReadUtf8String();
                case ElementType.String:
                    return ReadUtf8String();
                case ElementType.Char:
                    return this.ReadChar();
                    throw new NotSupportedException();
                case ElementType.Boolean:
                    return this.ReadByte() == 1;


            }
            return null;
        }

        public string ReadUtf8String()
        {
            uint size = NETGlobals.ReadCompressedUInt32(this);
            if (size == 0xFF)
                return string.Empty;
            byte[] rawdata = this.ReadBytes((int)size);
            return Encoding.UTF8.GetString(rawdata);
        }
    }
}
