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

        public BlobSignatureReader(byte[] bytes, NETHeader netHeader)
            : this(new MemoryStream(bytes), netHeader)
        {
        }
        
        public IGenericContext GenericContext { get; set; }

        public bool EndOfStream
        {
            get
            {
                return BaseStream.Position == BaseStream.Length;
            }
        }

        public ElementType ReadElementType()
        {
            return (ElementType)ReadByte();
        }

        public TypeReference ReadTypeReference()
        {
            return ReadTypeReference(ReadElementType());
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
                    return GetGenericParameter(GenericParamType.Method, (int)NETGlobals.ReadCompressedUInt32(this));
                case ElementType.Var:
                    return GetGenericParameter(GenericParamType.Type, (int)NETGlobals.ReadCompressedUInt32(this));
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

                case ElementType.CModOpt:
                    return new CustomModifierType(ReadTypeToken(), ReadTypeReference(), false);
                case ElementType.CModReqD:
                    return new CustomModifierType(ReadTypeToken(), ReadTypeReference(), true);
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
            {
                provider.AddGenericParameter(new GenericParameter(provider, i));
            } 
        }

        public TypeReference[] ReadGenericArguments()
        {
            uint number = NETGlobals.ReadCompressedUInt32(this);

            var genericArguments = new TypeReference[number];

            for (int i = 0; i < number; i++)
                genericArguments[i] = ReadTypeReference();

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
        
        public object ReadCustomAttributeArgumentValue(TypeReference paramType)
        {
            if (!paramType.IsArray || !(paramType as ArrayType).IsVector)
                return ReadCustomAttributeArgumentElement(paramType);

            // throw new NotImplementedException("Array constructor values are not supported yet.");

            ushort elementcount = this.ReadUInt16();
            object[] elements = new object[elementcount];
            for (int i = 0; i < elementcount; i++)
                elements[i] = ReadCustomAttributeArgumentElement((paramType as ArrayType).OriginalType);

            return elements;
        }

        public object ReadCustomAttributeArgumentElement(TypeReference paramType)
        {
            if (paramType._elementType == ElementType.None)
            {
                // TODO: convert string to type ref:
                if (paramType.FullName == "System.Type")
                    return ReadUtf8String();

                var resolvedTypeDef = paramType.Resolve();
                if (resolvedTypeDef != null)
                {
                    var enumType = resolvedTypeDef.GetEnumType();
                    if (enumType != null)
                        return ReadCustomAttributeArgumentElement(enumType);
                }
                return null;
            }
            else
            {
                if (paramType._elementType == ElementType.String)
                    return ReadUtf8String();

                return ReadPrimitiveValue(paramType._elementType);
            }
        }

        public string ReadUtf8String()
        {
            if (ReadByte() == 0xFF)
                return string.Empty;
            BaseStream.Seek(-1, SeekOrigin.Current);
            uint size = NETGlobals.ReadCompressedUInt32(this);
            byte[] rawdata = this.ReadBytes((int)size);
            return Encoding.UTF8.GetString(rawdata);
        }

        public object ReadPrimitiveValue(ElementType type)
        {
            switch (type)
            {
                case ElementType.Boolean:
                    return ReadByte() == 1;
                case ElementType.Char:
                    return (char)ReadUInt16();
                case ElementType.I1:
                    return ReadSByte();
                case ElementType.I2:
                    return ReadInt16();
                case ElementType.I4:
                    return ReadInt32();
                case ElementType.I8:
                    return ReadInt64();
                case ElementType.U1:
                    return ReadByte();
                case ElementType.U2:
                    return ReadUInt16();
                case ElementType.U4:
                    return ReadUInt32();
                case ElementType.U8:
                    return ReadUInt64();
                case ElementType.R4:
                    return ReadSingle();
                case ElementType.R8:
                    return ReadDouble();
            }

            return null;
        }

        public TypeReference ReadCustomAttributeFieldOrPropType()
        {
            ElementType element = ReadElementType();
            switch (element)
            {
                case ElementType.Type:
                    return _netHeader.TypeSystem.Type;
                case ElementType.Boxed:
                    return _netHeader.TypeSystem.Object;
                case ElementType.Enum:
                    string typeName = ReadUtf8String();
                    //TODO: parse to type ref
                    return null;
                case ElementType.SzArray:
                    return new ArrayType(ReadCustomAttributeFieldOrPropType());
            }
            return ReadTypeReference(element);
        }
    }
}
