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

        public void WriteElements(TypeSignature argumentType, TypeSignature elementsType, IList<object> elements, bool isNullArray)
        {
            var writer = _context.Writer;
            
            if (elementsType.IsTypeOf("System", "Type"))
            {
                writer.WriteSerString(TypeNameBuilder.GetAssemblyQualifiedName((TypeSignature) elements[0]));
                return;
            }

            switch (elementsType.ElementType)
            {
                case ElementType.Boolean:
                    writer.WriteByte((byte) ((bool) elements[0] ? 1 : 0));
                    break;
                case ElementType.Char:
                    writer.WriteUInt16((char) elements[0]);
                    break;
                case ElementType.I1:
                    writer.WriteSByte((sbyte) elements[0]);
                    break;
                case ElementType.U1:
                    writer.WriteByte((byte) elements[0]);
                    break;
                case ElementType.I2:
                    writer.WriteInt16((short) elements[0]);
                    break;
                case ElementType.U2:
                    writer.WriteUInt16((ushort) elements[0]);
                    break;
                case ElementType.I4:
                    writer.WriteInt32((int) elements[0]);
                    break;
                case ElementType.U4:
                    writer.WriteUInt32((uint) elements[0]);
                    break;
                case ElementType.I8:
                    writer.WriteInt64((long) elements[0]);
                    break;
                case ElementType.U8:
                    writer.WriteUInt64((ulong) elements[0]);
                    break;
                case ElementType.R4:
                    writer.WriteSingle((float) elements[0]);
                    break;
                case ElementType.R8:
                    writer.WriteDouble((double) elements[0]);
                    break;
                case ElementType.String:
                    writer.WriteSerString(elements[0] as string);
                    break;
                case ElementType.Object:
                    TypeSignature innerTypeSig;

                    if (isNullArray)
                    {
                        innerTypeSig = argumentType.Module.CorLibTypeFactory.String;
                    }
                    else if (elements.Count > 0)
                    {
                        if (elements[0] is TypeSignature _)
                        {
                            innerTypeSig = new TypeDefOrRefSignature(
                                new TypeReference(argumentType.Module.CorLibTypeFactory.CorLibScope, "System", "Type"));
                        }
                        else if (elements[0] is null)
                        {
                            innerTypeSig = argumentType.Module.CorLibTypeFactory.String;
                        }
                        else
                        {
                            var valueType = elements[0].GetType();
                            innerTypeSig = argumentType.Module.CorLibTypeFactory
                                .FromName(valueType.Namespace, valueType.Name);
                        }
                    }
                    else
                    {
                        throw new NotSupportedException(
                            $"Expected the custom attribute argument of type {argumentType} to have at least one element.");
                    }

                    TypeSignature.WriteFieldOrPropType(writer, innerTypeSig);
                    WriteElements(argumentType, innerTypeSig, elements, isNullArray);
                    break;
                
                case ElementType.SzArray:
                    if (isNullArray)
                    {
                        writer.WriteUInt32(uint.MaxValue);
                        return;
                    }

                    var szArrayType = (SzArrayTypeSignature) argumentType;
                    var elementType = szArrayType.BaseType;
                    
                    writer.WriteUInt32((uint) elements.Count);
                    for (int i = 0; i < elements.Count; i++)
                        WriteElements(argumentType, elementType, new[] {elements[i]}, false);
                    break;
                    
                case ElementType.Class:
                case ElementType.Enum:
                case ElementType.ValueType:
                    var enumTypeDef = argumentType.Resolve();
                    if (enumTypeDef != null && enumTypeDef.IsEnum)
                        WriteElements(argumentType, enumTypeDef.GetEnumUnderlyingType(), elements, isNullArray);
                    else
                        throw new NotImplementedException();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}