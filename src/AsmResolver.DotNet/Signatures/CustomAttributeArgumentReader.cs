using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.DotNet.Signatures.Types.Parsing;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures
{
    // src/coreclr/src/vm/customattribute.cpp

    internal struct CustomAttributeArgumentReader
    {
        public static CustomAttributeArgumentReader Create() => new(new List<object?>());

        public CustomAttributeArgumentReader(IList<object?> elements)
        {
            Elements = elements;
            IsNullArray = false;
        }

        public IList<object?> Elements
        {
            get;
        }

        public bool IsNullArray
        {
            get;
            private set;
        }

        public void ReadValue(in BlobReadContext context, ref BinaryStreamReader reader, TypeSignature valueType)
        {
            var module = context.ReaderContext.ParentModule;

            if (valueType.IsTypeOf("System", "Type"))
            {
                string? typeFullName = reader.ReadSerString();
                var type = typeFullName is not null
                    ? TypeNameParser.Parse(module, typeFullName)
                    : null;

                Elements.Add(type);
                return;
            }

            switch (valueType.ElementType)
            {
                case ElementType.Boolean:
                    Elements.Add(reader.ReadByte() == 1);
                    break;

                case ElementType.Char:
                    Elements.Add((char) reader.ReadUInt16());
                    break;

                case ElementType.R4:
                    Elements.Add(reader.ReadSingle());
                    break;

                case ElementType.R8:
                    Elements.Add(reader.ReadDouble());
                    break;

                case ElementType.I1:
                    Elements.Add(reader.ReadSByte());
                    break;

                case ElementType.I2:
                    Elements.Add(reader.ReadInt16());
                    break;

                case ElementType.I4:
                    Elements.Add(reader.ReadInt32());
                    break;

                case ElementType.I8:
                    Elements.Add(reader.ReadInt64());
                    break;

                case ElementType.U1:
                    Elements.Add(reader.ReadByte());
                    break;

                case ElementType.U2:
                    Elements.Add(reader.ReadUInt16());
                    break;

                case ElementType.U4:
                    Elements.Add(reader.ReadUInt32());
                    break;

                case ElementType.U8:
                    Elements.Add(reader.ReadUInt64());
                    break;

                case ElementType.String:
                    Elements.Add(reader.ReadSerString());
                    break;

                case ElementType.Object:
                    var type = TypeSignature.ReadFieldOrPropType(context, ref reader);

                    var subReader = Create();
                    subReader.ReadValue(context, ref reader, type);
                    Elements.Add(new BoxedArgument(type, type.ElementType == ElementType.SzArray
                        ? subReader.Elements.ToArray()
                        : subReader.Elements[0]));
                    break;

                case ElementType.SzArray:
                    var arrayElementType = ((SzArrayTypeSignature) valueType).BaseType;
                    uint elementCount = reader.CanRead(sizeof(uint)) ? reader.ReadUInt32() : uint.MaxValue;
                    IsNullArray = elementCount == uint.MaxValue;

                    if (!IsNullArray)
                    {
                        for (uint i = 0; i < elementCount; i++)
                            ReadValue(context, ref reader, arrayElementType);
                    }

                    break;

                case ElementType.Class:
                case ElementType.Enum:
                case ElementType.ValueType:
                    // Value is an enum, resolve it and get underlying type.
                    // If that fails, most enums are int32s, assume that is the case in an attempt to recover.

                    var enumTypeDef = module.MetadataResolver.ResolveType(valueType);

                    TypeSignature? underlyingType = null;
                    if (enumTypeDef is {IsEnum: true})
                        underlyingType = enumTypeDef.GetEnumUnderlyingType();

                    if (underlyingType is null)
                    {
                        context.ReaderContext.BadImage($"Underlying enum type {valueType} could not be resolved. Assuming System.Int32 for custom attribute argument.");
                        underlyingType = module.CorLibTypeFactory.Int32;
                    }

                    ReadValue(context, ref reader, underlyingType);
                    break;

                default:
                    context.ReaderContext.NotSupported($"Unsupported element type {valueType.ElementType} in custom attribute argument.");
                    Elements.Add(null);
                    break;
            }

        }

    }
}
