using System;
using AsmResolver.DotNet.Builder;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Represents a single element of a custom attribute argument. 
    /// </summary>
    public readonly struct CustomAttributeArgumentElement
    {
          /// <summary>
        /// Reads a single element at the current position of the provided stream reader.
        /// </summary>
        /// <param name="parentModule">The image the custom attribute is defined in.</param>
        /// <param name="typeSignature">The type of the element to read.</param>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The read element.</returns>
        public static CustomAttributeArgumentElement FromReader(ModuleDefinition parentModule, TypeSignature typeSignature, IBinaryStreamReader reader)
        {
            return new CustomAttributeArgumentElement(ReadValue(parentModule, typeSignature, reader));
        }

        private static object ReadValue(ModuleDefinition parentModule, TypeSignature valueType, IBinaryStreamReader reader)
        {
            switch (valueType.ElementType)
            {
                case ElementType.Boolean:
                    return reader.ReadByte() == 1;
                case ElementType.Char:
                    return (char)reader.ReadUInt16();
                case ElementType.R4:
                    return reader.ReadSingle();
                case ElementType.R8:
                    return reader.ReadDouble();
                case ElementType.I1:
                    return reader.ReadSByte();
                case ElementType.I2:
                    return reader.ReadInt16();
                case ElementType.I4:
                    return reader.ReadInt32();
                case ElementType.I8:
                    return reader.ReadInt64();
                case ElementType.U1:
                    return reader.ReadByte();
                case ElementType.U2:
                    return reader.ReadUInt16();
                case ElementType.U4:
                    return reader.ReadUInt32();
                case ElementType.U8:
                    return reader.ReadUInt64();
                case ElementType.String:
                    return reader.ReadSerString();
                case ElementType.Object:
                    return ReadValue(parentModule, TypeSignature.ReadFieldOrPropType(parentModule, reader), reader);
                case ElementType.Class:
                case ElementType.Enum:
                case ElementType.ValueType:
                    var enumTypeDef = parentModule.MetadataResolver.ResolveType(valueType);
                    if (enumTypeDef != null && enumTypeDef.IsEnum)
                        return ReadValue(parentModule, enumTypeDef.GetEnumUnderlyingType(), reader);
                    break;
            }
            
            if (valueType.IsTypeOf("System", "Type"))
                return TypeNameParser.ParseType(parentModule, reader.ReadSerString());

            throw new NotSupportedException($"Unsupported element type {valueType.ElementType}.");
        }

        /// <summary>
        /// Creates a new element for a custom attribute argument.
        /// </summary>
        /// <param name="value"></param>
        public CustomAttributeArgumentElement(object value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets or sets the value of the argument element.
        /// </summary>
        public object Value
        {
            get;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Value?.ToString() ?? "null";
        }

        /// <summary>
        /// Writes the named argument to the provided output stream.
        /// </summary>
        /// <param name="writer">The output stream.</param>
        /// <param name="argumentType">The type of the argument.</param>
        /// <param name="provider">The object to use for obtaining metadata tokens for members in the tables stream.</param>
        public void Write(IBinaryStreamWriter writer, TypeSignature argumentType, ITypeCodedIndexProvider provider)
        {
            WriteValue(writer, argumentType, provider, Value);
        }

        private void WriteValue(IBinaryStreamWriter writer, TypeSignature argumentType, ITypeCodedIndexProvider provider, object value)
        {
            if (argumentType.IsTypeOf("System", "Type"))
            {
                writer.WriteSerString(TypeNameBuilder.GetAssemblyQualifiedName((TypeSignature) value));
                return;
            }

            switch (argumentType.ElementType)
            {
                case ElementType.Boolean:
                    writer.WriteByte((byte) ((bool) value ? 1 : 0));
                    break;
                case ElementType.Char:
                    writer.WriteUInt16((char) value);
                    break;
                case ElementType.I1:
                    writer.WriteSByte((sbyte) value);
                    break;
                case ElementType.U1:
                    writer.WriteByte((byte) value);
                    break;
                case ElementType.I2:
                    writer.WriteInt16((short) value);
                    break;
                case ElementType.U2:
                    writer.WriteUInt16((ushort) value);
                    break;
                case ElementType.I4:
                    writer.WriteInt32((int) value);
                    break;
                case ElementType.U4:
                    writer.WriteUInt32((uint) value);
                    break;
                case ElementType.I8:
                    writer.WriteInt64((long) value);
                    break;
                case ElementType.U8:
                    writer.WriteUInt64((ulong) value);
                    break;
                case ElementType.R4:
                    writer.WriteSingle((float) value);
                    break;
                case ElementType.R8:
                    writer.WriteDouble((double) value);
                    break;
                case ElementType.String:
                    writer.WriteSerString(value as string);
                    break;
                case ElementType.Object:
                    throw new NotImplementedException();
                    
                case ElementType.Class:
                case ElementType.Enum:
                case ElementType.ValueType:
                    var enumTypeDef = argumentType.Resolve();
                    if (enumTypeDef != null && enumTypeDef.IsEnum)
                        WriteValue(writer, enumTypeDef.GetEnumUnderlyingType(), provider, Value);
                    else
                        throw new NotImplementedException();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}