using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.DotNet.Signatures.Types.Parsing;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Signatures
{
    internal sealed class CustomAttributeArgumentWriter
    {
        private readonly BlobSerializationContext _context;

        public CustomAttributeArgumentWriter(BlobSerializationContext context)
        {
            _context = context;
        }

        public void WriteArgument(CustomAttributeArgument argument)
        {
            var argumentType = argument.ArgumentType;
            if (argumentType.ElementType != ElementType.SzArray)
                WriteElement(argumentType, argument.Element);
            else
                WriteArrayElement((SzArrayTypeSignature) argumentType, argument.Elements, argument.IsNullArray);
        }

        private void WriteArrayElement(SzArrayTypeSignature szArrayType, IList<object?> elements, bool isNullArray)
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

        private void WriteElement(TypeSignature argumentType, object? element)
        {
            if (element is null)
                WriteNullElement(argumentType);
            else
                WriteNonNullElement(argumentType, element);
        }

        private void WriteNullElement(TypeSignature argumentType)
        {
            var writer = _context.Writer;

            if (argumentType.IsTypeOf("System", "Type"))
            {
                writer.WriteSerString(default(string));
                return;
            }

            switch (argumentType.ElementType)
            {
                case ElementType.String:
                    writer.WriteSerString(default(Utf8String));
                    break;

                case ElementType.Object:
                    // Most efficient way to store "null" is writing null as a string (two bytes).
                    TypeSignature.WriteFieldOrPropType(_context, argumentType.Module!.CorLibTypeFactory.String);
                    break;

                case ElementType.SzArray:
                    WriteArrayElement((SzArrayTypeSignature) argumentType, Array.Empty<object>(), true);
                    break;

                case ElementType.Class:
                case ElementType.Enum:
                case ElementType.ValueType:
                    WriteEnumValue(argumentType, null);
                    break;

                default:
                    _context.ErrorListener.NotSupported("Cannot write a null value for a non-nullable argument type.");
                    break;
            }
        }

        private void WriteNonNullElement(TypeSignature argumentType, object element)
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

                case ElementType.String when element is string s:
                    writer.WriteSerString(s);
                    break;

                case ElementType.String when element is Utf8String s:
                    writer.WriteSerString(s);
                    break;

                case ElementType.Object:
                    TypeSignature innerTypeSig;
                    object? value;

                    if (element is BoxedArgument boxedArgument)
                    {
                        // Write the boxed argument.
                        innerTypeSig = boxedArgument.Type;
                        value = boxedArgument.Value;
                    }
                    else
                    {
                        _context.ErrorListener.NotSupported(
                            $"Object elements in a custom attribute signature should be either 'null' or an instance of {nameof(BoxedArgument)}.");

                        // Write null as a recovery.
                        innerTypeSig = argumentType.Module!.CorLibTypeFactory.String;
                        value = null;
                    }

                    TypeSignature.WriteFieldOrPropType(_context, innerTypeSig);
                    WriteElement(innerTypeSig, value);
                    break;

                case ElementType.SzArray:
                    WriteArrayElement(
                        (SzArrayTypeSignature) argumentType,
                        element as IList<object?> ?? Array.Empty<object?>(),
                        false);
                    break;

                case ElementType.Class:
                case ElementType.Enum:
                case ElementType.ValueType:
                    WriteEnumValue(argumentType, element);
                    break;

                default:
                    _context.ErrorListener.NotSupported($"Invalid or unsupported argument type {argumentType.FullName}.");
                    break;
            }
        }

        private void WriteEnumValue(TypeSignature argumentType, object? element)
        {
            // Try resolve enum and get enum underlying type.
            var enumTypeDef = argumentType.Resolve();
            if (enumTypeDef is {IsEnum: true} && enumTypeDef.GetEnumUnderlyingType() is { } underlyingType)
            {
                WriteElement(underlyingType, element);
                return;
            }

            // Enum arguments can never be null.
            if (element is null)
            {
                _context.ErrorListener.RegisterException(
                    new NotSupportedException($"The element of the enum-typed argument is null."));

                // Assume 0 if it is.
                element = 0;
            }

            // Try inferring the enum type from the argument value.
            var corLibTypeFactory = argumentType.Module!.CorLibTypeFactory;
            var elementType = Type.GetTypeCode(element.GetType()) switch
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
                _ => _context.ErrorListener.NotSupportedAndReturn<TypeSignature>(
                    $"Type {argumentType.SafeToString()} is a non-integer enum type.")
            };

            if (elementType is not null)
                WriteElement(elementType, element);
        }
    }
}
