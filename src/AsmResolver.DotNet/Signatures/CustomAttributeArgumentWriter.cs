using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.DotNet.Signatures.Types.Parsing;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures
{
    internal sealed class CustomAttributeArgumentWriter
    {
        private readonly BlobSerializationContext _context;

        public CustomAttributeArgumentWriter(BlobSerializationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void WriteArgument(CustomAttributeArgument argument)
        {
            if (argument.ArgumentType.ElementType != ElementType.SzArray)
            {
                WriteElement(argument.ArgumentType, argument.Element);
            }
            else
            {
                WriteArrayElement((SzArrayTypeSignature) argument.ArgumentType, argument.Elements, argument.IsNullArray);
            }
        }

        private void WriteArrayElement(SzArrayTypeSignature szArrayType, IList<object> elements, bool isNullArray)
        {
            var writer = _context.Writer;
            if (isNullArray)
            {
                writer.WriteUInt32(uint.MaxValue);
                return;
            }

            var elementType = szArrayType.BaseType;

            writer.WriteUInt32((uint) elements.Count);
            for (int i = 0; i < elements.Count; i++)
                WriteElement(elementType, elements[i]);
        }

        private void WriteElement(TypeSignature argumentType, object element)
        {
            var writer = _context.Writer;
            
            if (argumentType.IsTypeOf("System", "Type"))
            {
                writer.WriteSerString(TypeNameBuilder.GetAssemblyQualifiedName((TypeSignature) element));
                return;
            }

            switch (argumentType.ElementType)
            {
                case ElementType.Boolean:
                    writer.WriteByte((byte) ((bool) element ? 1 : 0));
                    break;
                case ElementType.Char:
                    writer.WriteUInt16((char) element);
                    break;
                case ElementType.I1:
                    writer.WriteSByte((sbyte) element);
                    break;
                case ElementType.U1:
                    writer.WriteByte((byte) element);
                    break;
                case ElementType.I2:
                    writer.WriteInt16((short) element);
                    break;
                case ElementType.U2:
                    writer.WriteUInt16((ushort) element);
                    break;
                case ElementType.I4:
                    writer.WriteInt32((int) element);
                    break;
                case ElementType.U4:
                    writer.WriteUInt32((uint) element);
                    break;
                case ElementType.I8:
                    writer.WriteInt64((long) element);
                    break;
                case ElementType.U8:
                    writer.WriteUInt64((ulong) element);
                    break;
                case ElementType.R4:
                    writer.WriteSingle((float) element);
                    break;
                case ElementType.R8:
                    writer.WriteDouble((double) element);
                    break;
                case ElementType.String:
                    writer.WriteSerString(element as string);
                    break;
                case ElementType.Object:
                    TypeSignature innerTypeSig;
                    object value = null;

                    if (element is null)
                    {
                        innerTypeSig = argumentType.Module.CorLibTypeFactory.String;
                    }
                    else if (element is BoxedArgument boxedArgument)
                    {
                        innerTypeSig = boxedArgument.Type;
                        value = boxedArgument.Value;
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                    
                    TypeSignature.WriteFieldOrPropType(writer, innerTypeSig);
                    WriteElement(innerTypeSig, value);
                    break;
                
                case ElementType.SzArray:
                    WriteArrayElement(
                        (SzArrayTypeSignature) argumentType,
                        element as IList<object> ?? new object[0],
                        element == null);
                    break;
                    
                case ElementType.Class:
                case ElementType.Enum:
                case ElementType.ValueType:
                    var enumTypeDef = argumentType.Resolve();
                    if (enumTypeDef != null && enumTypeDef.IsEnum)
                        WriteElement(enumTypeDef.GetEnumUnderlyingType(), element);
                    else
                        throw new NotImplementedException();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}