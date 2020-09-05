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
            var argumentType = argument.ArgumentType;
            if (argumentType.ElementType != ElementType.SzArray)
                WriteElement(argumentType, argument.Element);
            else
                WriteArrayElement((SzArrayTypeSignature) argumentType, argument.Elements, argument.IsNullArray);
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
                    object value;

                    if (element is null)
                    {
                        // Most efficient way to store "null" is writing null as a string (two bytes).
                        innerTypeSig = argumentType.Module.CorLibTypeFactory.String;
                        value = null;
                    }
                    else if (element is BoxedArgument boxedArgument)
                    {
                        // Write the boxed argument.
                        innerTypeSig = boxedArgument.Type;
                        value = boxedArgument.Value;
                    }
                    else
                    {
                        _context.DiagnosticBag.RegisterException(new NotSupportedException(
                            $"Object elements in a custom attribute signature should be either 'null' or an instance of {nameof(BoxedArgument)}."));
                        
                        // Write null as a recovery.
                        innerTypeSig = argumentType.Module.CorLibTypeFactory.String;
                        value = null;
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
                    WriteEnumValue(argumentType, element);
                    break;
                
                default:
                    UnsupportedArgumentType(argumentType);
                    break;
            }
        }

        private void WriteEnumValue(TypeSignature argumentType, object element)
        {
            // Try resolve enum and get enum underlying type.
            var enumTypeDef = argumentType.Resolve();
            if (enumTypeDef != null && enumTypeDef.IsEnum)
            {
                WriteElement(enumTypeDef.GetEnumUnderlyingType(), element);
                return;
            }
            
            // Enum arguments can never be null.
            if (element is null)
            {
                _context.DiagnosticBag.RegisterException(
                    new NotSupportedException($"The element of the enum-typed argument is null."));
                
                // Assume 0 if it is.
                element = 0;
            }

            // Try inferring the enum type from the argument value.
            var corLibTypeFactory = argumentType.Module.CorLibTypeFactory;
            argumentType = Type.GetTypeCode(element.GetType()) switch
            {
                TypeCode.Boolean => corLibTypeFactory.Boolean,
                TypeCode.Byte => corLibTypeFactory.Byte,
                TypeCode.Char => corLibTypeFactory.Char,
                TypeCode.Int16 => corLibTypeFactory.Int16,
                TypeCode.Int32 => corLibTypeFactory.Int32,
                TypeCode.Int64 => corLibTypeFactory.Int64,
                TypeCode.SByte => corLibTypeFactory.SByte,
                TypeCode.UInt16 => corLibTypeFactory.UInt16,
                TypeCode.UInt32 => corLibTypeFactory.UInt32,
                TypeCode.UInt64 => corLibTypeFactory.UInt64,
                _ => _context.DiagnosticBag.RegisterExceptionAndReturnDefault<TypeSignature>(
                    new NotSupportedException($"The element of the enum-typed argument is not of an integral type."))
            };

            if (argumentType != null)
                WriteElement(argumentType, element);
        }

        private void UnsupportedArgumentType(TypeSignature argumentType)
        {
            _context.DiagnosticBag.RegisterException(
                new NotSupportedException($"Invalid or unsupported argument type {argumentType.FullName}."));
        }
    }
}