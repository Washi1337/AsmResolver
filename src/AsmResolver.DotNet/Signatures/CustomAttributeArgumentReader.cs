using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.DotNet.Signatures.Types.Parsing;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures
{
    internal sealed class CustomAttributeArgumentReader
    {
        private readonly ModuleDefinition _parentModule;
        private readonly IBinaryStreamReader _reader;

        public CustomAttributeArgumentReader(ModuleDefinition parentModule, IBinaryStreamReader reader)
        {
            _parentModule = parentModule;
            _reader = reader;
        }

        public IList<object> Elements
        {
            get;
        } = new List<object>();

        public bool IsNullArray
        {
            get;
            private set;
        }

        public void ReadValue(TypeSignature valueType)
        {
            if (valueType.IsTypeOf("System", "Type"))
            {
                Elements.Add(TypeNameParser.Parse(_parentModule, _reader.ReadSerString()));
                return;
            }

            switch (valueType.ElementType)
            {
                case ElementType.Boolean:
                    Elements.Add(_reader.ReadByte() == 1);
                    break;

                case ElementType.Char:
                    Elements.Add((char) _reader.ReadUInt16());
                    break;

                case ElementType.R4:
                    Elements.Add(_reader.ReadSingle());
                    break;

                case ElementType.R8:
                    Elements.Add(_reader.ReadDouble());
                    break;

                case ElementType.I1:
                    Elements.Add(_reader.ReadSByte());
                    break;

                case ElementType.I2:
                    Elements.Add(_reader.ReadInt16());
                    break;

                case ElementType.I4:
                    Elements.Add(_reader.ReadInt32());
                    break;

                case ElementType.I8:
                    Elements.Add(_reader.ReadInt64());
                    break;

                case ElementType.U1:
                    Elements.Add(_reader.ReadByte());
                    break;

                case ElementType.U2:
                    Elements.Add(_reader.ReadUInt16());
                    break;

                case ElementType.U4:
                    Elements.Add(_reader.ReadUInt32());
                    break;

                case ElementType.U8:
                    Elements.Add(_reader.ReadUInt64());
                    break;

                case ElementType.String:
                    Elements.Add(_reader.ReadSerString());
                    break;

                case ElementType.Object:
                    var reader = new CustomAttributeArgumentReader(_parentModule, _reader);
                    var type = TypeSignature.ReadFieldOrPropType(_parentModule, _reader);
                    reader.ReadValue(type);
                    Elements.Add(new BoxedArgument(type, type.ElementType == ElementType.SzArray
                        ? reader.Elements.ToArray()
                        : reader.Elements[0]));
                    break;

                case ElementType.SzArray:
                    var arrayElementType = ((SzArrayTypeSignature) valueType).BaseType;
                    uint elementCount = _reader.CanRead(sizeof(uint)) ? _reader.ReadUInt32() : uint.MaxValue;
                    IsNullArray = elementCount == uint.MaxValue;

                    if (!IsNullArray)
                    {
                        for (uint i = 0; i < elementCount; i++)
                            ReadValue(arrayElementType);
                    }

                    break;

                case ElementType.Class:
                case ElementType.Enum:
                case ElementType.ValueType:
                    // Value is an enum, resolve it and get underlying type.
                    // If that fails, most enums are int32s, assume that is the case in an attempt to recover.
                    
                    var enumTypeDef = _parentModule.MetadataResolver.ResolveType(valueType);
                    
                    TypeSignature underlyingType;
                    if (enumTypeDef is null) 
                        underlyingType = _parentModule.CorLibTypeFactory.Int32;
                    else if (enumTypeDef.IsEnum)
                        underlyingType = enumTypeDef.GetEnumUnderlyingType() ?? _parentModule.CorLibTypeFactory.Int32;
                    else
                        throw new NotSupportedException($"Type {valueType} is not an enum.");

                    ReadValue(underlyingType);
                    break;

                default:
                    throw new NotSupportedException($"Unsupported element type {valueType.ElementType}.");
            }

        }

    }
}