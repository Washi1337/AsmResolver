using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.DotNet.Signatures.Types.Parsing;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures
{
    // src/coreclr/src/vm/customattribute.cpp
    
    internal sealed class CustomAttributeArgumentReader
    {
        private readonly BlobReadContext _context;
        private readonly IBinaryStreamReader _reader;

        public CustomAttributeArgumentReader(in BlobReadContext context, IBinaryStreamReader reader)
        {
            _context = context;
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
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
            var module = _context.ReadContext.ParentModule;
            
            if (valueType.IsTypeOf("System", "Type"))
            {
                Elements.Add(TypeNameParser.Parse(module, _reader.ReadSerString()));
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
                    var reader = new CustomAttributeArgumentReader(_context, _reader);
                    var type = TypeSignature.ReadFieldOrPropType(_context, _reader);
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
                    
                    var enumTypeDef = module.MetadataResolver.ResolveType(valueType);

                    TypeSignature underlyingType = null;
                    if (enumTypeDef != null && enumTypeDef.IsEnum)
                        underlyingType = enumTypeDef.GetEnumUnderlyingType();

                    if (underlyingType is null)
                    {
                        _context.ReadContext.BadImage($"Underlying enum type {valueType} could not be resolved. Assuming System.Int32 for custom attribute argument.");
                        underlyingType = module.CorLibTypeFactory.Int32;
                    }
                    
                    ReadValue(underlyingType);
                    break;

                default:
                    _context.ReadContext.NotSupported($"Unsupported element type {valueType.ElementType} in custom attribute argument.");
                    Elements.Add(null);
                    break;
            }

        }

    }
}