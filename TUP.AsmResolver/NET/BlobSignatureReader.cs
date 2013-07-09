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

                    if (GenericContext == null)
                        return new GenericParamReference(NETGlobals.ReadCompressedInt32(this), new TypeReference(string.Empty, "MVar", null) { elementType = ElementType.MVar, @namespace = "", netheader = this._netHeader });

                    return ReadGenericType();

                case ElementType.Var:
                    uint token = NETGlobals.ReadCompressedUInt32(this);
                    if (GenericContext != null)
                    {
                        if (GenericContext.DeclaringType != null && GenericContext.DeclaringType.GenericParameters != null && GenericContext.DeclaringType.GenericParameters.Length > token)
                            return GenericContext.DeclaringType.GenericParameters[token];
                        else if (GenericContext.GenericParameters != null && GenericContext.GenericParameters.Length > token)
                            return GenericContext.GenericParameters[token];
                    }
                    return new GenericParamReference((int)token, new TypeReference(string.Empty, "Var", null) { elementType = ElementType.Var, @namespace = "", netheader = this._netHeader });

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
                    bool flag = this.ReadByte() == 0x11;
                    TypeReference reference2 = ReadTypeToken();
                    GenericInstanceType instance = new GenericInstanceType(reference2);
                    this.ReadGenericInstanceSignature(instance);
                    if (flag)
                    {
                        instance.IsValueType = true;

                    }
                    return instance;
            }
            return new TypeReference(string.Empty, type.ToString(), null) { netheader = this._netHeader };

        }

        public void ReadGenericInstanceSignature(GenericInstanceType genericType)
        {
            uint number = NETGlobals.ReadCompressedUInt32(this);

            genericType.genericArguments = new TypeReference[number];

            for (int i = 0; i < number; i++)
                genericType.genericArguments[i] = ReadTypeReference((ElementType)this.ReadByte());

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

        public TypeReference ReadGenericType()
        {
            // not finished yet!

            uint token = NETGlobals.ReadCompressedUInt32(this);
            object genericType;

            if (GenericContext.IsDefinition)
            {
                if (TryGetArrayValue(GenericContext.GenericParameters, token, out genericType))
                    return genericType as TypeReference;
            }

            if (TryGetArrayValue(GenericContext.GenericArguments, token, out genericType))
                return genericType as TypeReference;

            return new TypeReference(string.Empty, token.ToString(), null);

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

            switch (paramType.elementType)
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

        public bool TryGetArrayValue(Array array, uint index, out object value)
        {
            value = null;
            if (array == null || array.Length < index || index < 0)
                return false;
            value = array.GetValue(index);
            return true;
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
